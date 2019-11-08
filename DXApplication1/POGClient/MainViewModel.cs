﻿using Common;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using NAudio.Wave;
using POGClient.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace POGClient
{
    [POCOViewModel()]
    [CallbackBehavior]
    public class MainViewModel : IPOGServiceCallback
    {
        // Variables
        private POGServiceClient serviceClient;

        private readonly Client cClient = new Client() { Id = Client.C_ID, Name = "Charlotte", LoggedIn = false };
        private readonly Client wClient = new Client() { Id = Client.W_ID, Name = "Wouter", LoggedIn = false };
        private readonly BindingList<Client> clients;

        // MVVM
        public virtual bool Connected { get; protected set; }
        public virtual bool LoggedIn { get; protected set; }
        public virtual DateTime LastCommunication { get; protected set; }
        public virtual BindingList<Message> Messages { get; protected set; }

        public virtual string MessageText { get; set; }

        // Services
        public virtual IMessageBoxService MessageBoxService { get { throw new NotImplementedException(); } }
        public virtual IDispatcherService DispatcherService { get { throw new NotImplementedException(); } }

        // Sounds
        public virtual BindingList<string> Sounds { get; protected set; }
        public virtual bool DoShake { get; set; }
        private WaveOutEvent soundDevice;
        private AudioFileReader soundFile;
        
        private WaveOutEvent notifyDevice;
        private AudioFileReader notifyFile;

        // Slang
        public virtual BindingList<Slang> SlangList { get; protected set; }
        public virtual Slang SelectedSlang { get; set; }

        // Ctor
        public MainViewModel()
        {
            clients = new BindingList<Client>() { cClient, wClient };
        }

        // General properties
        public virtual Client Me
        {
            get
            {
                if (ClientSettings.Cs.ClientId == Client.C_ID) return cClient;
                else return wClient;
            }
        }

        public virtual Client Other
        {
            get
            {
                if (ClientSettings.Cs.ClientId == Client.C_ID) return wClient;
                else return cClient;
            }
        }

        public virtual BindingList<Client> Clients
        {
            get
            {
                return clients;
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
                FetchClients();
                FetchAllMessages();
                LoadAllSounds();
                LoadAllSlang();

                Notification(@"Sounds/dialup.mp3");
            }
        }

        public virtual void Stop()
        {
            if (serviceClient != null && Me != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Call(c =>
                    {
                        c.UnRegister(Me.Id);
                    });
                });
            }
        }

        public virtual void UpdateCommands()
        {
            this.RaiseCanExecuteChanged(x => x.LogIn());
            this.RaiseCanExecuteChanged(x => x.LogOut());
            this.RaiseCanExecuteChanged(x => x.SendMessage());
            this.RaiseCanExecuteChanged(x => x.MakeSound());
        }

        public virtual bool CanLogIn()
        {
            return Connected && Me != null && !string.IsNullOrEmpty(Me.Name);
        }

        public virtual void LogIn()
        {
            Task.Factory.StartNew(() =>
            {
                Call(c =>
                {
                    c.Connect(Me);
                });
            });
        }

        public virtual bool CanLogOut()
        {
            return Connected && Me != null && Me.LoggedIn;
        }

        public virtual void LogOut()
        {
            if (serviceClient != null && Me != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Call(c =>
                    {
                        c.Disconnect(Me.Id);
                    });
                });
            }
        }

        public virtual void OnMessageTextChanged()
        {
            this.RaiseCanExecuteChanged(x => x.SendMessage());
        }

        public virtual void OnSelectedSlangChanged()
        {
            if (SelectedSlang != null)
            {
                if (string.IsNullOrEmpty(MessageText))
                {
                    MessageText = SelectedSlang.Code + " ";
                }
                else
                {
                    MessageText += " " + SelectedSlang.Code;
                }
                SelectedSlang = null;
            }
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

                    keyEvent.Handled = true;
                    keyEvent.SuppressKeyPress = true;
                }
            }
        }

        public virtual bool CanMakeSound()
        {
            return Connected && Sounds != null && Sounds.Count > 0;
        }

        public virtual void MakeSound()
        {
            Random random = new Random();
            int r = random.Next(Sounds.Count);
            string sound = Sounds[r];
            SendSound(sound);
        }

        public virtual bool CanSendMessage()
        {
            return Connected && Me != null && Me.Id > 0 && !string.IsNullOrEmpty(MessageText);
        }

        public virtual void SendMessage()
        {
            Message m = Message.CreateMessage(Me, MessageText);
            MessageText = "";
            Send(m);
        }

        public virtual void SendPicture(string file, byte[] picture)
        {
            Message m = Message.CreatePhoto(Me, Convert.ToBase64String(picture));
            Send(m);
        }

        public virtual void SendSound(string sound)
        {
            Message m = Message.CreateSound(Me, sound);
            Send(m);
        }

        private void Send(Message message)
        {
            if (message != null)
            {
                Task.Factory.StartNew(() =>
                {
                    Call(c =>
                    {
                        c.Say(message);
                    });
                });
            }
        }

        private void FetchClients()
        {
            Task.Factory.StartNew((dispatcher) =>
            {
                Call(c =>
                {
                    Client newW = c.GetWouter();
                    Client newC = c.GetCharlotte();
                    ((IDispatcherService)dispatcher).BeginInvoke(() =>
                    {
                        cClient.CopyFrom(newC);
                        wClient.CopyFrom(newW);
                        UpdateCommands();
                    });
                });
            }, DispatcherService);
        }

        private void FetchAllMessages()
        {
            Task.Factory.StartNew((dispatcher) =>
            {
                Call(c =>
                {
                    List<Message> messages = c.GetMessages();
                    ((IDispatcherService)dispatcher).BeginInvoke(() =>
                    {
                        Messages = new BindingList<Message>(messages);
                    });
                });
            }, DispatcherService);
        }

        private void LoadAllSounds()
        {
            Task.Factory.StartNew((dispatcher) =>
            {
                List<string> soundFiles = new List<string>();
                foreach (string file in Directory.GetFiles("Sounds/"))
                {
                    soundFiles.Add(file);
                }
                ((IDispatcherService)dispatcher).BeginInvoke(() =>
                {
                    Sounds = new BindingList<string>(soundFiles);
                });
            }, DispatcherService);
        }

        private void LoadAllSlang()
        {
            Task.Factory.StartNew((dispatcher) =>
            {
                List<Slang> slang = new List<Slang>();
                if (File.Exists("Config/slang.txt"))
                {
                    try
                    {
                        foreach (string line in File.ReadAllLines("Config/slang.txt"))
                        {
                            var split = line.Split(':');
                            if (split.Length == 2)
                            {
                                slang.Add(new Slang()
                                {
                                    Code = split[0].Trim(),
                                    Description = split[1].Trim()
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Faile to LoadAllSlang: " + e);
                    }
                }

                ((IDispatcherService)dispatcher).BeginInvoke(() =>
                {
                    SlangList = new BindingList<Slang>(slang);
                });
            }, DispatcherService);
        }

        private void PlaySound(string sound)
        {
            if (!string.IsNullOrEmpty(sound))
            {
                try
                {
                    if (soundDevice == null)
                    {
                        soundDevice = new WaveOutEvent();
                        soundDevice.PlaybackStopped += (s, e) =>
                        {
                            soundDevice.Dispose();
                            soundDevice = null;
                            soundFile.Dispose();
                            soundFile = null;
                        };
                    }
                    if (soundFile == null)
                    {
                        soundFile = new AudioFileReader(sound);
                        soundDevice.Init(soundFile);
                    }
                    soundDevice.Play();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Play sound failed: " + e);
                }
            }

        }

        private void Notification(string sound)
        {
            if (!string.IsNullOrEmpty(sound))
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (notifyDevice == null)
                        {
                            notifyDevice = new WaveOutEvent();
                            notifyDevice.PlaybackStopped += (s, e) =>
                            {
                                notifyDevice.Dispose();
                                notifyDevice = null;
                                notifyFile.Dispose();
                                notifyFile = null;
                            };
                        }
                        if (notifyFile == null)
                        {
                            notifyFile = new AudioFileReader(sound);
                            notifyDevice.Init(notifyFile);
                        }
                        notifyDevice.Play();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Play sound failed: " + e);
                    }
                });
            }
        }
        
        #region WebCall Helpers

        private void ShowWebCallError(string error, Exception e)
        {
            Connected = false;

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
                serviceClient.Register(Me.Id);
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

        public void RefreshClients(Client cClient, Client wClient)
        {
            DispatcherService?.BeginInvoke(() =>
            {
                this.cClient.CopyFrom(cClient);
                this.wClient.CopyFrom(wClient);
                LoggedIn = Me.LoggedIn;
                if (LoggedIn && notifyDevice != null)
                {
                    notifyDevice.Stop();
                }
                UpdateCommands();
            });
        }

        public void Receive(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.Message:
                case MessageType.Info:
                    Messages.Add(msg);
                    if (msg.Sender != Me.Id)
                    {
                        Notification(@"Sounds/alert.wav");
                    }
                    break;

                case MessageType.Photo:
                    Task.Factory.StartNew((disp) =>
                    {
                        byte[] data = Convert.FromBase64String(msg.Content);
                        string name = Guid.NewGuid() + ".png";
                        string file = Path.Combine(Path.GetTempPath(), name);
                        File.WriteAllBytes(file, data);
                        ((IDispatcherService)disp).BeginInvoke(() => { Messages.Add(msg); });
                    }, DispatcherService);
                    break;
                case MessageType.Sound:
                    Messages.Add(msg);
                    Task.Factory.StartNew(() =>
                    {
                        PlaySound(msg.Content);
                        DoShake = true;
                    });
                    break;
            }

        }


        #endregion
    }
}
