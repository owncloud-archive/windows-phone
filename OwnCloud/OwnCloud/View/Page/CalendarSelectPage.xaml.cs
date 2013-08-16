using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OwnCloud.Resource.Localization;
using OwnCloud.Data;

namespace OwnCloud.View.Page
{
    public partial class CalendarSelectPage : PhoneApplicationPage
    {
        public CalendarSelectPage()
        {
            InitializeComponent();

            this.Unloaded += CalendarSelectPage_Unloaded;
        }

        void CalendarSelectPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_context != null)
                Context.Dispose();
        }

        #region Private Fields

        private int _userId;
        private Data.Account _account;
        private Data.OwnCloudDataContext _context;
        private Data.OwnCloudDataContext Context
        {
            get
            {
                if (_context == null)
                    _context = new OwnCloudDataContext();
                return _context;
            }
        }

        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get userid in query
            if (NavigationContext.QueryString.ContainsKey("uid"))
                _userId = int.Parse(NavigationContext.QueryString["uid"]);
            else throw new ArgumentNullException("uid", AppResources.Exception_NoUserID);

            LoadCalendars();

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Load all availible Calendars for a Event
        /// </summary>
        private void LoadCalendars()
        {
            this.DataContext = new CalendarListDataContext(_userId);
        }


    }
}