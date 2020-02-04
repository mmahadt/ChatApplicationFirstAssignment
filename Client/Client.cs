using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientLib
{
    public class Client
    {
        public string Id;
        public Queue<Message> Inbox = new Queue<Message>();
        private NetworkStream serverStream;
        private TcpClient clientSocket;
        
        public Client()
        {
            try
            {
                clientSocket = new TcpClient();

                clientSocket.Connect(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 8888);

                Message dataFromServer = ReceiveFromServerStream();
                Inbox.Enqueue(dataFromServer);

                Thread receiverThread = new Thread(MessageReceiverThreadFunction);
                receiverThread.Start();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch (System.IO.IOException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" >> " + ex.ToString());
            }
            //First message sent to each client is from server containing
            //id assigned by the server
            
            Id = Inbox.Dequeue().MessageBody;
            
            
        }

        public Message Broadcast(string message)
        {
            Message m = new Message
            {
                Broadcast = true,
                SenderClientID = Id,
                MessageBody = message,
                ReceiverClientID = null
            };
            SendToServerStream(m);
            return m;
        }

        public Message Unicast(string message, string receiverId)
        {
            Message m = new Message
            {
                Broadcast = false,
                SenderClientID = Id,
                MessageBody = message,
                ReceiverClientID = receiverId
            };
            SendToServerStream(m);
            return m;
        }

        private void MessageReceiverThreadFunction()
        {
            while (true)
            {
                if (serverStream.DataAvailable)
                {
                    Message dataFromServer = ReceiveFromServerStream();
                    Inbox.Enqueue(dataFromServer);
                }         
            }
        }

        public void SendToServerStream(Message message)
        {
            var bin = new BinaryFormatter();
            bin.Serialize(serverStream, message);
            serverStream.Flush();
        }

        public Message ReceiveFromServerStream()
        {
            // Client side
            
            var bin = new BinaryFormatter();
            Message m1 = (Message)bin.Deserialize(serverStream);
            
            serverStream.Flush();
            return m1;
        }
    }
}
