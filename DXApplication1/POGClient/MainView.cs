using Common;
using DevExpress.Utils;
using DevExpress.Utils.Drawing.Helpers;
using DevExpress.Utils.MVVM.Services;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Camera;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace POGClient
{
    public partial class MainView : XtraForm
    {
        private long clientId = 0;

        public MainView()
        {
            InitializeComponent();
            Win32SubclasserException.Allow = false;
            if (!mvvmContext.IsDesignMode)
            {
                InitializeLayouts();
                InitializeBindings();
                RegisterServices();
                
            }
        }

        void RegisterServices()
        {
            mvvmContext.RegisterService(MessageBoxService.Create(DefaultMessageBoxServiceType.XtraMessageBox));
        }

        void InitializeLayouts()
        {
            xtraTabControl.ShowTabHeader = DefaultBoolean.False;
            xtraTabControl.SelectedTabPageIndex = 0;
            
            bbiSendNudes.Visibility = BarItemVisibility.Never;
            bbiMakeSound.Visibility = BarItemVisibility.Never;
            beiSlang.Visibility = BarItemVisibility.Never;
            
            gvMessages.OptionsBehavior.Editable = false;
            gvMessages.RowCellStyle += GvMessages_RowCellStyle;
            gvMessages.RowCountChanged += GvMessages_RowCountChanged;
            gvMessages.CustomRowCellEdit += GvMessages_CustomRowCellEdit;
            riPictureEdit.SizeMode = PictureSizeMode.Squeeze;
            
            for (int i = 0; i < icAvatars.Images.Count; i++)
            {
                icbAvatar.Properties.Items.Add(new ImageComboBoxItem("", i, i));
            }
        }
        
        void InitializeBindings()
        {
            var fluent = mvvmContext.OfType<MainViewModel>();

            fluent.WithEvent(this, "Load").EventToCommand(m => m.Start());
            fluent.WithEvent<FormClosingEventArgs>(this, "FormClosing").EventToCommand(
                m => m.Stop());

            fluent.SetObjectDataSourceBinding(bsClient, m => m.Me, m => m.UpdateCommands());
            fluent.SetObjectDataSourceBinding(bsClients, m => m.Clients, m => m.UpdateCommands());
            fluent.SetObjectDataSourceBinding(bsMessages, m => m.Messages);
            fluent.SetObjectDataSourceBinding(bsSlang, m => m.SlangList);
            
            fluent.WithEvent<KeyEventArgs>(meMessageText, "KeyDown").EventToCommand(
                m => m.KeyPressed(null));

            fluent.SetTrigger(m => m.LoggedIn, logged => 
            {
                xtraTabControl.Invoke(new Action(() => 
                {
                    if (logged)
                    {
                        clientId = fluent.ViewModel.Me.Id;
                        xtraTabControl.SelectedTabPageIndex = 1;
                        bbiSendNudes.Visibility = BarItemVisibility.Always;
                        bbiMakeSound.Visibility = BarItemVisibility.Always;
                        beiSlang.Visibility = BarItemVisibility.Always;
                    }
                    else
                    {
                        xtraTabControl.SelectedTabPageIndex = 0;
                        bbiSendNudes.Visibility = BarItemVisibility.Never;
                        beiSlang.Visibility = BarItemVisibility.Never;
                    }
                }));
            });
            fluent.SetTrigger(m => m.Me.Avatar, id => { peMe.Image = icAvatars.Images[id]; });
            fluent.SetTrigger(m => m.Other.Avatar, id => { peOther.Image = icAvatars.Images[id]; });
            fluent.SetTrigger(m => m.DoShake, shake => {
                if (shake)
                {
                    this.BeginInvoke(new Action(() => 
                    {
                        Shake();
                    }));
                    fluent.ViewModel.DoShake = false;
                }
            });

            fluent.SetBinding(this, v => v.Text, m => m.Me.Title);
            fluent.SetBinding(meMessageText, me => me.Text, m => m.MessageText);
            fluent.SetBinding(ceOnline, ce => ce.EditValue, m => m.Other.LoggedIn);
            fluent.SetBinding(lblOtherName, lbl => lbl.Text, m => m.Other.Name);
            fluent.SetBinding(beiSlang, bei => bei.EditValue, m => m.SelectedSlang);
            
            fluent.BindCommand(btnLogIn, m => m.LogIn());
            fluent.BindCommand(btnSendMessage, m => m.SendMessage());
            fluent.BindCommand(bbiLogOut, m => m.LogOut());
            fluent.BindCommand(bbiMakeSound, m => m.MakeSound());
            
        }

        private void GvMessages_RowCountChanged(object sender, EventArgs e)
        {
            gvMessages.MoveLast();
        }

        //void TileView_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e)
        //{
        //    if ((bool)this.tileView.GetRowCellValue(e.RowHandle, "LoggedIn"))
        //    {
        //        e.Item.Elements[2].Image = icLogStates.Images[0];
        //        e.Item.Elements[2].Text = string.Empty;
        //    }
        //    else
        //    {
        //        e.Item.Elements[2].Image = icLogStates.Images[1];
        //        e.Item.Elements[2].Text = string.Empty;
        //    }

        //    int ndx = (int)this.tileView.GetRowCellValue(e.RowHandle, "Avatar");
        //    e.Item.Elements[3].Image = icAvatars.Images[ndx];
        //    e.Item.Elements[3].Text = string.Empty;
        //}

        private void GvMessages_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            Common.Message m = (Common.Message)gvMessages.GetRow(e.RowHandle);
            if (m != null)
            {
                switch (m.Type)
                {
                    case MessageType.Message:
                        e.Appearance.FontStyleDelta = FontStyle.Bold;
                        break;
                    case MessageType.Info:
                        e.Appearance.FontStyleDelta = FontStyle.Regular;
                        break;
                    case MessageType.Photo:
                        e.Appearance.FontStyleDelta = FontStyle.Regular;
                        break;
                    case MessageType.Sound:
                        e.Appearance.FontStyleDelta = FontStyle.Regular;
                        break;
                }

                e.Appearance.ForeColor = m.Color;

                if (m.Sender == clientId)
                {
                    e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
                else
                {
                    e.Appearance.TextOptions.HAlignment = HorzAlignment.Near;
                }
            }
        }

        private void GvMessages_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            Common.Message m = (Common.Message)gvMessages.GetRow(e.RowHandle);
            if (m != null && m.Type == MessageType.Photo)
            {
                e.RepositoryItem = riPictureEdit;
            }
            else
            {
                e.RepositoryItem = riMemoEdit;
            }
        }

        private void Shake()
        {
            try
            {
                var original = this.Location;
                var rnd = new Random(1337);
                const int shake_amplitude = 10;
                for (int i = 0; i < 15; i++)
                {
                    this.Location = new Point(original.X + rnd.Next(-shake_amplitude, shake_amplitude), original.Y + rnd.Next(-shake_amplitude, shake_amplitude));
                    System.Threading.Thread.Sleep(20);
                }
                this.Location = original;
            }
            catch { }
        }

        private void bbiSendNudes_ItemClick(object sender, ItemClickEventArgs e)
        {
             TakePictureDialog dialog = new TakePictureDialog();
            if (dialog.ShowDialog("Fotos enal") == DialogResult.OK)
            {
                Image image = dialog.Image;
                string tmp = Path.GetTempFileName();
                image.Save(tmp);
                
                var fluent = mvvmContext.OfType<MainViewModel>();
                fluent.ViewModel.SendPicture(tmp, ImageToByteArray(dialog.Image));
            }
        }

        public byte[] ImageToByteArray(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

    }
}
