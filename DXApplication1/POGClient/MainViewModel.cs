using Common;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using POGApp;
using POGClient.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;


namespace POGClient
{
    [POCOViewModel()]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MainViewModel : IPOGServiceCallback
    {
        // Variables
        private POGServiceClient serviceClient;

        // MVVM
        public virtual bool Connected { get; protected set; }
        public virtual bool LoggedIn { get; protected set; }
        public virtual DateTime LastCommunication { get; protected set; }
        public virtual BindingList<Client> Clients { get; protected set; } = new BindingList<Client>();
        public virtual BindingList<Message> Messages { get; protected set; }

        public virtual string MessageText { get; set; }

        // Services
        public virtual IMessageBoxService MessageBoxService { get { throw new NotImplementedException(); } }
        public virtual IDispatcherService DispatcherService { get { throw new NotImplementedException(); } }
        
        public virtual Client Client
        {
            get
            {
                return Clients.FirstOrDefault(c => c.Id == ClientSettings.Cs.ClientId);
            }
            protected set { }
        }

        public virtual void Start()
        {
            // Service
            CreateService();

            // Client
            if (Connected)
            {
                Messages = new BindingList<Message>();
                FetchAllClients();
            }
        }

        public virtual void Stop()
        {
            if (serviceClient != null && Client != null)
            {
                Call(c =>
                {
                    Task.Factory.StartNew((dispatcher) =>
                    {
                        c.Disconnect(Client);
                        try
                        {
                            serviceClient.Close();
                        }
                        catch
                        {
                            //
                        }
                    }, DispatcherService);

                });
            }
        }

        public virtual void UpdateCommands()
        {
            this.RaiseCanExecuteChanged(x => x.LogIn());
            this.RaiseCanExecuteChanged(x => x.LogOut());
            this.RaiseCanExecuteChanged(x => x.SendMessage());
        }

        public virtual bool CanLogIn()
        {
            return Connected && !string.IsNullOrEmpty(Client.Name);
        }

        public virtual void LogIn()
        {
            Call(c =>
            {
                Task.Factory.StartNew((dispatcher) =>
                {
                    bool loggedIn = serviceClient.Connect(Client);
                    ClientSettings.Cs.ClientInfo = Client.Info;
                    ClientSettings.Cs.ClientName = Client.Name;
                    ((IDispatcherService)dispatcher).BeginInvoke(() =>
                    {
                        LoggedIn = loggedIn;
                        UpdateCommands();
                    });
                }, DispatcherService);

            });
        }

        public virtual bool CanLogOut()
        {
            return Connected && Client != null && Client.LoggedIn;
        }

        public virtual void LogOut()
        {
            if (serviceClient != null && Client != null)
            {
                Call(c =>
                {
                    Task.Factory.StartNew((dispatcher) =>
                    {
                        c.Disconnect(Client);
                        Client.LoggedIn = false;
                        LoggedIn = false;
                    }, DispatcherService);

                });
            }
        }

        public virtual void OnMessageTextChanged()
        {
            this.RaiseCanExecuteChanged(x => x.SendMessage());
        }

        public virtual bool CanSendMessage()
        {
            return Connected && Client != null && Client.Id > 0 && !string.IsNullOrEmpty(MessageText);
        }

        public virtual void SendMessage()
        {
            Message m = new Message()
            {
                Sender = Client.Id,
                Content = MessageText,
                Time = DateTime.Now
            };

            Call(c => 
            {
                Task.Factory.StartNew(() =>
                {
                    c.Say(m);
                });
            });
        }

        private void FetchAllClients()
        {
            Call(c =>
            {
                Task.Factory.StartNew((dispatcher) =>
                {
                    List<Client> newClients = c.GetClients();
                    ((IDispatcherService)dispatcher).BeginInvoke(() =>
                    {
                        Clients = new BindingList<Client>(newClients);
                        UpdateCommands();
                        this.RaisePropertyChanged(m => m.Client);
                    });
                }, DispatcherService);

            });
        }
        
        #region WebCall Helpers

        private void ShowWebCallError(string error, Exception e)
        {
            Connected = false;
            if (e != null && ClientSettings.Cs.ShowExceptions)
            {
                error += "\n" + e;
            }

            MessageBoxService.ShowMessage(
                error,
                "Doeme toch",
                MessageButton.OK,
                MessageIcon.Error);
        }

        private void CreateService()
        {
            // Service
            InstanceContext context = new InstanceContext(this);
            serviceClient = new POGServiceClient(context);
            serviceClient.Endpoint.Address = new EndpointAddress(ClientSettings.Cs.Address);
            try
            {
                serviceClient.Open();
                Connected = true;
            }
            catch (Exception e)
            {
                ShowWebCallError("Geen verbinding :(\n vraag aan andere POG wat je moet doen..", e);
            }
        }

        private void Call(Action<POGServiceClient> webCall)
        {
            Call((s) => { webCall(s); return 0; });
        }

        private T Call<T>(Func<POGServiceClient, T> webCall)
        {
            LastCommunication = DateTime.Now;
            if (serviceClient.InnerChannel.State >= CommunicationState.Closing)
            {
                try
                {
                    CreateService();
                }
                catch (Exception e)
                {
                    ShowWebCallError("Het loopt hier allemaal mis", e);
                }
            }
            try
            {
                return webCall(serviceClient);
            }
            catch (Exception e)
            {
                ShowWebCallError("Het loopt hier allemaal mis", e);
            }
            return default(T);
        }
        #endregion

        #region IPOGCallback Interface

        public void RefreshClients(List<Client> clients)
        {
            Task.Factory.StartNew((dispatcher) =>
            {
                // Do checks?
                List<Client> newClients = new List<Client>();
                if (clients != null)
                {
                    newClients.AddRange(clients);
                }
                ((IDispatcherService)dispatcher).BeginInvoke(() =>
                {
                    Clients = new BindingList<Client>(newClients);
                });
            }, DispatcherService);
        }

        public void Receive(Message msg)
        {
            Messages.Add(msg);
            if (msg.Sender == Client.Id)
            {
                MessageText = "";
                UpdateCommands();
            }
        }

        public void ReceiveWhisper(Message msg, Client receiver)
        {

        }

        public void IsWritingCallback(Client client)
        {

        }

        public void ReceiverFile(FileMessage fileMsg, Client receiver)
        {

        }

        public void UserJoin(Client client)
        {
            if (Clients != null && client != null)
            {
                foreach (Client c in Clients)
                {
                    if (c.Id == client.Id)
                    {
                        c.LoggedIn = true;
                    }
                }
            }
        }

        public void UserLeave(Client client)
        {
            if (Clients != null && client != null)
            {
                foreach (Client c in Clients)
                {
                    if (c.Id == client.Id)
                    {
                        c.LoggedIn = false;
                    }
                }
            }
        }

        #endregion
    }
}
