using Common;
using DevExpress.Mvvm.DataAnnotations;
using POGApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;


namespace POGClient
{
    [POCOViewModel()]
    public class MainViewModel : IPOGCallback
    {
        public virtual void Start()
        {

        }

        #region IPOGCallback Interface

        public void RefreshClients(List<Client> clients)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            throw new NotImplementedException();
        }

        public void ReceiveWhisper(Message msg, Client receiver)
        {
            throw new NotImplementedException();
        }

        public void IsWritingCallback(Client client)
        {
            throw new NotImplementedException();
        }

        public void ReceiverFile(FileMessage fileMsg, Client receiver)
        {
            throw new NotImplementedException();
        }

        public void UserJoin(Client client)
        {
            throw new NotImplementedException();
        }

        public void UserLeave(Client client)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }
}
