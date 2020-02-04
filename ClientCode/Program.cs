using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;

namespace ClientCode
{
    public class Client
    {
        public static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();

        public static void Main(string[] args)
        {

            Console.WriteLine("Client Started");
            try
            {
                clientSocket.Connect("127.0.0.1", 8888);

                NetworkStream serverStream = clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes("Message from Client$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                do
                {
                    byte[] inStream = new byte[1024];
                    serverStream.Read(inStream, 0, inStream.Length);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    Console.WriteLine("Data from Server : " + returndata);
                } while (serverStream.DataAvailable);
            }
            catch (Exception e){
                Console.WriteLine("{0}", e.Message);
            }
        }
        
    }
}
