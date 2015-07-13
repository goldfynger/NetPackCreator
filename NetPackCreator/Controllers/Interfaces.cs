using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    internal interface INullableValueController<T>
    {
        /// <summary></summary>
        event EventHandler ValueChanged;

        /// <summary></summary>
        T CurrentValue { get; }

        /// <summary></summary>
        bool HasValue { get; }
    }

    /// <summary></summary>
    internal interface INetworkInterfaceController : INullableValueController<NetworkInterface>
    {
        /// <summary></summary>
        /// <param name="interfaceId"></param>
        void UpdateInterfaces(string interfaceId);
    }

    /// <summary></summary>
    internal interface IPhysicalAddressController : INullableValueController<PhysicalAddress>
    {
        /// <summary></summary>
        /// <param name="address"></param>
        void SetAddress(PhysicalAddress address);

        /// <summary></summary>
        void ClearAddress();
    }

    /// <summary></summary>
    internal interface ISourceIpAddressController : INullableValueController<IPAddress>
    {
        /// <summary></summary>
        /// <param name="list"></param>
        /// <param name="currentIpAddress"></param>
        void SetAddressList(List<IPAddress> list, IPAddress currentIpAddress);

        /// <summary></summary>
        void ClearAddressList();
    }

    /// <summary></summary>
    internal interface IDestinationIpAddressController : INullableValueController<IPAddress>
    {
        /// <summary></summary>
        /// <param name="address"></param>
        void SetAddress(IPAddress address);

        /// <summary></summary>
        void ClearAddress();
    }

    /// <summary></summary>
    internal interface IPortController : INullableValueController<ushort?>
    {
        /// <summary></summary>
        /// <param name="port"></param>
        void SetPort(ushort port);

        /// <summary></summary>
        void ClearPort();
    }

    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    internal interface IValueController<T>
    {
        /// <summary></summary>
        event EventHandler ValueChanged;

        /// <summary></summary>
        T CurrentValue { get; }
    }

    /// <summary></summary>
    internal interface IConnectionModeController : IValueController<ConnectionMode> { }

    /// <summary></summary>
    internal interface ICommandController
    {
        /// <summary></summary>
        event EventHandler CommandServed;
    }

    internal interface IConnectionStateController : ICommandController, IValueController<ConnectionState>
    {
        /// <summary></summary>
        /// <param name="state"></param>
        void SetValue(ConnectionState state);
    }

    /// <summary></summary>
    internal interface IWriteController
    {
        /// <summary></summary>
        /// <returns></returns>
        byte[] GetData();
    }

    /// <summary></summary>
    internal interface IExchangeViewController
    {
        /// <summary></summary>
        /// <param name="data"></param>
        /// <param name="info"></param>
        void AddData(byte[] data, ExchangeViewDataInfo info);
    }
}