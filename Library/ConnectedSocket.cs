namespace SocketLibrary
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An IPv4 TCP connected socket.
    /// </summary>
    public sealed class ConnectedSocket : IDisposable
    {
        private readonly Encoding _encoding;

        private readonly Socket _socket;

        /// <summary>
        /// Constructs and connects the socket.
        /// </summary>
        /// <param name="endpoint">Endpoint to connect to</param>
        public ConnectedSocket(EndPoint endpoint) : this(endpoint, Encoding.UTF8) { }

        /// <summary>
        /// Constructs and connects the socket.
        /// </summary>
        /// <param name="endpoint">Endpoint to connect to</param>
        /// <param name="encoding">Encoding of the content sended and received by the socket</param>
        public ConnectedSocket(EndPoint endpoint, Encoding encoding)
        {
            _encoding = encoding;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(endpoint);
        }

        /// <summary>
        /// Constructs and connects the socket.
        /// </summary>
        /// <param name="host">Host to connect to</param>
        /// <param name="port">Port to connect to</param>
        public ConnectedSocket(string host, int port) : this(host, port, Encoding.UTF8) { }

        /// <summary>
        /// Constructs and connects the socket.
        /// </summary>
        /// <param name="host">Host to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="encoding">Encoding of the content sended and received by the socket</param>
        public ConnectedSocket(string host, int port, Encoding encoding)
        {
            _encoding = encoding;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(host, port);
        }

        internal ConnectedSocket(Socket socket)
        {
            _encoding = Encoding.UTF8;
            _socket = socket;
        }

        /// <summary>
        /// True if there's any data to receive on the socket.
        /// </summary>
        public bool AnythingToReceive
        {
            get
            {
                return _socket.Available > 0;
            }
        }

        /// <summary>
        /// The underlying socket.
        /// </summary>
        public Socket UnderlyingSocket
        {
            get
            {
                return _socket;
            }
        }

        /// <summary>
        /// Disposes the socket.
        /// </summary>
        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

        public int Receive(byte[] buffer, int offset, int count)
        {
            return _socket.Receive(buffer, offset, count, SocketFlags.None);
        }

        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
        {
            return await Task.Factory.FromAsync(
                (asyncCallback, state) => _socket.BeginReceive(buffer, offset, count, SocketFlags.None, asyncCallback, state),
                (asyncResult) => { return _socket.EndReceive(asyncResult); },
                TaskCreationOptions.None);
        }

        /// <summary>
        /// Receives any pending data.
        /// This blocks execution until there's data available.
        /// </summary>
        /// <param name="bufferSize">Amount of data to read</param>
        /// <returns>Received data</returns>
        public string ReceiveString(int bufferSize = 1024)
        {
            var buffer = new byte[bufferSize];
            _socket.Receive(buffer);
            return _encoding.GetString(buffer).TrimEnd('\0');
        }

        public async Task<string> ReceiveStringAsync(int bufferSize = 1024)
        {
            var buffer = new byte[bufferSize];

            await Task.Factory.FromAsync(
                (asyncCallback, state) => _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, asyncCallback, state),
                (asyncResult) => _socket.EndReceive(asyncResult),
                TaskCreationOptions.None);

            return _encoding.GetString(buffer).TrimEnd('\0');
        }

        /// <summary>
        /// Sends the given data.
        /// </summary>
        /// <param name="data">Data to send</param>
        public int Send(string data)
        {
            var bytes = _encoding.GetBytes(data);
            return _socket.Send(bytes);
        }

        public Task<int> SendAsync(string data)
        {
            var bytes = _encoding.GetBytes(data);
            return Task.Factory.FromAsync(
                (asyncCallback, state) => _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, asyncCallback, state),
                (asyncResult) => { return _socket.EndSend(asyncResult); },
                TaskCreationOptions.None);
        }
    }
}
