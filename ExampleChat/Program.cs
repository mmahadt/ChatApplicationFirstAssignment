using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using ClientLib;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExampleChat
{
    class Program
    {
        //A list of strings to contain client Ids
        private static List<handleClinet> listOfClients = new List<handleClinet>();

        public static Queue<Message> Outbox = new Queue<Message>();

        private static void MessageSender()
        {
            while (true)
            {
                if (Outbox.Count != 0)
                {
                    Message message = Outbox.Peek();
                    if (message.Broadcast)
                    {
                        Console.WriteLine(">> Broadcast message from client\t" + message.MessageBody);
                        Broadcast(message);
                        Outbox.Dequeue();
                    }
                    else
                    {
                        Console.WriteLine(">> Unicast message from client\t" + message.MessageBody);
                        Unicast(message);
                        Outbox.Dequeue();
                    }
                }
            }
        }

        public static void Unicast(Message msg)
        {
            foreach (handleClinet client in listOfClients)
            {
                if (client.clNo == msg.ReceiverClientID)
                //send message to intended recipient only
                {
                    handleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        public static void Broadcast(Message msg)
        {
            foreach (handleClinet client in listOfClients)
            {
                if (client.clNo != msg.SenderClientID) //send the message to all 
                                             //clients except the sender
                {
                    handleClinet.SendOverNetworkStream(msg, client.clientSocket.GetStream());
                }
            }
        }

        static void Main(string[] args)
        {
            //Read the port number from app.config file
            int port = int.Parse(ConfigurationManager.AppSettings["connectionManager:port"]);

            TcpListener serverSocket = new TcpListener(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], port);

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
                
                

                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " connected!");
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, Convert.ToString(counter));

                //Make a list of clients
                listOfClients.Add(client);

            }

            //clientSocket.Close();
            //serverSocket.Stop();
            //Console.WriteLine(" >> " + "exit");
            //Console.ReadLine();
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

            Message m1 = new Message
            {
                Broadcast = false,
                SenderClientID = null,
                ReceiverClientID = Convert.ToString(clNo),
                MessageBody = Convert.ToString(clNo)
            };
            SendOverNetworkStream(m1, clientSocket.GetStream());

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
                    break;
                }
                catch (System.IO.IOException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    //Thread.CurrentThread.Abort();
                }
            }
        }

        public static void SendOverNetworkStream(Message message, NetworkStream serverStream)
        {
            var bin = new BinaryFormatter();
            bin.Serialize(serverStream, message);
            serverStream.Flush();
        }

        public static Message ReadFromNetworkStream(NetworkStream serverStream)
        {
            // Client side
            var bin = new BinaryFormatter();
            Message m1 = (Message)bin.Deserialize(serverStream);
            serverStream.Flush();
            Console.WriteLine(" >> " + "From client-" + m1.SenderClientID + "\t" + m1.MessageBody);
            return m1;
        }
    }
}