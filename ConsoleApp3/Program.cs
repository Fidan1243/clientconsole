using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        public static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int BUFFER_SIZE = 1024;
        private static byte[] buffer = new byte[BUFFER_SIZE];
        private static List<Socket> clientsockets = new List<Socket>();
        static void Main(string[] args)
        {
            Console.Title = "Server";
            server.Bind(new IPEndPoint(IPAddress.Parse(NetworkHelpers.Class1.IP), int.Parse(NetworkHelpers.Class1.Port)));
            server.Listen(5);
            server.BeginAccept(AcceptCallBack, null);
        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            Socket socket;
            try
            {
                socket = server.EndAccept(ar);
            }
            catch (Exception ex)
            {
                return;
            }
            clientsockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None,ReceiveCallBack,socket);
            Console.WriteLine(socket.RemoteEndPoint.ToString(),": Client connected waiting for request");
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket current = ar.AsyncState as Socket;
            int received;
            try
            {
                received = current.EndReceive(ar);
            }
            catch (Exception)
            {
                Console.WriteLine("Client disconnected");
                current.Close();
                clientsockets.Remove(current);
                return;
            }
            byte[] recbuffer = new byte[received];
            Array.Copy(buffer, recbuffer, received);
            string text = Encoding.ASCII.GetString(recbuffer);
            Console.WriteLine("Received request: ",text);

            if (text.ToLower() == "get-time")
            {
                byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                current.Send(data);
                Console.WriteLine("Time sent to the client");

            }
            else if (text.ToLower() == "exit")
            {
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                clientsockets.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, current);
        }
    }
}
