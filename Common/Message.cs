using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum MessageType
    {
        [EnumMember]
        Message = 0,
        [EnumMember]
        Info = 1,
        [EnumMember]
        Photo = 2,
        [EnumMember]
        Sound = 3
    }

    [DataContract]
    public class Message
    {
        private static Message Create(Client client)
        {
            return new Message()
            {
                Sender = client.Id,
                Time = DateTime.Now,
                Color = Color.Black,
            };
        }

        public static Message CreateMessage(Client client, string message)
        {
            Message m = Create(client);
            m.Type = MessageType.Message;
            m.Content = message;
            m.Color = Color.Blue;
            return m;
        }

        public static Message CreateInfo(Client client)
        {
            Message m = Create(client);
            m.Type = MessageType.Info;
            m.Content = DateTime.Now.ToString("dd/MM/yyyy H:mm") + " - " + client.Name + " (" + client.Info + ")";
            m.Color = Color.Gray;
            return m;
        }

        public static Message CreatePhoto(Client client, string photo)
        {
            Message m = Create(client);
            m.Type = MessageType.Photo;
            m.Content = photo;
            m.Color = Color.Gray;
            return m;
        }

        public static Message CreateSound(Client client, string sound)
        {
            Message m = Create(client);
            m.Type = MessageType.Sound;
            m.Content = sound;
            m.Color = Color.Green;
            return m;
        }

        private MessageType type;

        private long sender;
        private string content;
        private DateTime time;
        private Color color;

        protected Message() { }

        public override string ToString()
        {
            return content;
        }

        public object Value
        {
            get
            {
                switch (Type)
                {
                    case MessageType.Message: 
                    case MessageType.Info:
                    case MessageType.Sound:
                        return Content;

                    case MessageType.Photo:
                        byte[] data = Convert.FromBase64String(Content);
                        using (var ms = new MemoryStream(data))
                        {
                            return Image.FromStream(ms);
                        }
                }
                return null;
            }
        }

        [DataMember]
        public MessageType Type
        {
            get { return type; }
            set { type = value; }
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

    }
}
