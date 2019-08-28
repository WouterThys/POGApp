using Common;
using DevExpress.Utils;
using DevExpress.Utils.Drawing.Helpers;
using DevExpress.Utils.MVVM.Services;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace POGClient
{
    public partial class MainView : DevExpress.XtraBars.Ribbon.RibbonForm
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

            gvMessages.OptionsBehavior.Editable = false;

            tileView.ItemCustomize += TileView_ItemCustomize;
            gvMessages.RowCellStyle += GvMessages_RowCellStyle;
            //gvMessages.CustomDrawCell += GridView_CustomDrawCell;


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

            fluent.SetObjectDataSourceBinding(bsClient, m => m.Client, m => m.UpdateCommands());
            fluent.SetObjectDataSourceBinding(bsClients, m => m.Clients, m => m.UpdateCommands());
            fluent.SetObjectDataSourceBinding(bsMessages, m => m.Messages);

            fluent.SetTrigger(m => m.LoggedIn, logged => 
            {
                xtraTabControl.Invoke(new Action(() => 
                {
                    if (logged)
                    {
                        clientId = fluent.ViewModel.Client.Id;
                        xtraTabControl.SelectedTabPageIndex = 1;
                    }
                    else
                    {
                        xtraTabControl.SelectedTabPageIndex = 0;
                    }
                }));
            });

            fluent.SetBinding(meMessageText, me => me.EditValue, m => m.MessageText);

            fluent.BindCommand(btnLogIn, m => m.LogIn());
            fluent.BindCommand(btnSendMessage, m => m.SendMessage());
            fluent.BindCommand(bbiLogOut, m => m.LogOut());
        }



        void TileView_ItemCustomize(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemCustomizeEventArgs e)
        {
            if ((bool)this.tileView.GetRowCellValue(e.RowHandle, "LoggedIn"))
            {
                e.Item.Elements[2].Image = icLogStates.Images[0];
                e.Item.Elements[2].Text = string.Empty;
            }
            else
            {
                e.Item.Elements[2].Image = icLogStates.Images[1];
                e.Item.Elements[2].Text = string.Empty;
            }

            int ndx = (int)this.tileView.GetRowCellValue(e.RowHandle, "Avatar");
            e.Item.Elements[3].Image = icAvatars.Images[ndx];
            e.Item.Elements[3].Text = string.Empty;
        }

        private void GvMessages_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            Common.Message m = (Common.Message)gvMessages.GetRow(e.RowHandle);
            if (m != null)
            {
                if (m.Sender == clientId)
                {
                    e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                    e.Appearance.ForeColor = Color.Blue;
                }
                else
                {
                    e.Appearance.TextOptions.HAlignment = HorzAlignment.Near;
                    e.Appearance.ForeColor = Color.Green;
                }
            }
        }

        //private void GridView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        //{
        //    Common.Message m = (Common.Message)gvMessages.GetRow(e.RowHandle);
        //    if (m != null)
        //    {
        //        if (m.Sender == clientId)
        //        {
        //            e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
        //            e.Appearance.ForeColor = Color.Blue;
        //            //e.Graphics.DrawImage(new Bitmap(@"left.png"), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        //            //Rectangle bounds = e.Bounds;
        //            //bounds.Offset(-30, 0);
        //            //e.Cache.DrawString(e.DisplayText, e.Appearance.Font, Brushes.Black, bounds, e.Appearance.GetStringFormat());
        //        }
        //        else
        //        {
        //            //Rectangle bounds = e.Bounds;
        //            //bounds.Offset(30, 0);
        //            e.Appearance.TextOptions.HAlignment = HorzAlignment.Near;
        //            e.Appearance.ForeColor = Color.Green;
        //            //e.Graphics.DrawImage(new Bitmap(@"right.png"), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        //            //e.Cache.DrawString(e.DisplayText, e.Appearance.Font, Brushes.Black, bounds, e.Appearance.GetStringFormat());
        //        }
        //        e.Handled = true;
        //    }
        //}

    }
}
