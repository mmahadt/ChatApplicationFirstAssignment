using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientLib;

namespace ConsoleApp1
{
    public class Client
    {
        private string Id;
        public Queue<Message> Inbox = new Queue<Message>();
        
        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient();
            NetworkStream serverStream;

            Console.WriteLine("Client Started");
            clientSocket.Connect(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 8888);
            Console.WriteLine("Client Socket Program - Server Connected...");

            serverStream = clientSocket.GetStream();

            Console.WriteLine("My Id is " + ReceiveFromServerStream(serverStream));

            Thread receiverThread = new Thread(() => ReceiverPrinterThreadFunction(serverStream));
            receiverThread.Start();

            while (true)
            {
                SendToServerStream(serverStream);
            }

            //Console.Read();
        }

        private static string GetMessageText(string msg)
        {
            string[] words = msg.Split('_');
            return words[0];
        }

        private static string GetSenderId(string msg)
        {
            string[] words = msg.Split('_');
            return words[1];
        }
        private static string GetReceiverId(string msg)
        {
            string[] words = msg.Split('_');
            return words[2];
        }
        private static bool IsBroadcast(string msg)
        {
            string[] words = msg.Split('_');
            return (words[3] == "1");
        }

        private static void ReceiverPrinterThreadFunction(NetworkStream stream)
        {
            while (true)
            {
                string dataFromServer = ReceiveFromServerStream(stream);

                //Print the message to the console
                //Console.WriteLine(" >> " + "Message received from Client-{0}", GetSenderId(dataFromServer) + "\t" + GetMessageText(dataFromServer) + "\n");

                Console.WriteLine(dataFromServer);

            }
        }

        //https://stackoverflow.com/questions/7099875/sending-messages-and-files-over-networkstream
        private static string ReceiveFromServerStream(NetworkStream serverStream)
        {
            //Read the length of incoming message from the server stream
            byte[] msgLengthBytes1 = new byte[sizeof(int)];
            serverStream.Read(msgLengthBytes1, 0, msgLengthBytes1.Length);
            //store the length of message as an integer
            int msgLength1 = BitConverter.ToInt32(msgLengthBytes1, 0);

            //create a buffer for incoming data of size equal to length of message
            byte[] inStream = new byte[msgLength1];
            //read that number of bytes from the server stream
            serverStream.Read(inStream, 0, msgLength1);
            //convert the byte array to message string
            string dataFromServer = Encoding.ASCII.GetString(inStream);

            //Console.WriteLine(dataFromServer);

            return dataFromServer;
        }

        private static void SendToServerStream(NetworkStream serverStream)
        {
            
            Console.WriteLine("Type a message to be sent to the server.\n");
            Console.WriteLine("Follow message format\n\n message_senderID_ReceiverId_BroadcastTrue/False\n\n");
            string message = Console.ReadLine();

            //Get the length of message in terms of number of bytes
            int messageLength = Encoding.ASCII.GetByteCount(message);

            //lengthBytes are first 4 bytes in stream that contain
            //message length as integer
            byte[] lengthBytes = BitConverter.GetBytes(messageLength);
            serverStream.Write(lengthBytes, 0, lengthBytes.Length);

            //Write the message to the server stream
            byte[] outStream = Encoding.ASCII.GetBytes(message);
            serverStream.Write(outStream, 0, outStream.Length);
           
            //ReceiveFromServerStream(serverStream);
            serverStream.Flush();
        }
    }
}