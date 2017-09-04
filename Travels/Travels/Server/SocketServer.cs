using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Travels.Server
{
    internal static class SocketServer
    {
        private const int MaxSocketConnections = 50000;
        private const int MaxBufferSize = 1024 * 2;

        private static readonly IPEndPoint IpEndPoint;
        private static readonly Socket ServerSocket;
        private static readonly ConcurrentBag<Request> RequestPool = new ConcurrentBag<Request>();

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
                Blocking = false,
                ReceiveBufferSize = MaxBufferSize,
                SendBufferSize = MaxBufferSize * 2,
                LingerState = new LingerOption(true, 0)
            };
#else
            IpEndPoint = new IPEndPoint(IPAddress.IPv6Loopback, port);
            ServerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                ReceiveBufferSize = MaxBufferSize,
                SendBufferSize = MaxBufferSize * 2,
                LingerState = new LingerOption(true, 0)
            };
#endif
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            for (var i = 0; i < MaxSocketConnections; ++i)
            {
                var body = new byte[MaxBufferSize];
                RequestPool.Add(new Request(body));
            }
        }

        public static void Init()
        {
            ServerSocket.Bind(IpEndPoint);
            ServerSocket.Listen(MaxSocketConnections);
            ProcessConnections();

            Console.WriteLine("Socket server initialized");
        }

        private static void ProcessConnections()
        {
            try
            {
                ServerSocket.BeginAccept(ProcessConnection, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void ProcessConnection(IAsyncResult asyncResult)
        {
            Socket acceptedSocket = null;

            try
            {
                var canContinue = true;

                try
                {
                    acceptedSocket = ServerSocket.EndAccept(asyncResult);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    canContinue = false;
                }

                ProcessConnections();

                if (!canContinue)
                    return;

                if (!acceptedSocket.Connected)
                {
                    CloseSocket(acceptedSocket);
                    return;
                }

                if (!RequestPool.TryTake(out var request))
                    request = new Request(new byte[MaxBufferSize]);

                request.AssignSocket(acceptedSocket);

                acceptedSocket.BeginReceive(request.Body, 0, request.Body.Length, SocketFlags.None, ProcessRequest, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                CloseSocket(acceptedSocket);
            }
        }

        private static void ProcessRequest(IAsyncResult asyncResult)
        {
            var request = (Request)asyncResult.AsyncState;

            try
            {
                var readBytes = request.Socket.EndReceive(asyncResult);
                var response = request.Process(readBytes);
                request.Socket.BeginSend(response, 0, response.Length, SocketFlags.None, ProcessResponse, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                CloseSocket(request.Socket);
            }
        }

        private static void ProcessResponse(IAsyncResult asyncResult)
        {
            var request = (Request)asyncResult.AsyncState;

            try
            {
                request.Socket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                CloseSocket(request.Socket);

                request.Reset();
                RequestPool.Add(request);
            }
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
