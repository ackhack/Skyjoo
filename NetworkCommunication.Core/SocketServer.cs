using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkCommunication.Core
{
    /// <summary>
    /// Socket server.
    /// </summary>
    public class SocketServer
    {
        /// <summary>
        /// Occurs when the server state changes.
        /// </summary>
        public event EventHandler<SocketServerState> StateChanged;

        /// <summary>
        /// Occurs when the server receives message.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> ReceivedMessage;

        /// <summary>
        /// Occurs when the server has accepted the connection host.
        /// </summary>
        public event EventHandler<RemoteHost> HostAcceptConnection;

        /// <summary>
        /// Occurs when a host connection is closed.
        /// </summary>
        public event EventHandler<RemoteHost> HostClosedConnection;

        /// <summary>
        /// Gets the server current port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get
            {
                return port;
            }
        }

        /// <summary>
        /// Gets the server current state.
        /// </summary>
        /// <value>The state.</value>
        public SocketServerState State
        {
            get
            {
                return state;
            }
        }

        private List<RemoteHost> clients = new List<RemoteHost>();
        private int maxClients;

        /// <summary>
        /// Initializes a new instance of the server class.
        /// </summary>
        public SocketServer(int maxClients = 100)
        {
            this.maxClients = maxClients;
            this.state = SocketServerState.Stopped;
        }

        /// <summary>
        /// Run the server by specified port.
        /// </summary>
        /// <param name="port">The port for the server to listen to.</param>
        public void Run(int port)
        {
            if (state == SocketServerState.Stopped)
            {
                this.port = port;

                OnStateChanged(SocketServerState.Starting);

                var thread = new Thread(initListening);

                thread.IsBackground = true;

                thread.Start();
            }
        }

        /// <summary>
        /// Stop this server instance.
        /// </summary>
        public void Stop()
        {
            if (mainSocket != null)
            {
                mainSocket.Close();

                mainSocket = null;
            }
        }

        /// <summary>
        /// Inits the listening for the incoming connections on specified port.
        /// </summary>
        private void initListening()
        {
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, (int)port);

                mainSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                mainSocket.Bind(endPoint);

                mainSocket.Listen((int)(SocketOptionName.MaxConnections));

                OnStateChanged(SocketServerState.Running);

                var senderThread = new Thread(HostSender);
                senderThread.IsBackground = true;
                senderThread.Start();
                Listening();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                stopServer();
            }
        }

        /// <summary>
        /// Listening for the incoming connections on specified port.
        /// </summary>
        private void Listening()
        {
            try
            {
                while (state == SocketServerState.Running && clients.Count < maxClients)
                {
                    var host = new RemoteHost(mainSocket.Accept());

                    OnHostAcceptConnection(host);

                    var recieverThread = new Thread(HostReciever);
                    recieverThread.IsBackground = true;
                    recieverThread.Start(host);

                    clients.Add(host);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                OnStateChanged(SocketServerState.Stopped);

                for (int index = 0; index < clients.Count; index++)
                {
                    var host = clients[index];

                    if (host.Connection.Connected)
                    {
                        host.Connection.Close();
                    }
                }
                clients.Clear();
            }
        }

        /// <summary>
        /// Stops the Server and disconnects Clients.
        /// </summary>
        private void stopServer()
        {
            OnStateChanged(SocketServerState.Stopped);

            for (int index = 0; index < clients.Count; index++)
            {
                var host = clients[index];

                if (host.Connection.Connected)
                {
                    host.Connection.Close();
                }
            }

            clients.Clear();
        }

        /// <summary>
        /// Recieves messages from clients.
        /// </summary>
        private void HostReciever(object obj)
        {
            var host = (RemoteHost)obj;
            try
            {
                while (state == SocketServerState.Running)
                {
                    if (host.Connection.Connected)
                    {
                        var buffer = new byte[1448];
                        var count = host.Connection.Receive(buffer);
                        if (count == 0 || state == SocketServerState.Stopped)
                        {
                            break;
                        }
                        OnReceivedMessage(host, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                OnHostClosedConnection(host);
                clients.Remove(host);
                if (clients.Count >= maxClients - 1)
                {
                    new Thread(Listening).Start();
                }
            }
        }

        /// <summary>
        /// Sends messages to clients
        /// </summary>
        private void HostSender()
        {
            while (state == SocketServerState.Running)
            {
                while (messageQueue.TryDequeue(out var message))
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        var host = clients[i];
                        if (host.Connection.Connected)
                        {
                            try
                            {
                                host.Connection.Send(message);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                        else
                        {
                            OnHostClosedConnection(host);
                            clients.Remove(host);
                            if (clients.Count >= maxClients - 1)
                            {
                                new Thread(Listening).Start();
                            }
                            i--;
                        }
                    }
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Raises the server state changed event.
        /// </summary>
        /// <param name="state">Server state.</param>
        protected void OnStateChanged(SocketServerState state)
        {
            this.state = state;

            var handler = StateChanged;

            if (handler != null)
            {
                handler(this, state);
            }
        }

        /// <summary>
        /// Raises the server received message event.
        /// </summary>
        /// <param name="host">Information about host connection who sent the message.</param>
        /// <param name="message">Message byte array.</param>
        protected void OnReceivedMessage(RemoteHost host, byte[] message)
        {
            ReceivedMessage?.Invoke(this, new MessageReceivedEventArgs(host, message));
        }

        /// <summary>
        /// Raises the event whenever server accepts connection.
        /// </summary>
        /// <param name="host">Information about remote host connection from which is accepted</param>
        protected void OnHostAcceptConnection(RemoteHost host)
        {
            HostAcceptConnection?.Invoke(this, host);
        }

        /// <summary>
        /// Raises a host closed connection event.
        /// </summary>
        /// <param name="host">Information about host connection.</param>
        protected void OnHostClosedConnection(RemoteHost host)
        {
            HostClosedConnection?.Invoke(this, host); ;
        }


        public void SendMessageToClients(string message)
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

            var bytes = Encoding.UTF8.GetBytes(message);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                messageQueue.Enqueue(mso.ToArray());
            }
        }

        /// <summary>
        /// The main server socket.
        /// </summary>
        private Socket mainSocket;

        /// <summary>
        /// The current server state.
        /// </summary>
        private SocketServerState state;

        /// <summary>
        /// The current server port.
        /// </summary>
        private int port;

        /// <summary>
        /// The message queue.
        /// </summary>
        private QueueWithBlock<byte[]> messageQueue = new QueueWithBlock<byte[]>();
    }
}

