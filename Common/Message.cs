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
        private long sender;
        private string content;
        private DateTime time;
        private Color color;

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
    }
}
