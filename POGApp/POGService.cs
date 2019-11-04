using Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace POGApp
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class POGService : IPOGService
    {
        private readonly Client cClient = new Client() { Id = Client.C_ID, Name = "Charlotte", LoggedIn = false };
        private readonly Client wClient = new Client() { Id = Client.W_ID, Name = "Wouter", LoggedIn = false };

        private IPOGCallback cCallback;
        private IPOGCallback wCallback;

        private Message lastMessage;

        private readonly List<Message> messages = new List<Message>();

        private readonly object syncObj = new object();

        public IPOGCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IPOGCallback>();
            }
        }

        public void Register(long id)
        {
            if (id == Client.C_ID)
            {
                cCallback = CurrentCallback;
            }
            else
            {
                wCallback = CurrentCallback;
            }
        }

        public void UnRegister(long id)
        {
            if (id == Client.C_ID)
            {
                cCallback = null;
            }
            else
            {
                wCallback = null;
            }
            Disconnect(id);
        }

        public void Connect(Client client)
        {
            try
            {
                Console.WriteLine("Connect " + client);
                lock (syncObj)
                {
                    client.LoggedIn = true;
                    if (client.Id == Client.C_ID)
                    {
                        cClient.CopyFrom(client);
                    }
                    else
                    {
                        wClient.CopyFrom(client);
                    }
                    wCallback?.RefreshClients(cClient, wClient);
                    cCallback?.RefreshClients(cClient, wClient);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - Connect: " + e);
            }
        }

        public Client GetWouter()
        {
            try
            {
                Console.WriteLine("GetWouter");
                return wClient;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - Disconnect: " + e);
            }
            return null;
        }

        public Client GetCharlotte()
        {
            try
            {
                Console.WriteLine("GetCharlotte");
                return cClient;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - GetCharlotte: " + e);
            }
            return null;
        }

        public List<Message> GetMessages()
        {
            try
            {
                Console.WriteLine("GetMessages");
                lock (syncObj)
                {
                    return new List<Message>(messages);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - GetMessages: " + e);
            }
            return null;
        }

        private void AddMessage(Message msg)
        {
            lock (syncObj)
            {
                messages.Add(msg);
                if (messages.Count > 2000)
                {
                    messages.RemoveAt(0);
                }
            }
        }

        private void SendInfo(Message msg)
        {
            try
            {
                if (lastMessage != null && msg.Sender != lastMessage.Sender)
                {
                    Message info = Message.CreateInfo(msg.Sender == Client.C_ID ? cClient : wClient);

                    AddMessage(info);
                    wCallback?.Receive(info);
                    cCallback?.Receive(info);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - SendInfo: " + e);
            }
        }

        public void Say(Message msg)
        {
            try
            {
                Console.WriteLine("Say " + msg);
                lock (syncObj)
                {
                    SendInfo(msg);
                    AddMessage(msg);
                    wCallback?.Receive(msg);
                    cCallback?.Receive(msg);
                    lastMessage = msg;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - Say: " + e);
            }
        }

        public void Disconnect(long id)
        {
            try
            {
                if (id == Client.C_ID)
                {
                    cClient.LoggedIn = false;
                    Console.WriteLine("Disconnect " + cClient);
                }
                else
                {
                    wClient.LoggedIn = false;
                    Console.WriteLine("Disconnect " + wClient);
                }
                cCallback?.RefreshClients(cClient, wClient);
                wCallback?.RefreshClients(cClient, wClient);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - Disconnect: " + e);
            }
        }
    }
}
