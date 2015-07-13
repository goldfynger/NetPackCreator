#define INTERFACECONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Controls;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal sealed class InterfaceController : INetworkInterfaceController
    {
        /// <summary></summary>
        private readonly ComboBox _comboBox;

        /// <summary></summary>
        private Dictionary<string, InterfaceWrapper> _availableInterfaces = new Dictionary<string, InterfaceWrapper>();

        /// <summary></summary>
        private InterfaceWrapper _generalInterface;

        /// <summary></summary>
        private bool _raiseEvent = false;

        /// <summary>Возникает, когда новое значение не равно предыдущему. Если равно (Equals), не возникнет, даже если произошло обновление содержимого (Update).</summary>
        public event EventHandler ValueChanged;

        /// <summary></summary>
        /// <param name="comboBox"></param>
        /// <param name="helper"></param>
        public InterfaceController(ComboBox comboBox, ComboBoxSelectionChangedHelper helper)
        {
            if (comboBox == null) throw new ArgumentNullException("comboBox");
            if (helper == null) throw new ArgumentNullException("helper");

            this._comboBox = comboBox;

            helper.SelectionChanged += (sender, e) =>
            {
                if (!this._raiseEvent) return;

                this.OnValueChanged();
            };

            this._raiseEvent = true;
        }

        /// <summary></summary>
        /// <param name="interfaceId"></param>
        public void UpdateInterfaces(string interfaceId)
        {
            this._raiseEvent = false;
            
            // Приоритет у интерфейса с переданным Id, если не был передан, то у выбранного до обновления.
            var interfaceAfterUpdate = interfaceId != null ? interfaceId : (this.HasValue ? this.CurrentValue.Id : null);

            this._comboBox.Items.Clear();

            this.UpdateInternal(interfaceId);

            this._raiseEvent = true;


            if (!(this.CurrentValue != null && this.CurrentValue.Id.Equals(interfaceId)))
            {
                this.OnValueChanged();
            }
        }

        /// <summary>Обновление состояния словаря и ComboBox'а. Запреты событий должны быть выше.</summary>
        /// <param name="interfaceId"></param>
        private void UpdateInternal(string interfaceId)
        {
            this.UpdateAvailableInterfaces();
            this.UpdateGeneralInterface();

            var counter = 0;

            var generalIndex = -1;
            var inputIndex = -1;

            foreach (var pair in this._availableInterfaces)
            {
                this._comboBox.Items.Add(pair.Value);

                // Приоритет у интерфейса с переданным Id (переданным для обновления, либо выбранный до обновления), если не был передан, или не найден, то приоритет у главного интерфейса системы.
                // Если приоритетный интерфейс не найден, то будет выбран первый (нулевой в zero-based) в списке.
                if (interfaceId != null && pair.Key == interfaceId) inputIndex = counter;
                if (pair.Value == this._generalInterface) generalIndex = counter;

                counter++;
            }

            if (this._comboBox.Items.Count != 0)
            {
                this._comboBox.SelectedIndex = inputIndex != -1 ? inputIndex : generalIndex;
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        private void UpdateAvailableInterfaces()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            this._availableInterfaces = interfaces
                .Where(i => (i.NetworkInterfaceType == NetworkInterfaceType.Ethernet || i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && i.OperationalStatus == OperationalStatus.Up)
                .ToDictionary(i => i.Id, i => new InterfaceWrapper(i));
        }

        /// <summary></summary>
        private void UpdateGeneralInterface()
        {
            using (var udpClient = new UdpClient("google.com", 80))
            {
                var localAddress = ((IPEndPoint)udpClient.Client.LocalEndPoint).Address;

                this._generalInterface = this._availableInterfaces.Values.FirstOrDefault(v => v.Interface.GetIPProperties().UnicastAddresses.Any(a => a.Address.Equals(localAddress)));
            }
        }

        /// <summary></summary>
        private void OnValueChanged()
        {

#if INTERFACECONTROLLER_SHOW_VALUE_CHANGED_EVENT_MSG

            Debug.Fail("InterfaceController.OnValueChanged");

#endif

            var handler = this.ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary></summary>
        public NetworkInterface CurrentValue
        {
            get
            {
                var wrapper = this._comboBox.SelectedItem as InterfaceWrapper;
                return wrapper == null ? null : wrapper.Interface;
            }
        }

        /// <summary></summary>
        public bool HasValue
        {
            get { return this.CurrentValue != null; }
        }

        /// <summary></summary>
        private sealed class InterfaceWrapper
        {
            /// <summary></summary>
            private readonly NetworkInterface _networkInterface;
            /// <summary></summary>
            private readonly string _description;

            /// <summary></summary>
            /// <param name="networkInterface"></param>
            public InterfaceWrapper(NetworkInterface networkInterface)
            {
                if (networkInterface == null) throw new ArgumentNullException("networkInterface");

                this._networkInterface = networkInterface;

                this._description = string.Format("{0} - {1}", networkInterface.Description, networkInterface.Name);
            }

            /// <summary></summary>
            public NetworkInterface Interface
            {
                get { return this._networkInterface; }
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