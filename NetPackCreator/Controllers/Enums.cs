using System.ComponentModel;

namespace NetPackCreator.Controllers
{
    /// <summary></summary>
    internal enum ConnectionMode
    {
        /// <summary></summary>
        [Description("UDP")]
        UDP,
        /// <summary></summary>
        [Description("TCP client")]
        TCPClient,
        /// <summary></summary>
        [Description("TCP server")]
        TCPServer
    }

    /// <summary></summary>
    internal enum ConnectionState
    {
        /// <summary></summary>
        [Description("Connect")]
        Disconnected,
        /// <summary></summary>
        [Description("Disconnect")]
        Connected,
        /// <summary></summary>
        [Description("Disconnection...")]
        DisconnectionInProcess,
        /// <summary></summary>
        [Description("Connection...")]
        ConnectionInProcess
    }
}