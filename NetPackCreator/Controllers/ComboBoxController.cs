#define COMBOBOXCONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal class ComboBoxController<T>
    {
        /// <summary></summary>
        private readonly ComboBox _comboBox;

        /// <summary></summary>
        private readonly Func<T, string> _toStringFunc;

        /// <summary>Возникает, когда новое значение не равно предыдущему. Если равно (Equals), не возникнет, даже если произошло обновление содержимого (FillValues или ClearValues).</summary>
        public event EventHandler ValueChanged;

        /// <summary></summary>
        private bool _raiseEvent = false;

        /// <summary>http://stackoverflow.com/questions/5737959/restricting-a-generic-to-things-that-can-be-null</summary>
        static ComboBoxController()
        {
            var def = default(T);
            if (def is ValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException(string.Format("Cannot instantiate with non-nullable type: {0}", typeof(T)));
            }
        }

        /// <summary></summary>
        /// <param name="comboBox"></param>
        /// <param name="helper"></param>
        /// <param name="values"></param>
        /// <param name="toStringFunc"></param>
        public ComboBoxController(ComboBox comboBox, ComboBoxSelectionChangedHelper helper, List<T> values, Func<T, string> toStringFunc)
        {
            if (comboBox == null) throw new ArgumentNullException("comboBox");
            if (helper == null) throw new ArgumentNullException("helper");

            this._comboBox = comboBox;
            this._toStringFunc = toStringFunc;

            if (values != null) this.FillValues(values, default(T));

            helper.SelectionChanged += (sender, e) =>
            {
                if (!this._raiseEvent) return;

                this.OnValueChanged();
            };

            this._raiseEvent = true;
        }

        /// <summary></summary>
        /// <param name="values"></param>
        /// <param name="valueForSelection"></param>
        public void FillValues(List<T> values, T valueForSelection)
        {
            if (values == null) throw new ArgumentNullException("values");
            if (!values.Any()) throw new ArgumentException("List must be non-empty.");

            var wrapperList = values.Select(v => new ValueWrapper(v, this._toStringFunc)).ToList();

            var oldSelectedValue = this.CurrentValue;


            this._raiseEvent = false;

            this._comboBox.Items.Clear();

            wrapperList.ForEach(v => this._comboBox.Items.Add(v));

            if (this._comboBox.Items.Count != 0)
            {
                this._comboBox.SelectedIndex = valueForSelection != null ? values.IndexOf(valueForSelection) : 0;
            }

            this._raiseEvent = true;


            if (oldSelectedValue == null || (oldSelectedValue != null && !oldSelectedValue.Equals(this.CurrentValue)))
            {
                this.OnValueChanged();
            }
        }

        /// <summary></summary>
        public void ClearValues()
        {
            var isOldSelectionEmpty = this._comboBox.SelectedItem == null;


            this._raiseEvent = false;

            this._comboBox.Items.Clear();

            this._raiseEvent = true;

            
            if (!isOldSelectionEmpty)
            {
                this.OnValueChanged();
            }
        }

        /// <summary></summary>
        private void OnValueChanged()
        {

#if COMBOBOXCONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

            Debug.Fail("ComboBoxController.OnValueChanged");

#endif

            var handler = this.ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary></summary>
        public T CurrentValue
        {
            get
            {
                var wrapper = this._comboBox.SelectedItem as ValueWrapper;
                return wrapper == null ? default(T) : wrapper.Value;
            }
        }

        /// <summary></summary>
        private sealed class ValueWrapper
        {
            /// <summary></summary>
            private readonly T _value;
            /// <summary></summary>
            private readonly string _description;

            /// <summary></summary>
            /// <param name="value"></param>
            /// <param name="toStringFunc"></param>
            public ValueWrapper(T value, Func<T, string> toStringFunc)
            {
                if (value == null) throw new ArgumentNullException("value");

                this._value = value;

                this._description = toStringFunc == null ? value.ToString() : toStringFunc(value);
            }

            /// <summary></summary>
            public T Value
            {
                get { return this._value; }
            }

            /// <summary></summary>
            /// <returns></returns>
            public override string ToString()
            {
                return this._description;
            }
        }
    }
}