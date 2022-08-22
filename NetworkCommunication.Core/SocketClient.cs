using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkCommunication.Core
{
    /// <summary>
    /// Socket client.
    /// </summary>
    public class SocketClient
    {
        /// <summary>
        /// Occurs when the client state changed.
        /// </summary>
        public event EventHandler<SocketClientState> StateChanged;

        /// <summary>
        /// Occurs when host sends a message.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> HostMessageRecieved;

        /// <summary>
        /// TCP Port used for connection
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
        }

        public string HostIpAddress
        {
            get
            {
                return ip;
            }
        }

        /// <summary>
        /// Gets the client current state.
        /// </summary>
        /// <value>The state.</value>
        public SocketClientState State
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        /// Initializes a new instance of the client.
        /// </summary>
        public SocketClient()
        {
            this.state = SocketClientState.Disconnected;
        }

        /// <summary>
        /// Try to connect by specified ip and port.
        /// </summary>
        /// <param name="ip">The remote pp address.</param>
        /// <param name="port">The remote port.</param>
        public void Connect(string ip, int port)
        {
            this.port = port;
            this.ip = ip;

            var thread = new Thread(ConnectionWork);

            thread.IsBackground = true;

            thread.Start(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        /// <summary>
        /// Breaks connection to current server.
        /// </summary>
        public void Disconnect()
        {
            if (mainSocket != null)
            {
                mainSocket.Close();

                mainSocket = null;
            }
        }

        /// <summary>
        /// Inner method to handle connection to specified address.
        /// </summary>
        /// <param name="obj">EndPoint adderess.</param>
        private void ConnectionWork(object obj)
        {
            void CopyTo(Stream src, Stream dest)
            {
                byte[] bytes = new byte[4096];

                int cnt;

                while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
                {
                    dest.Write(bytes, 0, cnt);
                }
            }

            try
            {
                var endPoint = (IPEndPoint)obj;

                mainSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                mainSocket.Connect(endPoint);

                OnStateChanged(SocketClientState.Connected);

                while (state == SocketClientState.Connected)
                {
                    var buffer = new byte[1448];

                    var count = mainSocket.Receive(buffer);

                    System.Diagnostics.Debug.WriteLine("Recieved " + count);

                    if (count == 0 || state == SocketClientState.Disconnected)
                    {
                        break;
                    }
                    using (var msi = new MemoryStream(buffer))
                    using (var mso = new MemoryStream())
                    {
                        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                        {
                            CopyTo(gs, mso);
                        }

                        OnReceivedMessage(mso.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (state == SocketClientState.Connected)
                {
                    OnStateChanged(SocketClientState.Disconnected);
                }
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The byte array.</param>
        public void SendMessage(byte[] message)
        {
            if (messagesThread == null)
            {
                messagesThread = new Thread(MessagesThreadWork);

                messagesThread.IsBackground = true;

                messageQueue = new QueueWithBlock<byte[]>();

                messagesThread.Start();
            }

            messageQueue.Enqueue(message);
        }

        /// <summary>
        /// Start loop for sending messages.
        /// </summary>
        private void MessagesThreadWork()
        {
            while (state == SocketClientState.Connected)
            {
                var message = messageQueue.Dequeue();

                if (message != null)
                {
                    SendMessageWork(message);
                }
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="obj">The byte array.</param>
        private void SendMessageWork(object obj)
        {
            try
            {
                var message = (byte[])obj;

                if (mainSocket != null && mainSocket.Connected && message.Length > 0 && state == SocketClientState.Connected)
                {
                    mainSocket.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

                if (state == SocketClientState.Connected)
                {
                    OnStateChanged(SocketClientState.Disconnected);
                }
            }
        }

        private void OnReceivedMessage(byte[] message)
        {
            HostMessageRecieved?.Invoke(this, new MessageReceivedEventArgs(null, message));
        }

        /// <summary>
        /// Raised when connection state is changed.
        /// </summary>
        /// <param name="state">Client state.</param>
        protected void OnStateChanged(SocketClientState state)
        {
            this.state = state;

            if (state == SocketClientState.Disconnected)
            {
                if (messageQueue != null)
                {
                    messageQueue.Release();

                    messageQueue = null;
                }

                messagesThread = null;
            }

            var handler = StateChanged;

            if (handler != null)
            {
                handler(this, state);
            }
        }


        /// <summary>
        /// The message queue.
        /// </summary>
        private QueueWithBlock<byte[]> messageQueue;

        /// <summary>
        /// Thread for sending messages
        /// </summary>
        private Thread messagesThread;

        /// <summary>
        /// The main client socket.
        /// </summary>
        private Socket mainSocket;

        /// <summary>
        /// The client current port.
        /// </summary>
        private int port;

        /// <summary>
        /// The hosts ip.
        /// </summary>
        private string ip;

        /// <summary>
        /// The client current state.
        /// </summary>
        private SocketClientState state;
    }
}

