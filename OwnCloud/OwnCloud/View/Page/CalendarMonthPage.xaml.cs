using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OwnCloud.Data;
using OwnCloud.Net;
using OwnCloud.Resource.Localization;
using System.Linq;
using OwnCloud.Extensions;

namespace OwnCloud.View.Page
{
    public partial class CalendarMonthPage : PhoneApplicationPage
    {
        

        public CalendarMonthPage()
        {
            InitializeComponent();

            // Translate unsupported XAML bindings
            ApplicationBar.TranslateButtons();

            this.Unloaded += CalendarMonthPage_Unloaded;
        }

        void CalendarMonthPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        #region private fields

        private int _userId = 0;

        private OwnCloudDataContext _context;
        public OwnCloudDataContext Context
        {
            get { return _context ?? (_context = new OwnCloudDataContext()); }
            set { _context = value; }
        }

        private Account _account;
        public Account Account
        {
            get
            {
                if (_account == null)
                {
                    _account = Context.Accounts.Single(o => o.GUID == _userId);
                    if(_account.IsEncrypted)
                        _account.RestoreCredentials();
                }
                return _account;
            }
            set { _account = value; }
        }


        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get userid in query
            if (NavigationContext.QueryString.ContainsKey("uid"))
                _userId = int.Parse(NavigationContext.QueryString["uid"]);
            else throw new ArgumentNullException("uid",AppResources.Exception_NoUserID);

            CcCalendar.AccountID = _userId;
            CcCalendar.SelectedDate = DateTime.Now;
            
            ReloadAppointments();

            base.OnNavigatedTo(e);
        }

        private void ReloadAppointments()
        {
            LockPage();
            SetLoading();
            var sync = new CalendarSync();
            sync.SyncComplete += sync_SyncComplete;
            sync.Sync(Account.GetUri().AbsoluteUri, new Net.OwncloudCredentials { Username = Account.Username, Password = Account.Password }, Account.CalDAVPath);
        }

        void sync_SyncComplete(bool success)
        {
            Dispatcher.BeginInvoke(() => { CcCalendar.RefreshAppointments(); UnlockPage(); UnsetLoading(); });
        }



        #region Private events

        private void GotoCalendarSettings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Page/CalendarSelectPage.xaml?uid=" + _userId.ToString(), UriKind.Relative));
        }

        private void CcCalendar_OnDateChanged(object sender, RoutedEventArgs e)
        {
            tbMonthHeader.Text = CcCalendar.SelectedDate.ToString("MMMM");
        }

        private void ReloadCalendarEvents(object sender, EventArgs e)
        {
            ReloadAppointments();
        }

        #endregion


        private void LockPage()
        {
            foreach (var button in ApplicationBar.Buttons.OfType<ApplicationBarIconButton>())
            {
                button.IsEnabled = false;
            }
            IsEnabled = false;
        }
        private void UnlockPage()
        {
            foreach (var button in ApplicationBar.Buttons.OfType<ApplicationBarIconButton>())
            {
                button.IsEnabled = true;
            }
            IsEnabled = true;
        }
        private void SetLoading()
        {
            if (SystemTray.ProgressIndicator != null)
                SystemTray.ProgressIndicator.IsVisible = true;
        }
        private void UnsetLoading()
        {
            if (SystemTray.ProgressIndicator != null)
                SystemTray.ProgressIndicator.IsVisible = false;
        }
        
    }
}