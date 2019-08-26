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
        private readonly Dictionary<Client, IPOGCallback> clients = new Dictionary<Client, IPOGCallback>();

        private readonly object syncObj = new object();

        public IPOGCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IPOGCallback>();
            }
        }

        public bool Connect(Client client)
        {
            lock (syncObj)
            {
                client.LoggedIn = true;
                Client knownClient = clients.Keys.FirstOrDefault(c => c.Id == client.Id);
                if (knownClient != null)
                {
                    knownClient.Name = client.Name;
                    knownClient.Info = client.Info;
                    knownClient.Avatar = client.Avatar;
                    knownClient.Time = DateTime.Now;
                    knownClient.LoggedIn = client.LoggedIn;
                }
                clients[client] = CurrentCallback;
                
                foreach (Client key in clients.Keys)
                {
                    IPOGCallback callback = clients[key];
                    try
                    {
                        List<Client> clientList = new List<Client>(clients.Keys);
                        callback.RefreshClients(clientList);
                        callback.UserJoin(client);
                    }
                    catch
                    {
                        clients.Remove(key);
                        return false;
                    }
                }
            }
            return true;
        }

        public List<Client> GetClients()
        {
            lock (syncObj)
            {
                return new List<Client>(clients.Keys);
            }
        }

        public void Say(Message msg)
        {
            lock (syncObj)
            {
                foreach (IPOGCallback callback in clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void Whisper(Message msg, Client receiver)
        {
            foreach (Client rec in clients.Keys)
            {
                if (rec.Name == receiver.Name)
                {
                    IPOGCallback callback = clients[rec];
                    callback.ReceiveWhisper(msg, rec);

                    foreach (Client sender in clients.Keys)
                    {
                        if (sender.Id == msg.Sender)
                        {
                            IPOGCallback senderCallback = clients[sender];
                            senderCallback.ReceiveWhisper(msg, rec);
                            return;
                        }
                    }
                }
            }
        }

        public void IsWriting(Client client)
        {
            lock (syncObj)
            {
                foreach (IPOGCallback callback in clients.Values)
                {
                    callback.IsWritingCallback(client);
                }
            }
        }

        public bool SendFile(FileMessage fileMsg, Client receiver)
        {
            foreach (Client rcvr in clients.Keys)
            {
                if (rcvr.Name == receiver.Name)
                {
                    Message msg = new Message
                    {
                        Sender = fileMsg.Sender,
                        Content = "I'M SENDING FILE.. " + fileMsg.FileName
                    };

                    IPOGCallback rcvrCallback = clients[rcvr];
                    rcvrCallback.ReceiveWhisper(msg, receiver);
                    rcvrCallback.ReceiverFile(fileMsg, receiver);

                    foreach (Client sender in clients.Keys)
                    {
                        if (sender.Id == fileMsg.Sender)
                        {
                            IPOGCallback sndrCallback = clients[sender];
                            sndrCallback.ReceiveWhisper(msg, receiver);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Disconnect(Client client)
        {
            foreach (Client c in clients.Keys)
            {
                if (client.Id == c.Id)
                {
                    lock (syncObj)
                    {
                        c.LoggedIn = false;
                        foreach (Client c2 in clients.Keys)
                        {
                            if (client.Id != c2.Id)
                            {
                                IPOGCallback callback = clients[c2];
                                List<Client> clientList = new List<Client>(clients.Keys);
                                callback.RefreshClients(clientList);
                                callback.UserLeave(client);
                            }
                        }
                    }
                }
            }
        }
    }
}
