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
        private readonly List<Client> clientList = new List<Client>();

        private readonly object syncObj = new object();

        public IPOGCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IPOGCallback>();
            }
        }

        private bool HasClient(string name)
        {
            foreach (Client c in clients.Keys)
            {
                if (c.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Connect(Client client)
        {
            if (!clients.ContainsValue(CurrentCallback) && !HasClient(client.Name))
            {
                lock (syncObj)
                {
                    clients.Add(client, CurrentCallback);
                    clientList.Add(client);

                    foreach (Client key in clients.Keys)
                    {
                        IPOGCallback callback = clients[key];
                        try
                        {
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
            return false;
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
                        if (sender.Name == msg.Sender)
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
                        if (sender.Name == fileMsg.Sender)
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
                if (client.Name == c.Name)
                {
                    lock (syncObj)
                    {
                        clients.Remove(c);
                        clientList.Remove(c);
                        foreach (IPOGCallback callback in clients.Values)
                        {
                            callback.RefreshClients(clientList);
                            callback.UserLeave(client);
                        }
                    }
                    return;
                }
            }
        }
    }
}
