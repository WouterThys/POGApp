﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class FileMessage
    {
        private long sender;
        private string fileName;
        private byte[] data;
        private DateTime time;

        [DataMember]
        public long Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        [DataMember]
        public string FileName
        {
            get { return fileName ?? ""; }
            set { fileName = value; }
        }

        [DataMember]
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
    }
}
