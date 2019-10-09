using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace POGApp
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class POGService : IPOGService
    {

        private readonly Client cClient = new Client() { Id = Client.C_ID, Name = "Charlotte", LoggedIn = false };
        private readonly Client wClient = new Client() { Id = Client.W_ID, Name = "Wouter", LoggedIn = false };

        private IPOGCallback cCallback;
        private IPOGCallback wCallback;

        private readonly List<Message> messages = new List<Message>();

        private readonly object syncObj = new object();

        public IPOGCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IPOGCallback>();
            }
        }

        public void Connect(Client client)
        {
            lock (syncObj)
            {
                client.LoggedIn = true;
                if (client.Id == Client.C_ID)
                {
                    cClient.CopyFrom(client);
                    cCallback = CurrentCallback;
                    wCallback?.UserJoin(cClient);
                }
                else
                {
                    wClient.CopyFrom(client);
                    wCallback = CurrentCallback;
                    cCallback?.UserJoin(cClient);
                }
            }
        }

        public List<Client> GetClients()
        {
            lock (syncObj)
            {
                return new List<Client>() { cClient, wClient };
            }
        }

        public List<Message> GetMessages()
        {
            lock(syncObj)
            {
                return new List<Message>(messages);
            }
        }

        private void AddMessage(Message msg)
        {
            lock(syncObj)
            {
                messages.Add(msg);
                if (messages.Count > 200)
                {
                    messages.RemoveAt(0);
                }
            }
        }

        public void Say(Message msg)
        {
            lock (syncObj)
            {
                AddMessage(msg);
                if (msg.Sender == Client.C_ID)
                {
                    wCallback?.Receive(msg);
                }
                else
                {
                    cCallback?.Receive(msg);
                }
            }
        }
        
        public void IsWriting(long id)
        {
            lock (syncObj)
            {
                if (id == Client.C_ID)
                {
                    wCallback?.IsWritingCallback(id);
                }
                else
                {
                    cCallback?.IsWritingCallback(id);
                }
            }
        }
        
        public void Disconnect(long id)
        {
            if (id == Client.C_ID)
            {
                cClient.LoggedIn = false;
                wCallback?.UserLeave(cClient);
            }
            else
            {
                wClient.LoggedIn = false;
                cCallback?.UserLeave(wClient);
            }
        }
    }
}
