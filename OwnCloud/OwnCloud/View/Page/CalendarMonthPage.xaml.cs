using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using OwnCloud.Data;
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
        }

        #region private fields

        private int _userId = 0;

        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get userid in query
            if (NavigationContext.QueryString.ContainsKey("uid"))
                _userId = int.Parse(NavigationContext.QueryString["uid"]);
            else throw new ArgumentNullException("uid",AppResources.Exception_NoUserID);

            CcCalendar.AccountID = _userId;
            CcCalendar.SelectedDate = DateTime.Now;
            
            base.OnNavigatedTo(e);
        }



        #region Private events

        private void GotoCalendarSettings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Page/CalendarSelectPage.xaml?uid=" + _userId.ToString(), UriKind.Relative));
        }

        #endregion

    }
}