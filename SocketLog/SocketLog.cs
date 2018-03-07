using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _ip = "192.168.8.171";//args[0];
            _port = "8899";//args[1];

            Socket s = GetSocket();
            
            s.BeginReceive(new byte[] { 0 }, 0, 0, SocketFlags.None, messageCallback, null);

            while(true)
            {
                var tmp = Console.ReadLine();
                if(!string.IsNullOrWhiteSpace(tmp))
                {
                    var utf8 = Encoding.Convert(
                        Encoding.Default
                        , Encoding.UTF8
                        , Encoding.Default.GetBytes(tmp));
                    var bytes = Encoding.Default.GetBytes(tmp);
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
