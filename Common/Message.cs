using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

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

        private bool isPicture;
        private string picture;

        public override string ToString()
        {
            return content;
        }

        public object Value
        {
            get
            {
                if (IsPicture && !string.IsNullOrEmpty(Picture))
                {
                    byte[] data = Convert.FromBase64String(Picture);
                    using (var ms = new MemoryStream(data))
                    {
                        return Image.FromStream(ms);
                    }
                }
                else
                {
                    return Content;
                }
            }
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

        [DataMember]
        public bool IsPicture
        {
            get { return isPicture; }
            set { isPicture = value; }
        }

        [DataMember]
        public string Picture
        {
            get { return picture; }
            set { picture = value; }
        }

    }
}
