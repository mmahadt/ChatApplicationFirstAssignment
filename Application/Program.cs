using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientLib
{
    public class ClientApplication
    {
        Client C1 = new Client();
        public Message GetInputFromUser()
        {
            Console.WriteLine("Input Sender ID");
            string sender = Console.ReadLine();


            Console.WriteLine("Type Messsage");
            string message = Console.ReadLine();

            Console.WriteLine("Is it Broadcast? Type (yes or no)");
            string inputString = Console.ReadLine();
            bool broadcast = inputString.ToLower() == "yes" || inputString.ToLower() == "y";

            if (broadcast)
            {
                Console.WriteLine("Input Receiver ID");
                string receiver = Console.ReadLine();
                return C1.StringsToMessageObject(receiver, message, true);
            }
            else
            {
                return C1.StringsToMessageObject(null, message, false);
            }
        }


        static void Main(string[] args)
        {
            ClientApplication CA = new ClientApplication();

            CA.C1.Start();

            Console.WriteLine("Client Socket Program - Server Connected...");
            Console.WriteLine("My Id is " + CA.C1.Id);

            Message m1 = CA.GetInputFromUser();
            //if (m1.Broadcast)
            //{
            //    CA.C1.Broadcast(m1);
            //}
            //else
            //{
            //    CA.C1.Unicast(m1);
            //}
            Console.ReadLine();
        }




    }
        
}