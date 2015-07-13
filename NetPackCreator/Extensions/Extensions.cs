using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NetPackCreator.Extensions
{
    /// <summary></summary>
    internal static class Extensions
    {
        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
        }

        #region AddressAccessibility

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

        /// <summary>Check accessibility of target IP address.</summary>
        /// <param name="ipAddress">Target IP address.</param>
        /// <param name="token">Operation cancellation token.</param>
        /// <returns><see langword="true"/> if address accessible; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"> thrown if <paramref name="ipAddress"/> is <see langword="null"/>.</exception>
        public static Task<bool> AddressAccessibilityAsync(this IPAddress ipAddress, CancellationToken token)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            token.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var result = AddressAccessibility(ipAddress);
                token.ThrowIfCancellationRequested();
                return result;
            }, token);
        }

        #endregion

        #region PhysicalAddressForIP

        ///// <summary>Sends an Address Resolution Protocol (ARP) request to obtain the physical address that corresponds
        ///// to the specified destination IPv4 address</summary>
        ///// <param name="destIP">The destination IPv4 address.</param>
        ///// <param name="srcIP">The source IPv4 address of the sender. This parameter is optional and may be ZERO.</param>
        ///// <param name="pMacAddr">A pointer to an array of ULONG variables.</param>
        ///// <param name="phyAddrLen">On input, a pointer to a ULONG value that specifies the maximum buffer size. On
        ///// successful output, this parameter points to a value that specifies the number of bytes written
        ///// to the buffer pointed to by the pMacAddr.</param>
        ///// <returns>If the function succeeds, the return value is NO_ERROR.</returns>
        //[DllImport("iphlpapi.dll", ExactSpelling = true)]
        //private static extern int SendARP([In] int destIP, [In] int srcIP, [Out] byte[] pMacAddr, [In, Out] ref uint phyAddrLen);

        ///// <summary>Gets the MAC address from ARP table.</summary>
        ///// <param name="destinationAddress">IPv4 address of the remote host for which MAC address is desired.</param>
        ///// <param name="sourceAddress">The source IPv4 address of the sender. This parameter
        ///// is optional and may be <see langword="null"/>.</param>
        ///// <returns>MAC address; <see langword="null"/> if MAC address could not be found.</returns>
        ///// <exception cref="ArgumentNullException"> thrown
        ///// if <paramref name="destinationAddress"/> is <see langword="null"/>.</exception>
        //public static PhysicalAddress PhysicalAddressForIP(this IPAddress destinationAddress,  IPAddress sourceAddress)
        //{
        //    if (destinationAddress == null) throw new ArgumentNullException("destinationAddress");
            
        //    var macAddress = new byte[6];
        //    var macAddressLength = (uint)macAddress.Length;
        //    var sourceIPAddress = (sourceAddress != null) ? (int)sourceAddress.Address : 0;

        //    // Call WinAPI SendARP function.            
        //    return SendARP((int)destinationAddress.Address, sourceIPAddress, macAddress, ref macAddressLength) != 0 ? null : new PhysicalAddress(macAddress);
        //}

        ///// <summary>Gets the MAC address from ARP table.</summary>
        ///// <param name="destinationAddress">IPv4 address of the remote host for which MAC address is desired.</param>
        ///// <param name="sourceAddress">The source IPv4 address of the sender. This parameter
        ///// is optional and may be <see langword="null"/>.</param>
        ///// <param name="token">Operation cancellation token.</param>
        ///// <returns>MAC address; <see langword="null"/> if MAC address could not be found.</returns>
        ///// <exception cref="ArgumentNullException"> thrown
        ///// if <paramref name="destinationAddress"/> is <see langword="null"/>.</exception>
        //public static Task<PhysicalAddress> PhysicalAddressForIPAsync(this IPAddress destinationAddress, IPAddress sourceAddress, CancellationToken token)
        //{
        //    if (destinationAddress == null) throw new ArgumentNullException("destinationAddress");
        //    token.ThrowIfCancellationRequested();

        //    return Task.Run(() =>
        //    {
        //        var result = PhysicalAddressForIP(destinationAddress, sourceAddress);
        //        token.ThrowIfCancellationRequested();
        //        return result;
        //    }, token);
        //}

        #endregion

        #region PortAccessibility

        /// <summary>Check accessibility of port in target IPv4 address.</summary>
        /// <param name="ipAddress">Target IP address.</param>
        /// <param name="port">Target port.</param>
        /// <returns><see langword="true"/> if port accessible; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"> thrown if <paramref name="ipAddress"/> is <see langword="null"/>.</exception>
        public static bool PortAccessibility(this IPAddress ipAddress, ushort port)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");

            using (var tcpClient = new TcpClient(AddressFamily.InterNetwork))
            {
                try
                {
                    tcpClient.Connect(ipAddress, port);
                    return true;
                }
                catch (SocketException) { return false; }
            }
        }

        /// <summary>Async check accessibility of port in target IPv4 address.</summary>
        /// <param name="ipAddress">Target IP address.</param>
        /// <param name="port">Target port.</param>
        /// <param name="token">Operation cancellation token.</param>
        /// <returns><see langword="true"/> if port accessible; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"> thrown if <paramref name="ipAddress"/> is <see langword="null"/>.</exception>
        public static Task<bool> PortAccessibilityAsync(this IPAddress ipAddress, ushort port, CancellationToken token)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            token.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var result = PortAccessibility(ipAddress, port);
                token.ThrowIfCancellationRequested();
                return result;
            }, token);
        }

        #endregion
    }
}