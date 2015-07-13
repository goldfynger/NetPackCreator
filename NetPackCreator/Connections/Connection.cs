using System;
using System.Net;

using NetPackCreator.Controllers;

namespace NetPackCreator.Connections
{
    /// <summary></summary>
    internal abstract class Connection : IDisposable
    {
        /// <summary></summary>
        /// <param name="connectionMode"></param>
        /// <param name="sourceEndPoint"></param>
        /// <returns></returns>
        public static Connection CreateConnection(ConnectionMode connectionMode, IPEndPoint sourceEndPoint)
        {
            switch (connectionMode)
            {
                case ConnectionMode.UDP:
                    return new UdpConnection(sourceEndPoint);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary></summary>
        protected readonly ConnectionMode _connectionMode;

        /// <summary></summary>
        protected readonly IPEndPoint _sourceEndPoint;

        /// <summary></summary>
        protected ConnectionState _connectionState;

        /// <summary></summary>
        /// <param name="connectionMode"></param>
        /// <param name="sourceEndPoint"></param>
        protected Connection(ConnectionMode connectionMode, IPEndPoint sourceEndPoint)
        {
            this._connectionMode = connectionMode;
            this._sourceEndPoint = sourceEndPoint;

            this._connectionState = ConnectionState.Disconnected;
        }

        /// <summary></summary>
        public ConnectionMode ConnectionMode
        {
            get { return this._connectionMode; }
        }

        /// <summary></summary>
        public ConnectionState ConnectionState
        {
            get { return this._connectionState; }
        }

        /// <summary></summary>
        /// <param name="destinationEndPoint"></param>
        public abstract void Connect(IPEndPoint destinationEndPoint);

        /// <summary></summary>
        /// <param name="data"></param>
        public abstract void Send(byte[] data, Action<int> actionResult);

        /// <summary></summary>
        public event EventHandler<DataEventArgs<byte[]>> DataReceived;

        /// <summary></summary>
        /// <param name="data"></param>
        protected void OnDataReceived(byte[] data)
        {
            var handler = this.DataReceived;
            if (handler != null) handler(this, new DataEventArgs<byte[]>(data));
        }

        /// <summary></summary>
        public abstract void Dispose();
    }
}