using System;
using System.Collections.Generic;
using ClientLib;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to chat application");
            Client c1 = new Client();
            Console.WriteLine(c1.Id);
            
            //InboxPrinter(c1.Inbox);
            Console.Read();
        }

        static void MessagePrinter(Message message)
        {
            Console.WriteLine("Sender ID:\t{0}", message.SenderClientID);
            Console.WriteLine("Receiver ID:\t{0}", message.ReceiverClientID);
            Console.WriteLine("Message:\t{0}", message.MessageBody);
            Console.WriteLine("Broadcast:\t{0}", message.Broadcast);
        }

        static void InboxPrinter(Queue<Message> Inbox)
        {
            foreach (Message message in Inbox)
            {
                MessagePrinter(message);
            }
        }

        static Message GetInputFromUser()
        {
            Console.WriteLine("Input Sender ID");
            string sender = Console.ReadLine();
            
            Console.WriteLine("Input Receiver ID");
            string receiver = Console.ReadLine();
            
            Console.WriteLine("Type Messsage");
            string message = Console.ReadLine();
            
            Console.WriteLine("Is it Broadcast? Type yes or no)");
            string inputString = Console.ReadLine();
            bool broadcast = inputString.ToLower() == "yes" || inputString.ToLower() == "y";

            Message m1 = new Message
            {
                Broadcast = broadcast,
                SenderClientID = sender,
                ReceiverClientID = receiver,
                MessageBody = message
            };

            return m1;
        }

        

        //public void PrintInbox(Client c)
        //{
        //    foreach (message in c.Inbox)
        //    {
        //        MessagePrinter(message);
        //    }
        //}
    }
}
