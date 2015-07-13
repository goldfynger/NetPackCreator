using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace NetPackCreator
{
    /// <summary>http://stackoverflow.com/questions/9253300/how-do-i-use-resolveipnetentry2-in-c-sharp</summary>
    internal static class IPv6MacResolver
    {
        /// <summary></summary>
        private const short AF_INET6 = 23;

        #region Structs for ResolveIpNetEntry2

        /// <summary></summary>
        private struct MIB_IPNET_ROW2
        {
            public SOCKADDR_INET Address;
            public uint InterfaceIndex;
            public ulong InterfaceLuid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] PhysicalAddress;

            public uint PhysicalAddressLength;
            public NL_NEIGHBOR_STATE State;
            public byte Flags;
            public uint LastReachable;
        }

        /// <summary></summary>
        private struct SOCKADDR_INET
        {
            public SOCKADDR_IN6 Ipv6;
        }

        /// <summary></summary>
        private struct SOCKADDR_IN6
        {
            public short sin6_family;
            public ushort sin6_port;
            public uint sin6_flowinfo;
            public in6_addr sin6_addr;
            public uint sin6_scope_id;
        }

        /// <summary></summary>
        private struct in6_addr
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Byte;
        }

        #endregion

        /// <summary></summary>
        private enum NL_NEIGHBOR_STATE
        {
            NlnsUnreachable,
            NlnsIncomplete,
            NlnsProbe,
            NlnsDelay,
            NlnsStale,
            NlnsReachable,
            NlnsPermanent,
            NlnsMaximum
        }

        [DllImport("Iphlpapi.dll")]
        private static extern int ResolveIpNetEntry2(ref MIB_IPNET_ROW2 Row, ref SOCKADDR_INET SourceAddress);

        /// <summary></summary>
        /// <param name="ipv6Address"></param>
        /// <returns></returns>
        public static byte[] GetMacFromIPv6Address(IPAddress ipv6Address)
        {
            if (ipv6Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6) throw new ArgumentException("The IPAddress provided was not an IPv6 address.");

            //set up target address
            MIB_IPNET_ROW2 row2 = new MIB_IPNET_ROW2();
            row2.PhysicalAddress = new byte[32];
            row2.State = NL_NEIGHBOR_STATE.NlnsReachable;
            row2.Address.Ipv6.sin6_addr.Byte = new byte[16];
            row2.Address.Ipv6.sin6_family = AF_INET6;
            row2.Address.Ipv6.sin6_flowinfo = 0;
            row2.Address.Ipv6.sin6_port = 0;
            row2.Address.Ipv6.sin6_scope_id = Convert.ToUInt32(ipv6Address.ScopeId);

            byte[] ipv6AddressBytes = ipv6Address.GetAddressBytes();
            System.Buffer.BlockCopy(ipv6AddressBytes, 0, row2.Address.Ipv6.sin6_addr.Byte, 0, ipv6AddressBytes.Length);

            //get this machine's local IPv6 address
            SOCKADDR_INET sock = new SOCKADDR_INET();
            sock.Ipv6.sin6_family = AF_INET6;
            sock.Ipv6.sin6_flowinfo = 0;
            sock.Ipv6.sin6_port = 0;
            sock.Ipv6.sin6_addr.Byte = new byte[16];

            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    sock.Ipv6.sin6_addr.Byte = address.GetAddressBytes();
                    break;
                }
            }

            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus == OperationalStatus.Up)
                {
                    row2.InterfaceIndex = (uint)ClsNetworkStats.GetInterfaceIndex(netInterface.Description);
                    break;
                }
            }

            int result = ResolveIpNetEntry2(ref row2, ref sock);

            if (result != 0) throw new ApplicationException("The call to ResolveIpNetEntry2 failed; error number: " + result.ToString());

            byte[] macAddress = new byte[6];
            Buffer.BlockCopy(row2.PhysicalAddress, 0, macAddress, 0, 6);

            return macAddress;
        }

        /// <summary>http://stackoverflow.com/questions/9253300/how-do-i-use-resolveipnetentry2-in-c-sharp</summary>
        private class ClsNetworkStats
        {
            // Fields
            private const long ERROR_SUCCESS = 0L;
            private ArrayList m_Adapters;
            private const long MAX_INTERFACE_NAME_LEN = 0x100L;
            private const long MAXLEN_IFDESCR = 0x100L;
            private const long MAXLEN_PHYSADDR = 8L;

            // Methods
            public ClsNetworkStats() : this(true) { }

            public ClsNetworkStats(bool IgnoreLoopBack)
            {
                int lRetSize = 0;
                MIB_IFROW ifrow = new MIB_IFROW();
                byte[] buff = new byte[1];
                byte val = 0;
                long ret = GetIfTable(ref val, ref lRetSize, 0);
                buff = new byte[lRetSize];
                ret = GetIfTable(ref buff[0], ref lRetSize, 0);
                int lRows = buff[0];
                this.m_Adapters = new ArrayList(lRows);
                byte len = (byte)lRows;
                for (byte i = 1; i <= len; i++)
                {
                    ifrow = new MIB_IFROW();
                    ifrow.dwIndex = Convert.ToUInt32(i);
                    ret = GetIfEntry(ref ifrow);
                    IFROW_HELPER ifhelp = this.PrivToPub(ifrow);
                    if (IgnoreLoopBack)
                    {
                        if (ifhelp.Description.IndexOf("Loopback") < 0)
                        {
                            this.m_Adapters.Add(ifhelp);
                        }
                    }
                    else
                    {
                        this.m_Adapters.Add(ifhelp);
                    }
                }
            }

            public IFROW_HELPER GetAdapter(int index)
            {
                return (IFROW_HELPER)this.m_Adapters[index];
            }

            public int Count
            {
                get
                {
                    return this.m_Adapters.Count;
                }
            }

            [DllImport("iphlpapi")]
            private static extern int GetIfEntry(ref MIB_IFROW pIfRow);
            [DllImport("iphlpapi")]
            private static extern int GetIfTable(ref byte pIfRowTable, ref int pdwSize, int bOrder);
            //[DebuggerStepThrough]
            private IFROW_HELPER PrivToPub(MIB_IFROW pri)
            {
                IFROW_HELPER ifhelp = new IFROW_HELPER();
                ifhelp.Name = pri.wszName.Trim();
                ifhelp.Index = Convert.ToInt32(pri.dwIndex);
                ifhelp.Type = Convert.ToInt32(pri.dwType);
                ifhelp.Mtu = Convert.ToInt32(pri.dwMtu);
                ifhelp.Speed = Convert.ToInt32(pri.dwSpeed);
                ifhelp.PhysAddrLen = Convert.ToInt32(pri.dwPhysAddrLen);
                ifhelp.PhysAddr = Encoding.ASCII.GetString(pri.bPhysAddr);
                ifhelp.AdminStatus = Convert.ToInt32(pri.dwAdminStatus);
                ifhelp.OperStatus = Convert.ToInt32(pri.dwOperStatus);
                ifhelp.LastChange = Convert.ToInt32(pri.dwLastChange);
                ifhelp.InOctets = pri.dwInOctets; //Convert.ToInt32(pri.dwInOctets);
                ifhelp.InUcastPkts = Convert.ToInt32(pri.dwInUcastPkts);
                ifhelp.InNUcastPkts = Convert.ToInt32(pri.dwInNUcastPkts);
                ifhelp.InDiscards = Convert.ToInt32(pri.dwInDiscards);
                ifhelp.InErrors = Convert.ToInt32(pri.dwInErrors);
                ifhelp.InUnknownProtos = Convert.ToInt32(pri.dwInUnknownProtos);
                ifhelp.OutOctets = pri.dwOutOctets;//Convert.ToInt32(pri.dwOutOctets);
                ifhelp.OutUcastPkts = Convert.ToInt32(pri.dwOutUcastPkts);
                ifhelp.OutNUcastPkts = Convert.ToInt32(pri.dwOutNUcastPkts);
                ifhelp.OutDiscards = Convert.ToInt32(pri.dwOutDiscards);
                ifhelp.OutErrors = Convert.ToInt32(pri.dwOutErrors);
                ifhelp.OutQLen = Convert.ToInt32(pri.dwOutQLen);
                ifhelp.Description = Encoding.ASCII.GetString(pri.bDescr, 0, Convert.ToInt32(pri.dwDescrLen));
                ifhelp.InMegs = this.ToMegs((long)ifhelp.InOctets);
                ifhelp.OutMegs = this.ToMegs((long)ifhelp.OutOctets);
                return ifhelp;
            }

            [DebuggerStepThrough]
            private string ToMegs(long lSize)
            {
                string sDenominator = " B";
                if (lSize > 0x3e8L)
                {
                    sDenominator = " KB";
                    lSize = (long)Math.Round((double)(((double)lSize) / 1000.0));
                }
                else if (lSize <= 0x3e8L)
                {
                    sDenominator = " B";
                    //            lSize = lSize;
                }

                return lSize.ToString("###,###") + sDenominator;
                //            (Strings.Format(lSize, "###,###0") + sDenominator);
            }

            // Nested Types
            [StructLayout(LayoutKind.Sequential)]
            public struct IFROW_HELPER
            {
                public string Name;
                public int Index;
                public int Type;
                public int Mtu;
                public int Speed;
                public int PhysAddrLen;
                public string PhysAddr;
                public int AdminStatus;
                public int OperStatus;
                public int LastChange;
                public uint InOctets;   //changed
                public int InUcastPkts;
                public int InNUcastPkts;
                public int InDiscards;
                public int InErrors;
                public int InUnknownProtos;
                public uint OutOctets;  //changed
                public int OutUcastPkts;
                public int OutNUcastPkts;
                public int OutDiscards;
                public int OutErrors;
                public int OutQLen;
                public string Description;
                public string InMegs;
                public string OutMegs;
            }

            /// <summary></summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private struct MIB_IFROW
            {
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
                public string wszName;
                public uint dwIndex;
                public uint dwType;
                public uint dwMtu;
                public uint dwSpeed;
                public uint dwPhysAddrLen;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] bPhysAddr;
                public uint dwAdminStatus;
                public uint dwOperStatus;
                public uint dwLastChange;
                public uint dwInOctets;
                public uint dwInUcastPkts;
                public uint dwInNUcastPkts;
                public uint dwInDiscards;
                public uint dwInErrors;
                public uint dwInUnknownProtos;
                public uint dwOutOctets;
                public uint dwOutUcastPkts;
                public uint dwOutNUcastPkts;
                public uint dwOutDiscards;
                public uint dwOutErrors;
                public uint dwOutQLen;
                public uint dwDescrLen;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
                public byte[] bDescr;
            }

            /// <summary></summary>
            /// <param name="description"></param>
            /// <returns></returns>
            public static int GetInterfaceIndex(string description)
            {
                int val = 0;

                ClsNetworkStats stats = new ClsNetworkStats();
                for (int index = 0; index < stats.Count; index++)
                {
                    string desc = stats.GetAdapter(index).Description;
                    if (desc == description + "\0")
                    {
                        val = stats.GetAdapter(index).Index;
                        break;
                    }
                }

                return val;
            }
        }
    }
}