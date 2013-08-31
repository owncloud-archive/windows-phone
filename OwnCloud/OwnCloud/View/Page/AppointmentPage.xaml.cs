using System;
using System.Linq;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using OwnCloud.Data;
using OwnCloud.Data.Calendar;
using OwnCloud.Data.Calendar.Parsing;
using OwnCloud.Extensions;
using OwnCloud.Net;

namespace OwnCloud.View.Page
{
    public partial class AppointmentPage : PhoneApplicationPage
    {
        public AppointmentPage()
        {
            InitializeComponent();

            this.Unloaded += AppointmentPage_Unloaded;
        }

        void AppointmentPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        private string _url;
        private int _accountId;

        private OwnCloudDataContext _context;
        public OwnCloudDataContext Context
        {
            get { return _context ?? (_context = new OwnCloudDataContext()); }
            set { _context = value; }
        }

        private Account _account;
        public Account Account
        {
            get { return _account ?? (Context.Accounts.Single(o => o.GUID == _accountId)); }
            set { _account = value; }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                return;

            if(NavigationContext.QueryString.ContainsKey("url"))
                LoadFromUrl(NavigationContext.QueryString["url"]);

            //Load Account ID
            if (NavigationContext.QueryString.ContainsKey("uid"))
                _accountId = int.Parse(NavigationContext.QueryString["uid"]);

            base.OnNavigatedTo(e);
        }

        #region Load and Save and delete

        private void LoadFromUrl(string url)
        {
            _url = url;

            var dbEvent = Context.Events.SingleOrDefault(o => o.Url == url);

            if (dbEvent == null) return;

            var parser = new ParserICal();
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(dbEvent.CalendarData);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                var storedEvent = parser.Parse(stream).Events.First();

                TbTitle.Text = storedEvent.Title ?? "";
                DpFrom.Value = storedEvent.From;
                TpFrom.Value = storedEvent.From;
                DpTo.Value = storedEvent.To;
                TpTo.Value = storedEvent.To;
                TbDescription.Text = storedEvent.Description ?? "";
            }
        }

        private void SaveExisting()
        {
            LockPage();

            var dbEvent = Context.Events.SingleOrDefault(o => o.Url == _url);
            if (dbEvent == null) return;

            dbEvent.From = (DpFrom.Value ?? DateTime.Now).CombineWithTime(TpFrom.Value ?? DateTime.Now);
            dbEvent.To = (DpTo.Value ?? DateTime.Now).CombineWithTime(TpTo.Value ?? DateTime.Now);
            dbEvent.Title = TbTitle.Text;

            CalendarDataUpdater.UpdateCalendarData(dbEvent, TbDescription.Text,false);

            var ocCLient = LoadOcCalendarClient();
            ocCLient.SaveEventComplete += ocCLient_SaveEventComplete;
            ocCLient.SaveEvent(dbEvent);

            Context.SubmitChanges();
        }

        private void ocCLient_SaveEventComplete(bool success)
        {
            //Todo: Error Handling
            Dispatcher.BeginInvoke(() =>
            {
                UnlockPage();
                App.Current.RootFrame.Navigate(new Uri("/View/Page/CalendarMonthPage.xaml?uid=" + _accountId.ToString(), UriKind.Relative));
            });
        }

        /// <summary>
        /// Deletes the current existing event
        /// </summary>
        private void DeleteExisting()
        {
            var client = LoadOcCalendarClient();
            client.DeleteEventComplete += ocCLient_DeleteEventComplete;
            client.DeleteEvent(_url);
        }

        void ocCLient_DeleteEventComplete(bool success)
        {
            //Todo: Error Handling
            Dispatcher.BeginInvoke(() =>
            {
                UnlockPage();
                App.Current.RootFrame.Navigate(new Uri("/View/Page/CalendarMonthPage.xaml?uid=" + _accountId.ToString(), UriKind.Relative));
            });
        }


        private OcCalendarClient LoadOcCalendarClient()
        {
            if(Account.IsEncrypted)
                Account.RestoreCredentials();

            return new OcCalendarClient(Account.GetUri().AbsoluteUri,
                                        new Net.OwncloudCredentials
                                            {
                                                Username = Account.Username,
                                                Password = Account.Password
                                            }, Account.CalDAVPath);
        }


        #endregion


        private void LockPage()
        {
            SystemTray.ProgressIndicator.IsVisible = true;
            this.ApplicationBar.IsVisible = false;
        }

        private void UnlockPage()
        {
            Dispatcher.BeginInvoke(() =>
                {
                    SystemTray.ProgressIndicator.IsVisible = false;
                    this.ApplicationBar.IsVisible = true;
                });
        }

        #region Page Events

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (_url != null)
                SaveExisting();
        }

        private void OnDeleteClick(object sender, EventArgs e)
        {
            if(_url != null)
                DeleteExisting();
        }

        #endregion


    }
}