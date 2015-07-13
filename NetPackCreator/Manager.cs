using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

using NetPackCreator.Connections;
using NetPackCreator.Controllers;
using NetPackCreator.Extensions;

namespace NetPackCreator
{
    /// <summary></summary>
    internal sealed class Manager
    {
        /// <summary></summary>
        private Connection _connection;

        /// <summary></summary>
        private readonly SourceGroup _sourceGroup;
        /// <summary></summary>
        private readonly DestinationGroup _destinationGroup;
        /// <summary></summary>
        private readonly ExchangeGroup _exchangeGroup;

        /// <summary></summary>
        private readonly Dictionary<string, IPAddress> _currentAddressOfInterface = new Dictionary<string, IPAddress>();

        /// <summary></summary>
        /// <param name="sourceGroup"></param>
        /// <param name="destinationGroup"></param>
        public Manager(SourceGroup sourceGroup, DestinationGroup destinationGroup, ExchangeGroup exchangeGroup)
        {
            if (sourceGroup == null) throw new ArgumentNullException("sourceGroup");
            if (destinationGroup == null) throw new ArgumentNullException("destinationGroup");
            if (exchangeGroup == null) throw new ArgumentNullException("exchangeGroup");

            this._sourceGroup = sourceGroup;
            this._destinationGroup = destinationGroup;
            this._exchangeGroup = exchangeGroup;


            var interfaceController = this._sourceGroup.InterfaceController;
            var sourceMacController = this._sourceGroup.PhysicalAddressController;
            var sourceIpController = this._sourceGroup.SourceIpAddressController;
            var sourcePortController = this._sourceGroup.PortController;

            interfaceController.ValueChanged += (sender, e) =>
            {
                if (interfaceController.HasValue)
                {
                    var currentInterface = interfaceController.CurrentValue;

                    IPAddress currentAddressOfInterface = null;
                    this._currentAddressOfInterface.TryGetValue(currentInterface.Id, out currentAddressOfInterface);

                    sourceMacController.SetAddress(currentInterface.GetPhysicalAddress());
                    sourceIpController.SetAddressList(currentInterface.GetInterfaceIPv4UnicastAddresses(), currentAddressOfInterface);
                }
                else
                {
                    sourceMacController.ClearAddress();
                    sourceIpController.ClearAddressList();
                }
            };

            sourceIpController.ValueChanged += (sender, e) =>
            {
                var currentAddress = sourceIpController.CurrentValue;

                if (interfaceController.HasValue)
                {
                    var currentInterface = interfaceController.CurrentValue;

                    if (this._currentAddressOfInterface.ContainsKey(currentInterface.Id))
                    {
                        this._currentAddressOfInterface[currentInterface.Id] = currentAddress;
                    }
                    else
                    {
                        this._currentAddressOfInterface.Add(currentInterface.Id, currentAddress);
                    }
                }
            };

            interfaceController.UpdateInterfaces(null);


            var destinationMacController = destinationGroup.PhysicalAddressController;
            var destinationIpController = destinationGroup.DestinationIpAddressController;
            var destinationPortController = destinationGroup.PortController;

            var destinationMacResolver = new ParallelAwaitableWithResult<Tuple<IPAddress, IPAddress>, PhysicalAddress>(
                t => t.Item1.GetPhysicalAddressForIPv4(t.Item2),
                p => destinationMacController.SetAddress(p));

            destinationIpController.ValueChanged += (sender, e) =>
            {
                destinationMacController.ClearAddress();

                if (destinationIpController.HasValue)
                {
                    destinationMacResolver.Run(new Tuple<IPAddress, IPAddress>(destinationIpController.CurrentValue, sourceIpController.HasValue ? sourceIpController.CurrentValue : null));
                }
            };

            var exchangeConnectionModeController = exchangeGroup.ConnectionModeController;
            var exchangeConnectionStateController = exchangeGroup.ConnectionStateController;
            var sendCommandController = exchangeGroup.SendCommandController;

            EventHandler<DataEventArgs<byte[]>> receiveEventHandler = (sender, e) =>
            {
                var x = e.Value;
            };

            exchangeConnectionStateController.CommandServed += (sender, e) =>
            {
                switch (exchangeConnectionStateController.CurrentValue)
                {
                    case ConnectionState.Connected: 
                        exchangeConnectionStateController.SetValue(ConnectionState.Disconnected);
                        this._connection.DataReceived -= receiveEventHandler;
                        this._connection.Dispose();
                        this._connection = null;
                        break;

                    case ConnectionState.Disconnected:
                        exchangeConnectionStateController.SetValue(ConnectionState.Connected);
                        this._connection = Connection.CreateConnection(exchangeConnectionModeController.CurrentValue, new IPEndPoint(sourceIpController.CurrentValue, sourcePortController.CurrentValue.Value));
                        this._connection.DataReceived += receiveEventHandler;
                        this._connection.Connect(new IPEndPoint(destinationIpController.CurrentValue, destinationPortController.CurrentValue.Value));
                        break;
                }
            };

            Action<int> sendCompleteAction = i =>
            {
                var x = i;
            };

            sendCommandController.CommandServed += (sender, e) =>
            {
                this._connection.Send(new byte[] {1, 2, 3}, sendCompleteAction);
            };
        }
    }
}