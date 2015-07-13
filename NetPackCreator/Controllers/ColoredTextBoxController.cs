using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

using NetPackCreator.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Controller of {_textBox.Name}")]
    internal class ColoredTextBoxController<T> : IColoredController
    {
        /// <summary></summary>
        private readonly ColoredTextBox _textBox;

        /// <summary></summary>
        private readonly Func<T, string> _toStringFunc;

        /// <summary></summary>
        private T _value;

        /// <summary></summary>
        private States _state;

        /// <summary></summary>
        private bool _textChangeInProcess;

        /// <summary>http://stackoverflow.com/questions/5737959/restricting-a-generic-to-things-that-can-be-null</summary>
        static ColoredTextBoxController()
        {
            var def = default(T);
            if (def is ValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException(string.Format("Cannot instantiate with non-nullable type: {0}", typeof(T)));
            }
        }

        /// <summary></summary>
        /// <param name="textBox"></param>
        /// <param name="helper"></param>
        /// <param name="parseFunc"></param>
        /// <param name="validationFunc"></param>
        /// <param name="toStringFunc"></param>
        public ColoredTextBoxController(ColoredTextBox textBox, TextBoxChangeCanceledHelper helper, Func<string, T> parseFunc, Func<string, bool> validationFunc, Func<T, string> toStringFunc)
        {
            if (textBox == null) throw new ArgumentNullException("textBox");
            if (parseFunc == null) throw new ArgumentNullException("parseFunc");

            this._textBox = textBox;
            this._toStringFunc = toStringFunc;

            textBox.SetColoredController(this);

            helper.ChangeCanceled += (sender, e) => this.OnNeedBlinkBackground(Color.FromArgb(255, 255, 0, 0));

            textBox.TextChanged += (sender, e) =>
            {
                if (!this._textChangeInProcess)
                {
                    var text = this._textBox.Text;

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        if (this._state != States.Empty) this.ClearValueInternal(States.Empty);
                    }
                    else
                    {
                        if (validationFunc != null && !validationFunc(text))
                        {
                            this.ClearValueInternal(States.Incomplete);
                        }
                        else
                        {
                            try
                            {
                                this.SetValueInternal(parseFunc(text));

                                this.OnNeedBlinkBackground(Color.FromArgb(255, 0, 255, 0));
                            }
                            catch (Exception ex)
                            {
                                Debug.Fail(string.Format("Exception while parsing value: \"{0}\". Exception: \"{1}\"", text, ex.Message));

                                throw;
                            }
                        }
                    }
                }
            };
        }

        /// <summary></summary>
        public T Value { get { return this._value; } }

        /// <summary></summary>
        /// <param name="value"></param>
        public void SetValue(T value)
        {
            if (value == null) throw new ArgumentNullException("value");

            if (!value.Equals(this._value))
            {
                this._textChangeInProcess = true;

                this._textBox.Text = this._toStringFunc != null ? this._toStringFunc(value) : value.ToString();

                this._textChangeInProcess = false;

                this.SetValueInternal(value);
            }
        }

        /// <summary></summary>
        public void ClearValue()
        {
            if (this._state != States.Empty)
            {
                this._textChangeInProcess = true;

                this._textBox.Text = string.Empty;

                this._textChangeInProcess = false;

                this.ClearValueInternal(States.Empty);
            }
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private void SetValueInternal(T value)
        {
            this._state = States.Complete;

            if (!value.Equals(this._value))
            {
                this._value = value;

                this.OnValueChanged();
            }
        }

        /// <summary></summary>
        /// <param name="state"></param>
        private void ClearValueInternal(States state)
        {
            if (state == States.Complete) throw new InvalidOperationException();

            var needChangeValue = this._state == States.Complete;

            this._state = state;

            if (needChangeValue)
            {
                this._value = default(T);

                this.OnValueChanged();
            }
        }

        /// <summary></summary>
        private void ChangeToValid()
        {
            this.OnNeedChangeBackground(new Tuple<Color, Color>(Color.FromArgb(0xFF, 0x90, 0xEE, 0x90), Color.FromArgb(0xBB, 0x90, 0xEE, 0x90)));
        }

        /// <summary></summary>
        /// <param name="colors"></param>
        public void SetColor(Tuple<Color, Color> colors)
        {
            this.OnNeedChangeBackground(colors);
        }

        /// <summary></summary>
        public void ClearColor()
        {
            this.OnNeedClearBackground();
        }

        #region IColoredController

        /// <summary></summary>
        public event EventHandler<DataEventArgs<Tuple<Color, Color>>> NeedChangeBackground;
        /// <summary></summary>
        public event EventHandler<DataEventArgs<Color>> NeedBlinkBackground;
        /// <summary></summary>
        public event EventHandler NeedClearBackground;

        /// <summary></summary>
        /// <param name="colors"></param>
        private void OnNeedChangeBackground(Tuple<Color, Color> colors)
        {
            var handler = this.NeedChangeBackground;

            if (handler != null) handler(this, new DataEventArgs<Tuple<Color, Color>>(colors));
        }

        /// <summary></summary>
        /// <param name="color"></param>
        private void OnNeedBlinkBackground(Color color)
        {
            var handler = this.NeedBlinkBackground;

            if (handler != null) handler(this, new DataEventArgs<Color>(color));
        }

        /// <summary></summary>
        private void OnNeedClearBackground()
        {
            var handler = this.NeedClearBackground;

            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        /// <summary></summary>
        public event EventHandler ValueChanged;

        /// <summary></summary>
        private void OnValueChanged()
        {
            var handler = this.ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary></summary>
        private enum States
        {
            Empty,
            Incomplete,
            Complete
        }
    }
}