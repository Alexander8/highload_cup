using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Travels.Server
{
    internal static class SocketServer
    {
        private const int MaxSocketConnections = 50000;
        private const int MaxBufferSize = 1024 * 2;

        private static readonly IPEndPoint IpEndPoint;
        private static readonly Socket ServerSocket;
        private static readonly Thread AcceptConnectionsThread;
        private static readonly List<Thread> ProcessConnectionsThreads = new List<Thread>();
        private static readonly ConcurrentQueue<Socket> RequestsQueue = new ConcurrentQueue<Socket>();

        static SocketServer()
        {
            var port = 60000;
#if RELEASE
            port = 80;
#endif

#if RELEASE
            IpEndPoint = new IPEndPoint(0, port);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = true,
                ReceiveBufferSize = MaxBufferSize,
                SendBufferSize = MaxBufferSize * 2,
                ReceiveTimeout = 10,
                SendTimeout = 10,
                LingerState = new LingerOption(true, 0)
            };
#else
            IpEndPoint = new IPEndPoint(IPAddress.IPv6Loopback, port);
            ServerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = true,
                ReceiveBufferSize = MaxBufferSize,
                SendBufferSize = MaxBufferSize * 2,
                ReceiveTimeout = 10,
                SendTimeout = 10,
                LingerState = new LingerOption(true, 0)
            };
#endif
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            AcceptConnectionsThread = new Thread(AcceptConnections)
            {
                Name = "AcceptConnectionsThread"
            };

            for (var i = 0; i < 3; i++)
            {
                var thread = new Thread(ProcessConnections)
                {
                    Name = "ProcessConnectionsThread" + (i + 1)
                };

                ProcessConnectionsThreads.Add(thread);
            }
        }

        public static void Init()
        {
            ServerSocket.Bind(IpEndPoint);
            ServerSocket.Listen(MaxSocketConnections);

            AcceptConnectionsThread.Start();

            foreach (var thread in ProcessConnectionsThreads)
                thread.Start();

            Console.WriteLine("Socket server initialized");
        }

        private static void AcceptConnections()
        {
            Thread.BeginThreadAffinity();

            while (true)
            {
                try
                {
                    var acceptedSocket = ServerSocket.Accept();
                    RequestsQueue.Enqueue(acceptedSocket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Thread.EndThreadAffinity();
        }

        private static void ProcessConnections()
        {
            Thread.BeginThreadAffinity();

            var request = new Request(new byte[MaxBufferSize]);

            while (true)
            {
                if (!RequestsQueue.TryDequeue(out var socket))
                    continue;

                try
                {
                    if (!socket.Connected)
                    {
                        Console.WriteLine("Socket is not connected: skip");
                        continue;
                    }

                    var readBytes = socket.Receive(request.Body);
                    Debug.Assert(readBytes > 0);

                    var response = request.Process(readBytes);

                    var sentBytes = socket.Send(response);
                    Debug.Assert(sentBytes == response.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    CloseSocket(socket);
                }
            }

            Thread.EndThreadAffinity();
        }

        private static void CloseSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // ignored
            }
            finally
            {
                socket.Close();
            }
        }
    }
}
