using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketLog
{
    class SocketLog
    {
        private static Socket ConnectSocket(string server, int port)
        {
            Socket s = null;
            //var hostEntry = Dns.GetHostEntry(server);

            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            //foreach(IPAddress address in hostEntry.AddressList)
            {
                IPAddress address = IPAddress.Parse(server);
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if(tempSocket.Connected)
                {
                    s = tempSocket;
                    Console.WriteLine("connected {0}", s);
                    //break;
                }
                else
                {
                    Console.WriteLine("try next");
                }
            }
            return s;
        }

        static string _ip, _port;
        static void Main(string[] args)
        {
            string ipport = "192.168.8.171:8899";
            var configfile = ".logcat.cfg.txt";
            if(File.Exists(configfile))
            {
                ipport = File.ReadAllText(configfile);
            }

            if(args.Length == 1)
            {
                ipport = args[0];
                goto go;
            }

            Console.WriteLine("usage: LogCat.exe ip port\n或按下面提示操作");
            Console.WriteLine("输入目标 ip 和 port, 以分号[:]分割, 直接按 ENTER 使用默认值: " + ipport);
            Console.Write("输入: ");
            var tmpInput = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmpInput) && tmpInput.Contains(":"))
            {
                ipport = tmpInput;
            }
            else
            {
                Console.WriteLine("输入错误, 将使用默认值");
            }
            Console.WriteLine("监听目标为: " + ipport);

            go:
            var stream = new StreamRedirect.StreamRedirect("./test1.txt");
            stream.Add("./test2.txt");
            stream.Add("./test3.txt");
            stream.Add(Console.Out);
            stream.AutoFlush = true;
            Console.SetOut(stream);

            File.WriteAllText(configfile, ipport);
            var split = ipport.Split(':');
            _ip = split[0];
            _port = split[1];

            Socket s = GetSocket();
            
            s.BeginReceive(new byte[] { 0 }, 0, 0, SocketFlags.None, messageCallback, null);

            while(true)
            {
                tmpInput = Console.ReadLine();
                if(!string.IsNullOrWhiteSpace(tmpInput))
                {
                    var utf8 = Encoding.Convert(
                        Encoding.Default
                        , Encoding.UTF8
                        , Encoding.Default.GetBytes(tmpInput));
                    var bytes = Encoding.Default.GetBytes(tmpInput);
                    s.Send(bytes, SocketFlags.None);
                }
            }

            // Close the application only when the close button is clicked
            Process.GetCurrentProcess().WaitForExit();
        }

        static Socket _socket = null;
        private static Socket GetSocket()
        {
            if (_socket == null)
            {
                _socket = ConnectSocket(_ip, int.Parse(_port));
            }
            return _socket;
        }
        private static void messageCallback(IAsyncResult AsyncResult)
        {
            try
            {
                GetSocket().EndReceive(AsyncResult);

                // Read the incomming message 
                byte[] messageBuffer = new byte[1024];
                int bytesReceived = GetSocket().Receive(messageBuffer);

                // Resize the byte array to remove whitespaces 
                if(bytesReceived < messageBuffer.Length)
                    Array.Resize<byte>(ref messageBuffer, bytesReceived);

                //// Get the opcode of the frame
                //EOpcodeType opcode = Helpers.GetFrameOpcode(messageBuffer);

                var msg = Encoding.UTF8.GetString(messageBuffer).TrimEnd(new char[] { '\n', '\r' });
                Console.WriteLine(msg);

                // Start to receive messages again
                GetSocket().BeginReceive(new byte[] { 0 }, 0, 0, SocketFlags.None, messageCallback, null);
            }
            catch(Exception Exception)
            {
                GetSocket().Close();
                GetSocket().Dispose();
            }
        }
    }
}
