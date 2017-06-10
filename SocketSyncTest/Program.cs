using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketSyncTest
{
    class Program
    {
        static Socket listeningSocket;
        static Socket socket;
        static Thread thrReadRequest;
        static int iPort = 19871;
        static int iConnectionQueue = 100;

        static void Main(string[] args)
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            IPAddress servAddr = addr[5];
            /*foreach (var item in addr)
            {
                Console.WriteLine(item.ToString());
            }*/

            //Console.WriteLine(IPAddress.Parse(getLocalIPAddress()).ToString());
            Console.WriteLine(servAddr.ToString());
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //listeningSocket.Bind(new IPEndPoint(0, iPort));                                
                listeningSocket.Bind(new IPEndPoint(servAddr, iPort));
                listeningSocket.Listen(iConnectionQueue);

                thrReadRequest = new Thread(new ThreadStart(getRequest));
                thrReadRequest.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                //throw;
            }
        }

        static private void getRequest()
        {
            int i = 0;
            while (true)
            {
                i++;
                Console.WriteLine("Outside Try i = {0}", i.ToString());

                try
                {
                    socket = listeningSocket.Accept();
                    Console.WriteLine("Connectd...");
                    while (true)
                    {
                        try
                        {

                            // Receiving

                            byte[] buffer = new byte[socket.SendBufferSize];
                            int iBufferLength = socket.Receive(buffer, 0, buffer.Length, 0);
                            if (iBufferLength == 0) throw new SocketException();    // patch for cannot catching exception when android connection is lost
                            Console.WriteLine("Received {0}", iBufferLength);
                            Array.Resize(ref buffer, iBufferLength);
                            string formattedBuffer = Encoding.ASCII.GetString(buffer);

                            Console.WriteLine("Android Says: {0}", formattedBuffer);

                            if (formattedBuffer == "quit")
                            {
                                socket.Close();
                                listeningSocket.Close();
                                Console.WriteLine("Exiting");
                                Environment.Exit(0);
                            }

                            //Console.WriteLine("Inside Try i = {0}", i.ToString());
                            Thread.Sleep(500);
                        }
                        catch (SocketException e)
                        {
                            //socket.Close();
                            Console.WriteLine("Receiving error: " + e.ToString());
                            //Console.ReadKey();
                            //throw;
                            break;
                        }


                    }
                }
                catch (Exception e)
                {
                    //socket.Close();
                    Console.WriteLine("Error After Loop: " + e.ToString());
                }
                finally
                {
                    Console.WriteLine("Closing Socket");
                    socket.Close();
                    //listeningsocket.close();
                }
            }
        }

        static private string getLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        /*
        static void Main(string[] args)
        {
            //SocketListener listener = new SocketListener();
            //listener.StartListening();

        }
        */
    }
}
