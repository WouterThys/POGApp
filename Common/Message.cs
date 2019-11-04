using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Message
    {
        public static Message CreateInfo(Client client)
        {
            return new Message()
            {
                Sender = client.Id,
                Content = client.Name + " - " + DateTime.Now.ToString("dd/MM/yyyy H:mm"),
                Time = DateTime.Now,
                Color = Color.Gray,
                Info = true
            };
        }

        private long sender;
        private string content;
        private DateTime time;
        private Color color;
        private bool info;

        public override string ToString()
        {
            return content;
        }

        [DataMember]
        public long Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        [DataMember]
        public string Content
        {
            get { return content ?? ""; }
            set { content = value; }
        }

        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        [DataMember]
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        [DataMember] 
        public bool Info
        {
            get { return info; }
            set { info = value; }
        }
    }
}
