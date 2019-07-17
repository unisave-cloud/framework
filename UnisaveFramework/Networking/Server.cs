using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Unisave.Networking
{
    /*
        Unsiave.Networking provides a message-stream communication protocol
        built on top of TPC that is used for communication between all the
        unisave server components
     */

    public class Server
    {
        /// <summary>
        /// Lock for synchronizing operations on this instance
        /// </summary>
        private object syncLock = new object();

        /// <summary>
        /// Underlying listener instance
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// Thread on which the accepting loop runs
        /// </summary>
        private Thread listeningThread;

        private readonly string ipAddress;
        private readonly int port;

        private readonly Action<Client> clientHandler;

        // flags indicating server state
        // (server can only be started and stopped once)
        private bool started = false;
        private bool stopped = false;

        public Server(string ipAddress, int port, Action<Client> clientHandler)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.clientHandler = clientHandler;
        }

        /// <summary>
        /// Starts listening for incomming connections
        /// This method does not block, the listening is performed on another thread
        /// </summary>
        public void Start()
        {
            lock (syncLock)
            {
                if (started)
                    throw new InvalidOperationException("Cannot start the server twice.");

                listener = new TcpListener(IPAddress.Parse(ipAddress), port);
                listener.Start();

                listeningThread = new Thread(() => {
                    AcceptingLoop();
                });
                listeningThread.Start();
            }
        }

        /// <summary>
        /// The loop for accepting clients
        /// Runs in it's own thread
        /// </summary>
        private void AcceptingLoop()
        {
            while (true)
            {
                Socket s = listener.AcceptSocket(); // blocks

                new Thread(() => {
                    clientHandler(new Client(s));
                    s.Close();
                }).Start();
            }
        }

        /// <summary>
        /// Stops the server
        /// This method is thread-safe
        /// </summary>
        public void Stop()
        {
            lock (syncLock)
            {
                if (!started)
                    return;

                if (stopped)
                    return;

                listener.Stop();
                listener = null;

                listeningThread.Abort();
                listeningThread = null;
            }
        }
    }
}
