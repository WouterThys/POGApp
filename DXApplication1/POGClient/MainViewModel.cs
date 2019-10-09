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
        public virtual Client Client { get; protected set; }
        public virtual BindingList<Client> Clients { get; protected set; } = new BindingList<Client>();
        public virtual BindingList<Message> Messages { get; protected set; }

        public virtual string MessageText { get; set; }

        // Services
        public virtual IMessageBoxService MessageBoxService { get { throw new NotImplementedException(); } }
        public virtual IDispatcherService DispatcherService { get { throw new NotImplementedException(); } }


        private void UpdateClients(IEnumerable<Client> newClients)
        {
            foreach (Client newC in newClients)
            {
                Client oldC = Clients.FirstOrDefault(c => c.Id == newC.Id);
                if (oldC != null)
                {
                    oldC.CopyFrom(newC);
                }
                else
                {
                    Clients.Add(newC);
                }
            }
            if (Client == null)
            {
                Client = Clients.FirstOrDefault(c => c.Id == ClientSettings.Cs.ClientId);
            }
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
                FetchAllMessages();
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
                        c.Disconnect(Client.Id);
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
            return Connected && Client != null && !string.IsNullOrEmpty(Client.Name);
        }

        public virtual void LogIn()
        {
            Call(c =>
            {
                Task.Factory.StartNew((dispatcher) =>
                {
                    serviceClient.Connect(Client);
                    ((IDispatcherService)dispatcher).BeginInvoke(() =>
                    {
                        LoggedIn = true;
                        Client.LoggedIn = true;
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
                        c.Disconnect(Client.Id);
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

        public virtual void KeyPressed(System.Windows.Forms.KeyEventArgs keyEvent)
        {
            if (keyEvent == null) return;
            if (keyEvent.Control) return;

            if (keyEvent.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                if (CanSendMessage())
                {
                    SendMessage();
                    MessageText = "";
                    keyEvent.Handled = true;
                    keyEvent.SuppressKeyPress = true;
                }
            }
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
                
                Messages.Add(m);
                UpdateCommands();
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
                        UpdateClients(newClients);
                        UpdateCommands();
                    });
                }, DispatcherService);

            });
        }

        private void FetchAllMessages()
        {
            Call(c =>
            {
                Task.Factory.StartNew((dispatcher) =>
                {
                    List<Message> messages = c.GetMessages();
                    ((IDispatcherService)dispatcher).BeginInvoke(() =>
                    {
                        Messages = new BindingList<Message>(messages);
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
                    UpdateClients(newClients);
                });
            }, DispatcherService);
        }

        public void Receive(Message msg)
        {
            msg.Color = Client.Color;
            Messages.Add(msg);
        }

        public void IsWritingCallback(long client)
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
