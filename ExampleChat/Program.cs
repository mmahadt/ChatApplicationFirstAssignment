using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ClientLib;

namespace ExampleChat
{
    class Program
    {
        //A list of strings to contain client Ids
        private static List<handleClinet> listOfClients = new List<handleClinet>();

        //public static Queue<string> Outbox = new Queue<string>();

        public static Queue<Message> Outbox = new Queue<Message>();
        //private static string GetSenderId(string msg)
        //{
        //    string[] words = msg.Split('_');
        //    return words[1];
        //}
        //private static string GetReceiverId(string msg)
        //{
        //    string[] words = msg.Split('_');
        //    return words[2];
        //}
        //private static bool IsBroadcast(string msg)
        //{
        //    string[] words = msg.Split('_');
        //    return (words[3] == "1");
        //}

        private static void MessageSender()
        {
            while (true)
            {
                if (Outbox.Count != 0)
                {
                    Message message = Outbox.Peek();
                    if (message.Broadcast)
                    {
                        Console.WriteLine(">> Broadcast message from client\t" + message);
                        Broadcast(message, message.SenderClientID);
                        Outbox.Dequeue();
                    }
                    else
                    {
                        Console.WriteLine(">> Unicast message from client\t" + message);
                        Unicast(message, message.ReceiverClientID);
                        Outbox.Dequeue();
                    }
                }
            }
        }

        public static void Unicast(Message msg, string receiverId)
        {
            foreach (handleClinet client in listOfClients)
            {
                if (client.clNo == receiverId)
                //send message to intended recipient only
                {
                    //clientMapping[clientid].Send(Encoding.ASCII.GetBytes(msg));
                    handleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        public static void Broadcast(Message msg, string senderId)
        {
            foreach (handleClinet client in listOfClients)
            {
                if (client.clNo != senderId) //send the message to all 
                                             //clients except the sender
                {
                    //clientMapping[clientid].Send(Encoding.ASCII.GetBytes(msg));
                    handleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        static void Main(string[] args)
        {
            //Read the port number from app.config file
            int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);

            TcpListener serverSocket = new TcpListener(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 8888);

            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            counter = 0;

            Thread senderThread = new Thread(MessageSender);
            senderThread.Start();

            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, Convert.ToString(counter));

                //Make a list of clients
                listOfClients.Add(client);

            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
        }
    }



    //Class to handle each client request separatly
    public class handleClinet
    {
        public TcpClient clientSocket;
        public string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Message m1 = new Message()
            {
                Broadcast = false,
                SenderClientID = null,
                ReceiverClientID = Convert.ToString(clineNo),
                MessageBody = Convert.ToString(clineNo)
            };
            SendOverNetworkStream(m1, clientSocket.GetStream());
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            //byte[] bytesFrom = new byte[10025];
            Message dataFromClient = null;
            //Byte[] sendBytes = null;
            //string serverResponse = null;
            //string rCount = null;
            //requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount += 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    while (clientSocket.Connected)
                    {
                        if (networkStream.CanRead)
                        {
                            dataFromClient = ReadFromNetworkStream(networkStream);
                            Program.Outbox.Enqueue(dataFromClient);
                        }
                        else
                        {
                            networkStream.Close();
                            return;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Client {0} disconnected.", clNo);
                    break;
                }
                catch (System.IO.IOException)
                {
                    Console.WriteLine("Client {0} disconnected.", clNo);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    //Thread.CurrentThread.Abort();
                }
            }
        }

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Message obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        //public static void SendOverNetworkStream(string dataFromClient, NetworkStream networkStream)
        //{
        //    //Get the length of message in terms of number of bytes
        //    int messageLength = Encoding.ASCII.GetByteCount(dataFromClient);

        //    //lengthBytes are first 4 bytes in stream that contain
        //    //message length as integer
        //    byte[] lengthBytes = BitConverter.GetBytes(messageLength);
        //    networkStream.Write(lengthBytes, 0, lengthBytes.Length);

        //    //Write the message to the server stream
        //    byte[] outStream = Encoding.ASCII.GetBytes(dataFromClient);
        //    networkStream.Write(outStream, 0, outStream.Length);
        //    networkStream.Flush();
        //}

        public static void SendOverNetworkStream(Message dataFromClient, NetworkStream networkStream)
        {
            byte[] message = ObjectToByteArray(dataFromClient);
            //Get the length of message in terms of number of bytes
            int messageLength = message.Length;

            //lengthBytes are first 4 bytes in stream that contain
            //message length as integer
            byte[] lengthBytes = BitConverter.GetBytes(messageLength);
            networkStream.Write(lengthBytes, 0, lengthBytes.Length);

            //Write the message to the server stream
            byte[] outStream = message;
            networkStream.Write(outStream, 0, outStream.Length);
            networkStream.Flush();
        }


        public static Message ReadFromNetworkStream(NetworkStream networkStream)
        {
            //Read the length of incoming message from the server stream
            byte[] msgLengthBytes1 = new byte[sizeof(int)];
            networkStream.Read(msgLengthBytes1, 0, msgLengthBytes1.Length);
            //store the length of message as an integer
            int msgLength1 = BitConverter.ToInt32(msgLengthBytes1, 0);

            //create a buffer for incoming data of size equal to length of message
            byte[] inStream = new byte[msgLength1];
            //read that number of bytes from the server stream
            networkStream.Read(inStream, 0, msgLength1);
            //convert the byte array to message string
            //string dataFromServer = Encoding.ASCII.GetString(inStream);

            //Console.WriteLine(dataFromServer);
            Message dataFromServer = (Message)ByteArrayToObject(inStream);
            return dataFromServer;
        }

        //private string ReadFromNetworkStream(NetworkStream networkStream)
        //{
        //    string dataFromClient;
        //    byte[] msgLengthBytes = new byte[sizeof(int)];
        //    networkStream.Read(msgLengthBytes, 0, msgLengthBytes.Length);
        //    int msgLength = BitConverter.ToInt32(msgLengthBytes, 0);

        //    byte[] inStream = new byte[msgLength];//buffer for incoming data
        //    networkStream.Read(inStream, 0, msgLength);
        //    dataFromClient = Encoding.ASCII.GetString(inStream);
        //    Console.WriteLine(" >> " + "From client-" + clNo + "\t" + dataFromClient);

        //    return dataFromClient;
        //}
    }
}