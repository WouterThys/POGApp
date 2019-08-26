using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Client
    {
        private string name;
        private string info;
        private int avatar;
        private DateTime time;

        [DataMember]
        public string Name
        {
            get { return name ?? ""; }
            set { name = value; }
        }

        [DataMember]
        public string Info
        {
            get { return info ?? ""; }
            set { info = value; }
        }

        [DataMember]
        public int Avatar
        {
            get { return avatar; }
            set { avatar = value; }
        }

        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
    }
}
