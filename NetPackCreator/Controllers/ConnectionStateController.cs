#define CONNECTIONSTATECONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG
#define CONNECTIONSTATECONTROLLER_SHOW_COMMAND_SERVED_EVENT_MSG

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class ConnectionStateController : IConnectionStateController
    {
        /// <summary></summary>
        private readonly Button _button;

        /// <summary></summary>
        private ConnectionState _connectionState;

        /// <summary></summary>
        public event EventHandler ValueChanged;

        /// <summary></summary>
        public event EventHandler CommandServed;

        /// <summary></summary>
        private readonly string _connectedDescription =
            (typeof(ConnectionState).GetField(ConnectionState.Connected.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).First() as DescriptionAttribute).Description;
        /// <summary></summary>
        private readonly string _disconnectedDescription =
            (typeof(ConnectionState).GetField(ConnectionState.Disconnected.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).First() as DescriptionAttribute).Description;

        /// <summary></summary>
        private readonly string _connectionDescription =
            (typeof(ConnectionState).GetField(ConnectionState.ConnectionInProcess.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).First() as DescriptionAttribute).Description;
        /// <summary></summary>
        private readonly string _disconnectionDescription =
            (typeof(ConnectionState).GetField(ConnectionState.DisconnectionInProcess.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).First() as DescriptionAttribute).Description;

        /// <summary></summary>
        /// <param name="comboBox"></param>
        /// <param name="helper"></param>
        public ConnectionStateController(Button button)
        {
            if (button == null) throw new ArgumentNullException("button");

            this._button = button;

            this._connectionState = ConnectionState.Disconnected;
            this._button.Content = this._disconnectedDescription;

            button.Click += (sender, e) => this.OnCommandServed();
        }

        /// <summary></summary>
        private void OnValueChanged()
        {

#if CONNECTIONSTATECONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

            Debug.Fail("ConnectionStateController.OnValueChanged");

#endif

            var handler = this.ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary></summary>
        private void OnCommandServed()
        {

#if CONNECTIONSTATECONTROLLER_SHOW_COMMAND_SERVED_EVENT_MSG

            Debug.Fail("ConnectionStateController.OnCommandServed");

#endif

            var handler = this.CommandServed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary></summary>
        public ConnectionState CurrentValue
        {
            get { return this._connectionState; }
        }

        /// <summary></summary>
        /// <param name="state"></param>
        public void SetValue(ConnectionState state)
        {
            if (!this._connectionState.Equals(state))
            {
                switch (state)
                {
                    case ConnectionState.Disconnected :
                        this._button.IsEnabled = true;
                        this._button.Content = this._disconnectedDescription; break;

                    case ConnectionState.Connected:
                        this._button.IsEnabled = true;
                        this._button.Content = this._connectedDescription; break;

                    case ConnectionState.DisconnectionInProcess:
                        this._button.IsEnabled = false;
                        this._button.Content = this._disconnectionDescription; break;

                    case ConnectionState.ConnectionInProcess:
                        this._button.IsEnabled = false;
                        this._button.Content = this._connectionDescription; break;
                }

                this._connectionState = state;
                this.OnValueChanged();
            }
        }
    }
}