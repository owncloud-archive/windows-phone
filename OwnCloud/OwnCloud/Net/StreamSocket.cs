using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OwnCloud.Net
{
    /// <summary>
    /// Offsers a tiny TCP-Stream-Socket reader and writer.
    /// </summary>
    class StreamSocket : IDisposable
    {
        Socket _socket;
        SocketAsyncEventArgs _socketReadEventArgs;
        SocketAsyncEventArgs _socketWriteEventArgs;
        SocketAsyncOperation _lastOp;
        static ManualResetEvent _clientDone = new ManualResetEvent(false);
        Queue<byte[]> _writeBufferQueue = new Queue<byte[]>();

        const int TIMEOUT = 5000;
        public const int MAX_BUFFER_SIZE = 4096;

        public StreamSocket() {
            Blocking = true;
        }

        /// <summary>
        /// Connects to a server.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="remoteport"></param>
        public void Connect(string hostname, int remoteport)
        {
            DnsEndPoint hostEntry = new DnsEndPoint(hostname, remoteport);
            ConnectTo(hostEntry);
        }

        public void ConnectTo(DnsEndPoint hostEntry)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketError _lastError = SocketError.NotConnected;
            SocketAsyncEventArgs socketEventArgs = new SocketAsyncEventArgs();

            socketEventArgs.RemoteEndPoint = hostEntry;
            socketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
            {
                _lastError = e.SocketError;
                _End();
                // Install read and write handler
                _socketReadEventArgs = new SocketAsyncEventArgs();
                _socketReadEventArgs.RemoteEndPoint = _socket.RemoteEndPoint;
                _socketReadEventArgs.SetBuffer(new byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
                _socketReadEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(_AsyncCallComplete);

                _socketWriteEventArgs = new SocketAsyncEventArgs();
                _socketWriteEventArgs.RemoteEndPoint = _socket.RemoteEndPoint;
                _socketWriteEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(_AsyncCallComplete);
            });

            // async connect
            _Reset();
            _socket.ConnectAsync(socketEventArgs);
            _BlockUI();

            if (_lastError != SocketError.Success)
            {
                // connection failed
                throw new Exception(String.Format(LocalizedStrings.Get("Net_StreamSocket_ConnectFailed"), hostEntry, _lastError.ToString()));
            }            
        }

        /// <summary>
        /// Sends data as string to the server and returns
        /// the send count on bytes. Note that it cannot more
        /// bytes sent then MAX_BUFFER_SIZE is set.
        /// Attention! Converting large text into bytes can cause
        /// a buffer limit exception.
        /// </summary>
        /// <param name="data"></param>
        public int Send(string data)
        {
            return Write(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Sends data as string to the server and returns
        /// the send count on bytes. Note that it cannot more
        /// bytes sent then MAX_BUFFER_SIZE is set.
        /// </summary>
        /// <param name="data"></param>
        public int Write(byte[] buffer)
        {

            if (buffer.Length > MAX_BUFFER_SIZE)
            {
                var temp = new byte[MAX_BUFFER_SIZE];
                Buffer.BlockCopy(buffer, 0, temp, 0, MAX_BUFFER_SIZE);
                buffer = temp;
            }

            if (Blocking)
            {
                _Reset();
                _socketWriteEventArgs.SetBuffer(buffer, 0, buffer.Length);
                _socket.SendAsync(_socketWriteEventArgs);
                _BlockUI();
            }
            else
            {
                _writeBufferQueue.Enqueue(buffer);
                _writeAsync();
            }

            if (Blocking && _socketWriteEventArgs.SocketError == SocketError.Success)
            {
                return _socketWriteEventArgs.BytesTransferred;
            }
            return 0;
        }

        /// <summary>
        /// Called each time if data should be written in unblock mode
        /// means that data is sequently written after the event returns
        /// independend from gui thread.
        /// </summary>
        private void _writeAsync()
        {
            if(_writeBufferQueue.Count > 0) {
                byte[] buffer = _writeBufferQueue.Dequeue();

                try
                {
                    _socketWriteEventArgs.SetBuffer(buffer, 0, buffer.Length);
                    _socket.SendAsync(_socketWriteEventArgs);
                }
                catch (InvalidOperationException)
                {
                    // a call is already in progress 
                    // nothing else to do except
                    // to restore the data buffer
                    _writeBufferQueue.Enqueue(buffer);
                }
            }
        }

        /// <summary>
        /// Reads count data from stream or returns the max. available
        /// data in the buffer.
        /// Note that it will block until async thread is ready, not till any data was received
        /// </summary>
        /// <returns></returns>
        public byte[] Read()
        {
            _Reset();
            _socket.ReceiveAsync(_socketReadEventArgs);
            _BlockUI();

            if (_socketReadEventArgs.SocketError == SocketError.Success && _socketReadEventArgs.BytesTransferred > 0)
            {
                byte[] buffer = new byte[_socketReadEventArgs.BytesTransferred];
                Buffer.BlockCopy(_socketReadEventArgs.Buffer, 0, buffer, 0, _socketReadEventArgs.BytesTransferred);
                return buffer;
            }
            else
            {
                return new byte[0];
            }
        }


        /// <summary>
        /// Gets or sets the blocking status.
        /// Note that Blocking is only available for Writing.
        /// </summary>
        public bool Blocking
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the Last Socket Operation
        /// </summary>
        public SocketAsyncOperation LastOperation
        {
            get
            {
                return _lastOp;
            }
        }

        public void Shutdown()
        {
            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                Dispose();
            }
        }

        private void _Reset()
        {
            _clientDone.Reset();
        }

        private void _BlockUI()
        {
            _clientDone.WaitOne(TIMEOUT);
        }

        private void _End()
        {
            _clientDone.Set();
        }

        private void _AsyncCallComplete(object s, SocketAsyncEventArgs e)
        {
            _lastOp = e.LastOperation;
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    // if there a some data in queue call send routine
                    if (_writeBufferQueue.Count > 0)
                    {
                        _writeAsync();
                    }
                    break;
            }
            _End();
        }

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }
    }
}
