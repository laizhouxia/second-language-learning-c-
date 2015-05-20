using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Web;
using System.Threading;
using System.Collections;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace ConsoleApplication4
{
    class Program
    {
        static void Main(string[] args)
        {
            string posTaggerResult = ChineseSegmentation("这是一句用来测试的话");
            Console.WriteLine(posTaggerResult);

            //try
            //{
            //    TcpClient MyClient = new TcpClient();
            //    MyClient.Connect("localhost", Convert.ToInt16("1237"));
            //    Stream MyStream = MyClient.GetStream();
            //    UnicodeEncoding MyInfo = new UnicodeEncoding();
            //    byte[] MyData = new byte[256];
            //    byte[] MySendByte = MyInfo.GetBytes("你好我是客户端");
            //    MyStream.Write(MySendByte, 0, MySendByte.Length);
            //    MyStream.Read(MyData, 0, 256);
            //    Console.WriteLine(Encoding.Unicode.GetString(MyData));
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("信息提示");
            //}


            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }


        private static string ChineseSegmentation(string s)
        {
            try
            {
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                Int32 port = 1238;
                TcpClient client = new TcpClient("localhost", port);

                // Translate the passed message into ASCII and store it as a Byte array.
                string message = s + "\n";
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response. 

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();

                return Convert.ToString(responseData);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return "";
        }
    }
}
