using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using NetPackCreator.Controllers;

namespace NetPackCreator.Extensions
{
    /// <summary></summary>
    internal static class InterfaceExtentions
    {
        /// <summary></summary>
        /// <param name="networkInterface"></param>
        /// <returns></returns>
        public static List<IPAddress> GetInterfaceIPv4UnicastAddresses(this NetworkInterface networkInterface)
        {
            if (networkInterface == null) throw new ArgumentNullException("networkInterface");

            return networkInterface.GetIPProperties().UnicastAddresses.Select(a => a.Address).Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();
        }

        /// <summary></summary>
        /// <param name="interfaceController"></param>
        /// <returns></returns>
        public static List<IPAddress> GetCurrentInterfaceIPv4UnicastAddresses(this InterfaceController interfaceController)
        {
            if (interfaceController == null) throw new ArgumentNullException("interfaceController");

            return interfaceController.CurrentValue.GetInterfaceIPv4UnicastAddresses();
        }
    }
}