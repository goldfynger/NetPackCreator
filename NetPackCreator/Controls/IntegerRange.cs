using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetPackCreator.Controls
{
    /// <summary></summary>
    internal static class IntegerRange
    {
        /// <summary></summary>
        private static readonly DependencyPropertyKey _validationFuncPropertyKey = DependencyProperty.RegisterAttachedReadOnly("ValidationFunc", typeof(Func<int, bool>), typeof(IntegerRange), new FrameworkPropertyMetadata());

        /// <summary>Identifies the <see cref="Max"/> dependency property.</summary>
        public static readonly DependencyProperty MaxProperty = DependencyProperty.RegisterAttached("Max", typeof(int?), typeof(IntegerRange), new FrameworkPropertyMetadata(OnMaxChanged));

        /// <summary>Identifies the <see cref="Min"/> dependency property.</summary>
        public static readonly DependencyProperty MinProperty = DependencyProperty.RegisterAttached("Min", typeof(int?), typeof(IntegerRange), new FrameworkPropertyMetadata(OnMinChanged));

        /// <summary>Identifies the <see cref="ValidationFunc"/> dependency property.</summary>
        public static readonly DependencyProperty ValidationFuncProperty = _validationFuncPropertyKey.DependencyProperty;

        /// <summary></summary>
        public static readonly RoutedEvent ChangeCanceledEvent = EventManager.RegisterRoutedEvent("ChangeCanceled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntegerRange));

        /// <summary></summary>
        /// <param name="textBox"></param>
        private static void RaiseChangeCanceledEvent(TextBox textBox)
        {
            if (!textBox.IsReadOnly)
            {
                textBox.RaiseEvent(new RoutedEventArgs(ChangeCanceledEvent));
            }
        }

        /// <summary>Gets the max for a given <see cref="TextBox"/>.</summary>
        /// <param name="textBox">The <see cref="TextBox"/> whose max is to be retrieved.</param>
        /// <returns>The max, or <see langword="null"/> if no max has been set.</returns>
        public static int? GetMax(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(MaxProperty) as int?;
        }

        /// <summary>Sets the max for a given <see cref="TextBox"/>.</summary>
        /// <param name="textBox">The <see cref="TextBox"/> whose max is to be set.</param>
        /// <param name="max">The max to set, or <see langword="null"/> to remove any existing max from <paramref name="textBox"/>.</param>
        public static void SetMax(TextBox textBox, int max)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            textBox.SetValue(MaxProperty, max);
        }

        /// <summary>Gets the min for a given <see cref="TextBox"/>.</summary>
        /// <param name="textBox">The <see cref="TextBox"/> whose min is to be retrieved.</param>
        /// <returns>The min, or <see langword="null"/> if no min has been set.</returns>
        public static int? GetMin(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(MinProperty) as int?;
        }

        /// <summary>Sets the min for a given <see cref="TextBox"/>.</summary>
        /// <param name="textBox">The <see cref="TextBox"/> whose min is to be set.</param>
        /// <param name="min">The min to set, or <see langword="null"/> to remove any existing min from <paramref name="textBox"/>.</param>
        public static void SetMin(TextBox textBox, int min)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            textBox.SetValue(MinProperty, min);
        }

        /// <summary>Gets the validation func for the <see cref="TextBox"/>.</summary>
        /// <remarks>This method can be used to retrieve the actual <see cref="Func{int, bool}"/> instance created as a result of setting the min and max on a <see cref="TextBox"/>.</remarks>
        /// <param name="textBox">The <see cref="TextBox"/> whose mask expression is to be retrieved.</param>
        /// <returns>The mask expression as an instance of <see cref="Func{int, bool}"/>, or <see langword="null"/> if no max and min has been applied to <paramref name="textBox"/>.</returns>
        public static Func<int, bool> GetValidationFunc(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(ValidationFuncProperty) as Func<int, bool>;
        }

        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <param name="regex"></param>
        private static void SetValidationFunc(TextBox textBox, Func<int, bool> regex)
        {
            textBox.SetValue(_validationFuncPropertyKey, regex);
        }

        /// <summary></summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void OnMaxChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            var max = e.NewValue as int?;
            textBox.PreviewTextInput -= textBox_PreviewTextInput;
            textBox.PreviewKeyDown -= textBox_PreviewKeyDown;
            DataObject.RemovePastingHandler(textBox, Pasting);
            DataObject.RemoveCopyingHandler(textBox, NoDragCopy);
            CommandManager.RemovePreviewExecutedHandler(textBox, NoCutting);

            if (max == null)
            {
                textBox.ClearValue(MaxProperty);
            }
            else
            {
                textBox.SetValue(MaxProperty, max);
            }
            
            var min = textBox.GetValue(MinProperty) as int?;

            if (max == null && min == null)
            {
                textBox.ClearValue(ValidationFuncProperty);
            }
            else
            {
                Func<int, bool> func = (min != null && max != null) ? (i => i <= max && i >= min) : (min != null) ? (i => i >= min) : (Func<int, bool>)(i => i <= max);
                
                SetValidationFunc(textBox, func);

                textBox.PreviewTextInput += textBox_PreviewTextInput;
                textBox.PreviewKeyDown += textBox_PreviewKeyDown;
                DataObject.AddPastingHandler(textBox, Pasting);
                DataObject.AddCopyingHandler(textBox, NoDragCopy);
                CommandManager.AddPreviewExecutedHandler(textBox, NoCutting);
            }
        }       

        /// <summary></summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void OnMinChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            var min = e.NewValue as int?;
            textBox.PreviewTextInput -= textBox_PreviewTextInput;
            textBox.PreviewKeyDown -= textBox_PreviewKeyDown;
            DataObject.RemovePastingHandler(textBox, Pasting);
            DataObject.RemoveCopyingHandler(textBox, NoDragCopy);
            CommandManager.RemovePreviewExecutedHandler(textBox, NoCutting);

            if (min == null)
            {
                textBox.ClearValue(MinProperty);
            }
            else
            {
                textBox.SetValue(MinProperty, min);
            }

            var max = textBox.GetValue(MinProperty) as int?;

            if (max == null && min == null)
            {
                textBox.ClearValue(ValidationFuncProperty);
            }
            else
            {
                Func<int, bool> func = (min != null && max != null) ? (i => i <= max && i >= min) : (min != null) ? (i => i >= min) : (Func<int, bool>)(i => i <= max);

                SetValidationFunc(textBox, func);

                textBox.PreviewTextInput += textBox_PreviewTextInput;
                textBox.PreviewKeyDown += textBox_PreviewKeyDown;
                DataObject.AddPastingHandler(textBox, Pasting);
                DataObject.AddCopyingHandler(textBox, NoDragCopy);
                CommandManager.AddPreviewExecutedHandler(textBox, NoCutting);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void NoCutting(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;

                RaiseChangeCanceledEvent(sender as TextBox);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void NoDragCopy(object sender, DataObjectCopyingEventArgs e)
        {
            if (e.IsDragDrop)
            {
                e.CancelCommand();

                RaiseChangeCanceledEvent(sender as TextBox);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var func = GetValidationFunc(textBox);

            if (func == null)
            {
                return;
            }

            var proposedText = GetProposedText(textBox, e.Text);

            int integer;
            if (!int.TryParse(proposedText, out integer) || !func(integer) || (proposedText.Length > 1 && proposedText[0] == '0'))
            {
                e.Handled = true;

                RaiseChangeCanceledEvent(textBox);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var func = GetValidationFunc(textBox);

            if (func == null)
            {
                return;
            }

            string proposedText = null;

            // Pressing space doesn't raise PreviewTextInput, reasons here http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/446ec083-04c8-43f2-89dc-1e2521a31f6b?prof=required
            if (e.Key == Key.Space)
            {
                proposedText = GetProposedText(textBox, " ");
            }
            // Same story with backspace
            else if (e.Key == Key.Back )
            {
                proposedText = GetProposedTextBackspace(textBox);
            }
            // And Delete
            else if (e.Key == Key.Delete)
            {
                proposedText = GetProposedTextDelete(textBox);
            }

            int integer;
            if (proposedText != null && proposedText != string.Empty && (!int.TryParse(proposedText, out integer) || !func(integer) || (proposedText.Length > 1 && proposedText[0] == '0')))
            {
                e.Handled = true;

                RaiseChangeCanceledEvent(textBox);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            var func = GetValidationFunc(textBox);

            if (func == null)
            {
                return;
            }

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = e.DataObject.GetData(typeof(string)) as string;
                var proposedText = GetProposedText(textBox, pastedText);

                int integer;
                if (!int.TryParse(proposedText, out integer) || !func(integer) || (proposedText.Length > 1 && proposedText[0] == '0'))
                {
                    e.CancelCommand();

                    RaiseChangeCanceledEvent(textBox);
                }
            }
            else
            {
                e.CancelCommand();

                RaiseChangeCanceledEvent(textBox);
            }
        }

        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private static string GetProposedTextDelete(TextBox textBox)
        {
            var text = GetTextWithSelectionRemoved(textBox);
            if (textBox.SelectionStart < text.Length && textBox.SelectionLength == 0)
            {
                text = text.Remove(textBox.SelectionStart, 1);
            }

            return text;
        }

        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private static string GetProposedTextBackspace(TextBox textBox)
        {
            var text = GetTextWithSelectionRemoved(textBox);
            if (textBox.SelectionStart > 0 && textBox.SelectionLength == 0)
            {
                text = text.Remove(textBox.SelectionStart - 1, 1);
            }

            return text;
        }

        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        private static string GetProposedText(TextBox textBox, string newText)
        {
            var text = GetTextWithSelectionRemoved(textBox);
            text = text.Insert(textBox.CaretIndex, newText);

            return text;
        }

        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private static string GetTextWithSelectionRemoved(TextBox textBox)
        {
            var text = textBox.Text;

            if (textBox.SelectionStart != -1)
            {
                text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            }
            return text;
        }
    }
}