#define UDPCLIENT_SHOW_EXCEPTION_MSG

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

using NetPackCreator.Controllers;

namespace NetPackCreator.Connections
{
    /// <summary></summary>
    internal sealed class UdpConnection : Connection
    {
        /// <summary></summary>
        private UdpClient _client;

        /// <summary></summary>
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary></summary>
        /// <param name="sourceEndPoint"></param>
        public UdpConnection(IPEndPoint sourceEndPoint) : base(ConnectionMode.UDP, sourceEndPoint)
        {
            this._client = new UdpClient(sourceEndPoint);
        }

        /// <summary></summary>
        /// <param name="destinationEndPoint"></param>
        public override void Connect(IPEndPoint destinationEndPoint)
        {
            this._client.Connect(destinationEndPoint);

            this.Receive();

            base._connectionState = ConnectionState.Connected;
        }

        /// <summary></summary>
        /// <param name="data"></param>
        /// <param name="resultAction"></param>
        public override void Send(byte[] data, Action<int> resultAction)
        {
            if (data == null) throw new ArgumentNullException("data");

            if (this._connectionState != ConnectionState.Connected) throw new InvalidOperationException("Connection not established");

            this.SendAsync(data, resultAction);
        }

        /// <summary></summary>
        /// <param name="data"></param>
        /// <param name="resultAction"></param>
        private async void SendAsync(byte[] data, Action<int> resultAction)
        {
            if (!Dispatcher.CurrentDispatcher.CheckAccess()) throw new InvalidOperationException("Function called in non-UI thread");

            var result = -1;
            Exception exception = null;

            try
            {
                result = await this._client.SendAsync(data, data.Length);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {

#if UDPCLIENT_SHOW_EXCEPTION_MSG

                Debug.Fail(exception.Message);

#endif

                return;
            }

            resultAction(result);
        }

        /// <summary></summary>
        private async void Receive()
        {
            var token = this._cancellationTokenSource.Token;

            while (!token.IsCancellationRequested)
            {
                var result = await this._client.ReceiveAsync();

                if (token.IsCancellationRequested) return;

                this.OnDataReceived(result.Buffer);
            }
        }

        /// <summary></summary>
        public override void Dispose()
        {
            this._cancellationTokenSource.Cancel();

            if (this._client != null)
            {
                this._client.Close();

                this._connectionState = ConnectionState.Disconnected;
            }
        }
    }
}