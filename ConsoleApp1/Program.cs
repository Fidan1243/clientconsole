using ConsoleApp3.NetworkHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static User User { get; set; } = new User() { Name = "123",Surname = "null",Age = 34 };
        static void Main(string[] args)
        {
            Console.Title = "Client";
            ConnectToServer();
            RequestLoop();
            Exit();
        }

        private static void Exit()
        {
            SendString("exit");
            Client.Shutdown(SocketShutdown.Both);
            Client.Close();
            Environment.Exit(0);
        }

        private static void SendString(string v)
        {
            User.Message = v;
            var jsonst = JsonConvert.SerializeObject(User);
            byte[] buffer = Encoding.ASCII.GetBytes(jsonst);
            Client.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void RequestLoop()
        {
            var sender = Task.Run(() =>
            {
                while (true)
                {
                    SendRequest();
                }
            });
            var receiver = Task.Run(() =>
            {
                while (true)
                {
                    ReceiveRequest();
                }
            });

            Task.WaitAll(sender, receiver);
        }

        private static void ReceiveRequest()
        {
            var buffer = new byte[2048];
            int received = Client.Receive(buffer, SocketFlags.None);
            if (received == 0)
            {
                return;
            }
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);
        }

        private static void SendRequest()
        {
            Console.WriteLine("Send Message: ");
            string request = Console.ReadLine();
            SendString(request);
            if (request.ToLower() == "exit")
            {
                Exit();
            }
        }

        private static void ConnectToServer()
        {
            int attempts = 0;
            while (!Client.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempts: " + attempts);
                    Client.Connect(Class1.IP, int.Parse(Class1.Port));


                }
                catch (Exception)
                {
                    Console.Clear();
                }
            }
            Console.Clear();
            Console.WriteLine("Connected Successfully");
        }
    }
}
