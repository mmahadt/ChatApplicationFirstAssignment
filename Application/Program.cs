using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientLib
{
    public class ClientApplication
    {
        Client client = new Client();

        public Message GetInputFromUser()
        {
            Console.WriteLine("\n\n\nType Messsage");
            string message = Console.ReadLine();

            Console.WriteLine("Is it Broadcast? Type (yes or no)");
            string inputString = Console.ReadLine();
            bool broadcast = inputString.ToLower() == "yes" || inputString.ToLower() == "y";

            if (!broadcast)
            {
                Console.WriteLine("Input Receiver ID");
                string receiver = Console.ReadLine();
                return client.StringsToMessageObject(receiver, message, false);
            }
            else
            {
                return client.StringsToMessageObject(null, message, true);
            }
        }

        static void MessagePrinter(Message message)
        {
            Console.WriteLine("___________New Message____________");
            Console.WriteLine("Sender ID:\t{0}", message.SenderClientID);
            //Console.WriteLine("Receiver ID:\t{0}", message.ReceiverClientID);
            Console.WriteLine("Message:\t{0}", message.MessageBody);
            Console.WriteLine("Broadcast:\t{0}", message.Broadcast);
            Console.WriteLine("______________________________________");
        }

        static void InboxPrinter(Queue<Message> Inbox)
        {
            while(true)
            {
                if(Inbox.Count != 0)
                {
                    
                    MessagePrinter(Inbox.Dequeue());
                       
                }
            }
            
        }

        static void Main(string[] args)
        {
            ClientApplication clientApplication = new ClientApplication();
            clientApplication.client.Start();

            Console.WriteLine("Welcome to Chat application");
            Console.WriteLine("I am a client application and my Id is " + clientApplication.client.Id);

            //foreach(string cl in clientApplication.client.listOfOtherClients)
            //{
            //    Console.WriteLine(cl);
            //}
            Thread messagePrinterThread = new Thread(() => InboxPrinter(clientApplication.client.Inbox));
            messagePrinterThread.Start();

            while (true)
            {
                Message m1 = clientApplication.GetInputFromUser();
                if (m1.Broadcast)
                {
                    clientApplication.client.Broadcast(m1);
                }
                else
                {
                    clientApplication.client.Unicast(m1);
                }
            }
        }

    }
        
}