using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NetPackCreator.Controls
{
    /// <summary></summary>
    [DebuggerDisplay("Name = {Name}")]
    internal sealed class ColoredTextBox : TextBox
    {
        #region DependencyProperties

        /// <summary></summary>
        private static readonly DependencyPropertyKey _normalFocusedBackgroundPropertyKey =
            DependencyProperty.RegisterReadOnly("NormalFocusedBackground", typeof(Color), typeof(ColoredTextBox), new FrameworkPropertyMetadata(Colors.Transparent));
        /// <summary></summary>
        public static readonly DependencyProperty NormalFocusedBackgroundProperty = _normalFocusedBackgroundPropertyKey.DependencyProperty;

        /// <summary></summary>
        private static readonly DependencyPropertyKey _normalUnfocusedBackgroundPropertyKey =
            DependencyProperty.RegisterReadOnly("NormalUnfocusedBackground", typeof(Color), typeof(ColoredTextBox), new FrameworkPropertyMetadata(Colors.Transparent));
        /// <summary></summary>
        public static readonly DependencyProperty NormalUnfocusedBackgroundProperty = _normalUnfocusedBackgroundPropertyKey.DependencyProperty;


        /// <summary></summary>
        private static readonly DependencyPropertyKey _changedFocusedBackgroundPropertyKey =
            DependencyProperty.RegisterReadOnly("ChangedFocusedBackground", typeof(Color?), typeof(ColoredTextBox), new FrameworkPropertyMetadata((object)null));
        /// <summary></summary>
        public static readonly DependencyProperty ChangedFocusedBackgroundProperty = _changedFocusedBackgroundPropertyKey.DependencyProperty;

        /// <summary></summary>
        private static readonly DependencyPropertyKey _changedUnfocusedBackgroundPropertyKey =
            DependencyProperty.RegisterReadOnly("ChangedUnfocusedBackground", typeof(Color?), typeof(ColoredTextBox), new FrameworkPropertyMetadata((object)null));
        /// <summary></summary>
        public static readonly DependencyProperty ChangedUnfocusedBackgroundProperty = _changedUnfocusedBackgroundPropertyKey.DependencyProperty;


        /// <summary></summary>
        private static readonly DependencyPropertyKey _currentBackgroundFocusedPropertyKey =
            DependencyProperty.RegisterReadOnly("CurrentBackgroundFocused", typeof(Color), typeof(ColoredTextBox), new FrameworkPropertyMetadata(Colors.Transparent));
        /// <summary></summary>
        public static readonly DependencyProperty CurrentBackgroundFocusedProperty = _currentBackgroundFocusedPropertyKey.DependencyProperty;

        /// <summary></summary>
        private static readonly DependencyPropertyKey _currentBackgroundUnfocusedPropertyKey =
            DependencyProperty.RegisterReadOnly("CurrentBackgroundUnfocused", typeof(Color), typeof(ColoredTextBox), new FrameworkPropertyMetadata(Colors.Transparent));
        /// <summary></summary>
        public static readonly DependencyProperty CurrentBackgroundUnfocusedProperty = _currentBackgroundUnfocusedPropertyKey.DependencyProperty;

        /// <summary></summary>
        private static readonly DependencyPropertyKey _currentBackgroundPropertyKey =
            DependencyProperty.RegisterReadOnly("CurrentBackground", typeof(Color), typeof(ColoredTextBox), new FrameworkPropertyMetadata(Colors.Transparent));
        /// <summary></summary>
        public static readonly DependencyProperty CurrentBackgroundProperty = _currentBackgroundPropertyKey.DependencyProperty;

        /// <summary></summary>
        private static readonly DependencyProperty CurrentBackgroundAnimatableProperty =
            DependencyProperty.RegisterAttached("CurrentBackgroundAnimatable", typeof(Color), typeof(ColoredTextBox), new FrameworkPropertyMetadata((d, e) => d.SetValue(_currentBackgroundPropertyKey, e.NewValue)));

        #endregion

        /// <summary></summary>
        private IColoredController _coloredController;

        /// <summary></summary>
        public ColoredTextBox()
        {
            this.SetValue(_normalFocusedBackgroundPropertyKey, Colors.White);
            this.SetValue(_normalUnfocusedBackgroundPropertyKey, Color.FromRgb(0xF9, 0xF9, 0xF9));

            DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(ColoredTextBox)).AddValueChanged(this, (sender, e) => this.FillBackground());
            DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(ColoredTextBox)).AddValueChanged(this, (sender, e) => this.FillBackground());

            this.FillCurrentBackgroundFocused();
            this.FillCurrentBackgroundUnfocused();

            this.SetValue(CurrentBackgroundAnimatableProperty, (Color)this.GetValue(CurrentBackgroundUnfocusedProperty));
        }

        /// <summary></summary>
        private void FillCurrentBackgroundFocused()
        {
            var changedFocused = this.GetValue(ChangedFocusedBackgroundProperty) as Color?;

            this.SetValue(_currentBackgroundFocusedPropertyKey, changedFocused.HasValue ? changedFocused.Value : (Color)this.GetValue(NormalFocusedBackgroundProperty));
        }

        /// <summary></summary>
        private void FillCurrentBackgroundUnfocused()
        {
            var changedUnfocused = this.GetValue(ChangedUnfocusedBackgroundProperty) as Color?;

            this.SetValue(_currentBackgroundUnfocusedPropertyKey, changedUnfocused.HasValue ? changedUnfocused.Value : (Color)this.GetValue(NormalUnfocusedBackgroundProperty));
        }

        /// <summary></summary>
        /// <param name="isFocused"></param>
        private void FillBackground()
        {
            var newCurrentBackground = this.IsMouseOver || this.IsFocused ? (Color)this.GetValue(CurrentBackgroundFocusedProperty) : (Color)this.GetValue(CurrentBackgroundUnfocusedProperty);

            var toAnimation = new ColorAnimation { To = newCurrentBackground, Duration = new Duration(TimeSpan.FromMilliseconds(90)) };

            var storyboard = new Storyboard();
            storyboard.Children.Add(toAnimation);

            Storyboard.SetTarget(toAnimation, this);
            Storyboard.SetTargetProperty(toAnimation, new PropertyPath("CurrentBackgroundAnimatable"));

            storyboard.Begin();

            this.CaretBrush = Brushes.Black;
        }

        /// <summary></summary>
        /// <param name="color"></param>
        private void BlinkBackground(Color color)
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(200));

            var toAnimation = new ColorAnimation { To = color, Duration = duration };
            var fromAnimation = new ColorAnimation { From = color, Duration = duration };

            var storyboard = new Storyboard();
            storyboard.Children.Add(toAnimation);
            storyboard.Children.Add(fromAnimation);

            Storyboard.SetTarget(toAnimation, this);
            Storyboard.SetTarget(fromAnimation, this);
            Storyboard.SetTargetProperty(toAnimation, new PropertyPath("CurrentBackgroundAnimatable"));
            Storyboard.SetTargetProperty(fromAnimation, new PropertyPath("CurrentBackgroundAnimatable"));

            storyboard.Begin();
        }

        /// <summary></summary>
        /// <param name="controller"></param>
        public void SetColoredController(IColoredController controller)
        {
            if (this._coloredController != null)
            {
                this._coloredController.NeedChangeBackground -= NeedChangeColor;
                this._coloredController.NeedBlinkBackground -= NeedBlinkColor;
                this._coloredController.NeedClearBackground -= NeedClearColor;
            }

            this._coloredController = controller;

            if (this._coloredController != null)
            {
                this._coloredController.NeedChangeBackground += NeedChangeColor;
                this._coloredController.NeedBlinkBackground += NeedBlinkColor;
                this._coloredController.NeedClearBackground += NeedClearColor;
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="dataEventArgs"></param>
        private void NeedChangeColor(object sender, DataEventArgs<Tuple<Color, Color>> dataEventArgs)
        {
            var tuple = dataEventArgs.Value;
            
            this.SetValue(_changedFocusedBackgroundPropertyKey, tuple == null ? (Color?)null : tuple.Item1);
            this.SetValue(_changedUnfocusedBackgroundPropertyKey, tuple == null ? (Color?)null : tuple.Item2);

            this.FillCurrentBackgroundFocused();
            this.FillCurrentBackgroundUnfocused();

            this.FillBackground();
        }
        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="dataEventArgs"></param>
        private void NeedBlinkColor(object sender, DataEventArgs<Color> dataEventArgs)
        {
            this.BlinkBackground(dataEventArgs.Value);
        }
        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void NeedClearColor(object sender, EventArgs eventArgs)
        {
            this.NeedChangeColor(sender, new DataEventArgs<Tuple<Color, Color>>(null));
        }
    }

    /// <summary></summary>
    internal interface IColoredController
    {
        /// <summary></summary>
        event EventHandler<DataEventArgs<Tuple<Color, Color>>> NeedChangeBackground;
        /// <summary></summary>
        event EventHandler<DataEventArgs<Color>> NeedBlinkBackground;
        /// <summary></summary>
        event EventHandler NeedClearBackground;
    }
}