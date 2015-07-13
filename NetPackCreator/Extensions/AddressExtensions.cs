using System;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NetPackCreator.Extensions
{
    /// <summary></summary>
    internal static class AddressExtensions
    {
        /// <summary>Check accessibility of target IP address.</summary>
        /// <param name="ipAddress">Target IP address.</param>
        /// <returns><see langword="true"/> if address accessible; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"> thrown if <paramref name="ipAddress"/> is <see langword="null"/>.</exception>
        public static bool AddressAccessibility(this IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");

            var reply = new Ping().Send(ipAddress.ToString(), 2000);
            return reply != null && reply.Status == IPStatus.Success;
        }

        /// <summary>Sends an Address Resolution Protocol (ARP) request to obtain the physical address that corresponds
        /// to the specified destination IPv4 address. http://www.pinvoke.net/default.aspx/iphlpapi/SendARP.html </summary>
        /// <param name="destIP">The destination IPv4 address.</param>
        /// <param name="srcIP">The source IPv4 address of the sender. This parameter is optional and may be ZERO.</param>
        /// <param name="pMacAddr">A pointer to an array of ULONG variables.</param>
        /// <param name="phyAddrLen">On input, a pointer to a ULONG value that specifies the maximum buffer size. On
        /// successful output, this parameter points to a value that specifies the number of bytes written
        /// to the buffer pointed to by the pMacAddr.</param>
        /// <returns>If the function succeeds, the return value is NO_ERROR.</returns>
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP([In] uint destIP, [In] uint srcIP, [Out] byte[] pMacAddr, [In, Out] ref int phyAddrLen);

        /// <summary>Gets the MAC address from ARP table.</summary>
        /// <param name="destinationAddress">IPv4 address of the remote host for which MAC address is desired.</param>
        /// <param name="sourceAddress">The source IPv4 address of the sender. This parameter
        /// is optional and may be <see langword="null"/>.</param>
        /// <returns>MAC address; <see langword="null"/> if MAC address could not be found.</returns>
        /// <exception cref="ArgumentNullException"> thrown
        /// if <paramref name="destinationAddress"/> is <see langword="null"/>.</exception>
        public static PhysicalAddress GetPhysicalAddressForIPv4(this IPAddress destinationAddress, IPAddress sourceAddress)
        {
            if (destinationAddress == null) throw new ArgumentNullException("destinationAddress");
            if (destinationAddress.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("Invalid destination address family.");

            if (sourceAddress != null && sourceAddress.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("Invalid source address family.");

            var macAddress = new byte[6];
            var macAddressLength = macAddress.Length;

            var destinationIPAddress = BitConverter.ToUInt32(destinationAddress.GetAddressBytes(), 0);
            var sourceIPAddress = sourceAddress != null ? BitConverter.ToUInt32(sourceAddress.GetAddressBytes(), 0) : 0;

            // Call WinAPI SendARP function.            
            var result = SendARP(destinationIPAddress, sourceIPAddress, macAddress, ref macAddressLength);

            if (result != 0) throw new Win32Exception(result, "SendARP failed.");

            return new PhysicalAddress(macAddress);
        }
    }
}