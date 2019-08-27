using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POGClient
{
    public class ClientSettings : INotifyPropertyChanged
    {
        private static readonly ClientSettings INSTANCE = new ClientSettings();
        public static ClientSettings Cs { get { return INSTANCE; } }
        private ClientSettings()
        {
            iniFile = new IniFile(@"Config\config.ini");
            Initialize();
        }

        // Base properties
        public event PropertyChangedEventHandler PropertyChanged;

        private IniFile iniFile;

        private string url;

        private long clientId;
        private string clientName;
        private string clientInfo;

        private string viewSkin;
        private bool showExceptions;

        private void Initialize()
        {
            url = iniFile.ReadString("Service", "address");

            clientId = iniFile.ReadLong("Client", "clientId");
            clientName = iniFile.ReadString("Client", "clientName");
            clientInfo = iniFile.ReadString("Client", "clientInfo");

            viewSkin = iniFile.ReadString("View", "skin");
            showExceptions = iniFile.ReadBool("View", "showExceptions");
        }

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }


        public string Address
        {
            get { return url; }
        }

        public long ClientId
        {
            get { return clientId; }
        }

        public string ClientName
        {
            get { return clientName ?? ""; }
            set
            {
                clientName = value;
                iniFile.WriteString("Client", "clientName", value);
                OnPropertyChanged("ClientName");
            }
        }

        public string ClientInfo
        {
            get { return clientInfo ?? ""; }
            set
            {
                clientInfo = value;
                iniFile.WriteString("Client", "clientInfo", value);
                OnPropertyChanged("ClientInfo");
            }
        }

        public string ViewSkin
        {
            get { return viewSkin ?? ""; }
            set
            {
                viewSkin = value;
                iniFile.WriteString("Client", "viewSkin", value);
                OnPropertyChanged("ViewSkin");
            }
        }

        public bool ShowExceptions
        {
            get { return showExceptions; }
        }
    }
}
