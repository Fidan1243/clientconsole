using ConsoleApp1;
using Newtonsoft.Json;
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
            Console.ReadLine();
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
            var jsonst = Encoding.ASCII.GetString(recbuffer);
            var us = JsonConvert.DeserializeObject<User>(jsonst);
            Console.WriteLine(us.Name);
            Console.WriteLine(us.Surname);
            Console.WriteLine(us.Age);
            Console.WriteLine(us.Message);
            Console.WriteLine("Received request: ",jsonst);

            if (jsonst.ToLower() != String.Empty)
            {
                byte[] data = Encoding.ASCII.GetBytes(jsonst);
                current.Send(data);
                foreach (var item in clientsockets)
                {
                    if (item.RemoteEndPoint != current.RemoteEndPoint)
                    {
                        current.Send(data);
                    }
                }

            }
            else if (jsonst.ToLower() == "exit")
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
