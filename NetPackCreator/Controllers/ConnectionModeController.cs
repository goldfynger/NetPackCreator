#define CONNECTIONMODECONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class ConnectionModeController : IConnectionModeController
    {
        /// <summary></summary>
        private readonly ComboBox _comboBox;

        /// <summary></summary>
        public event EventHandler ValueChanged;

        /// <summary></summary>
        /// <param name="comboBox"></param>
        /// <param name="helper"></param>
        public ConnectionModeController(ComboBox comboBox, ComboBoxSelectionChangedHelper helper)
        {
            if (comboBox == null) throw new ArgumentNullException("comboBox");
            if (helper == null) throw new ArgumentNullException("helper");

            this._comboBox = comboBox;

            foreach (var connectionMode in Enum.GetValues(typeof(ConnectionMode)))
            {
                comboBox.Items.Add(new ConnectionModeWrapper((ConnectionMode)connectionMode));
            }

            comboBox.SelectedIndex = 0;

            helper.SelectionChanged += (sender, e) =>
            {
                this.OnValueChanged();
            };
        }

        /// <summary></summary>
        private void OnValueChanged()
        {

#if CONNECTIONMODECONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

            Debug.Fail("ConnectionModeController.OnValueChanged");

#endif

            var handler = this.ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary></summary>
        public ConnectionMode CurrentValue
        {
            get
            {
                return (this._comboBox.SelectedItem as ConnectionModeWrapper).ConnectionMode;
            }
        }

        /// <summary></summary>
        private sealed class ConnectionModeWrapper
        {
            /// <summary></summary>
            private readonly ConnectionMode _connectionMode;
            /// <summary></summary>
            private readonly string _description;

            /// <summary></summary>
            /// <param name="networkInterface"></param>
            public ConnectionModeWrapper(ConnectionMode connectionMode)
            {
                this._connectionMode = connectionMode;

                this._description = (typeof(ConnectionMode).GetField(connectionMode.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).First() as DescriptionAttribute).Description;
            }

            /// <summary></summary>
            public ConnectionMode ConnectionMode
            {
                get { return this._connectionMode; }
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