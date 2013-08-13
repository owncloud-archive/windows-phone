using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using OwnCloud.Data;
using OwnCloud.Resource.Localization;
using System.Linq;

namespace OwnCloud.View.Page
{
    public partial class CalendarMonthPage : PhoneApplicationPage
    {
        

        public CalendarMonthPage()
        {
            InitializeComponent();
        }

        #region private fields

        private int _usedId = 0;

        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get userid in query
            if (NavigationContext.QueryString.ContainsKey("uid"))
                _usedId = int.Parse(NavigationContext.QueryString["uid"]);
            else throw new ArgumentNullException("uid",AppResources.Exception_NoUserID);

            CcCalendar.AccountID = _usedId;
            CcCalendar.SelectedDate = DateTime.Now;

            base.OnNavigatedTo(e);
        }

    }
}