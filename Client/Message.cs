using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLib
{
    [Serializable]
    public class Message : IMessage
    {
        public string SenderClientID
        {
            get { return SenderClientID; }
            set { SenderClientID = value; }
        }
        public string ReceiverClientID
        {
            get { return ReceiverClientID; }
            set { ReceiverClientID = value; }
        }
        public string MessageBody
        {
            get { return MessageBody; }
            set { MessageBody = value; }
        }

        public bool Broadcast
        {
            get { return Broadcast; }
            set { Broadcast = value; }
        }
    }
}
