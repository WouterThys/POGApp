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
    public class Client : BaseClass, IEquatable<Client>
    {
        public const long C_ID = 1;
        public const long W_ID = 2;

        private long id;
        private string name;
        private string info;
        private int avatar;
        private DateTime time;
        private bool loggedIn;
        private Color color;

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Client);
        }

        public bool Equals(Client other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + Id.GetHashCode();
        }

        public static bool operator ==(Client client1, Client client2)
        {
            return EqualityComparer<Client>.Default.Equals(client1, client2);
        }

        public static bool operator !=(Client client1, Client client2)
        {
            return !(client1 == client2);
        }

        public void CopyFrom(Client client)
        {
            if (client == null) return;
            Name = client.Name;
            Info = client.Info;
            Avatar = client.Avatar;
            Time = client.Time;
            LoggedIn = client.LoggedIn;
            Color = client.Color;
        }

        [DataMember]
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public string Name
        {
            get { return name ?? ""; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("Title");
            }
        }

        [DataMember]
        public string Info
        {
            get { return info ?? ""; }
            set { info = value; OnPropertyChanged("Info"); }
        }

        [DataMember]
        public int Avatar
        {
            get { return avatar; }
            set { avatar = value; OnPropertyChanged("Avatar"); }
        }

        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; OnPropertyChanged("Time"); }
        }

        [DataMember]
        public bool LoggedIn
        {
            get { return loggedIn; }
            set { loggedIn = value; OnPropertyChanged("LoggedIn"); }
        }

        [DataMember]
        public Color Color
        {
            get { return color; }
            set { color = value; OnPropertyChanged("Color"); }
        }

        public string Title
        {
            get { return "POG Chat 2000 (" + Name + ")"; }
        }
    }
}
