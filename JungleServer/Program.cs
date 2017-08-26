using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JungleServer
{
    class Program
    {
        static Message msg = new Message();

        static void Main(string[] args)
        {
            StartServerAsync();
            Console.ReadKey();
        }

        static void StartServerAsync()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 9527);
            serverSocket.Bind(ipEndPoint);//绑定ip和端口号
            serverSocket.Listen(10);//开始监听端口号

             
            //Socket clientSocket = serverSocket.Accept();//接收一个客户端连接
            serverSocket.BeginAccept(AcceptCallBack,serverSocket);

        }

        static void AcceptCallBack(IAsyncResult ar)
        {
            Socket serverSocket = ar.AsyncState as Socket;
            Socket clientSocket = serverSocket.EndAccept(ar);

            //向客户端发送一个消息
            string msgStr = "Hello client";
            byte[] data = Encoding.UTF8.GetBytes(msgStr);
            clientSocket.Send(data);

            clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallBack, clientSocket);

            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket clientSocket = null;
            try
            {
	            clientSocket = ar.AsyncState as Socket;
	            int count = clientSocket.EndReceive(ar);
                //clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);
                if (count == 0)
                {
                    clientSocket.Close();
                    return;
                }
                msg.AddCount(count);
                msg.ReadMessage();
                clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallBack, clientSocket);
                //   string msgStr = Encoding.UTF8.GetString(dataBuffer, 0, count);
                //Console.WriteLine("从客户端接收到数据："  + msgStr);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                if (clientSocket != null)
                    clientSocket.Close();
            }
            finally
            {
               
            }
        }

        void StartServerSync()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 9527);
            serverSocket.Bind(ipEndPoint);//绑定ip和端口号
            serverSocket.Listen(10);//开始监听端口号
            Socket clientSocket = serverSocket.Accept();//接收一个客户端连接

            //向客户端发送一个消息
            string msg = "Hello client";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            //接收客户端的一条消息
            byte[] dataBuffer = new byte[1024];
            int count = clientSocket.Receive(dataBuffer);
            string msgReceive = Encoding.UTF8.GetString(dataBuffer, 0, count);
            Console.WriteLine(msgReceive);

            Console.ReadKey();
            clientSocket.Close();
            serverSocket.Close();
        }
    }
}
