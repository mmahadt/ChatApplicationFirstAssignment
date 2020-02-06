using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientLib
{
    public class Client
    {
        //id is set for the first time and then becomes read only
        //private string _id = "";  // Backing store (Real id)
        public string Id;
        //{
        //    get => _id;
        //    set
        //    {
        //        if (value == "")
        //        {
        //            _id = value;
        //        }
        //    }
        //}

        TcpClient clientSocket;
        NetworkStream serverStream;

        public void Start()
        {
            try
            {
                clientSocket = new TcpClient();

                clientSocket.Connect(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 8888);

                serverStream = clientSocket.GetStream();

                Id = ReceiveFromServerStream();
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
            
        }     

        public Message StringsToMessageObject(string receiver,string message,bool broadcast)
        {         
            if (!broadcast)
            {
                Message m1 = new Message()
                {
                    Broadcast = false,
                    SenderClientID = Id,
                    ReceiverClientID = receiver,
                    MessageBody = message
                };
                return m1;
            }
            else
            {
                Message m1 = new Message()
                {
                    Broadcast = true,
                    SenderClientID = Id,
                    ReceiverClientID = receiver,
                    MessageBody = message
                };
                return m1;
            }
        }


        //public bool Broadcast(Message message)
        //{
        //    try
        //    {
        //        SendToServerStream(message);
        //        return true;//success
        //    }
        //    catch(Exception ex)
        //    {
        //        return false;//failure
        //    }
        //}

        //public bool Unicast(Message message)
        //{
        //    try
        //    {
        //        SendToServerStream(message);
        //        return true;//success
        //    }
        //    catch
        //    {
        //        return false;//failure
        //    }
            
        //}

        //https://stackoverflow.com/questions/7099875/sending-messages-and-files-over-networkstream
        private string ReceiveFromServerStream()
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

        private void SendToServerStream(string message)
        { 
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

        //private void SendToServerStream(Message message)
        //{
        //    //Get the length of message in terms of number of bytes
        //    int messageLength = Encoding.ASCII.GetByteCount(message);

        //    //lengthBytes are first 4 bytes in stream that contain
        //    //message length as integer
        //    byte[] lengthBytes = BitConverter.GetBytes(messageLength);
        //    serverStream.Write(lengthBytes, 0, lengthBytes.Length);

        //    //Write the message to the server stream
        //    byte[] outStream = Encoding.ASCII.GetBytes(message);
        //    serverStream.Write(outStream, 0, outStream.Length);

        //    //ReceiveFromServerStream(serverStream);
        //    serverStream.Flush();
        //}
    }
}