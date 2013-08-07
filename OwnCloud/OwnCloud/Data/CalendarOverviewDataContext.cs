using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OwnCloud.Net;

namespace OwnCloud.Data
{
    public class CalendarOverviewDataContext 
    {

        public CalendarOverviewDataContext()
        {
            //First, all local stored calendars should be loaded. Then we can load the calendars,
            //that exists online
            CalendarGroups = new ObservableCollection<CalendarAccountGroup>();

            var context = new OwnCloudDataContext();

            foreach (var account in context.Accounts)
            {
                var currentAccountGroup = new CalendarAccountGroup { Key = account.DisplayUserName };

                //Copy to a local var. That fix an issue, that occures with some
                //different versions of comilers
                var currentAccount = account;

                foreach (var calendar in context.Calendars.Where(o => o.AcountID == currentAccount.GUID))
                {
                    currentAccountGroup.Add(calendar);
                }
            }

        }

        /// <summary>
        /// Contains all account groups for the calendars
        /// </summary>
        public ObservableCollection<CalendarAccountGroup> CalendarGroups
        {
            get;
            private set;
        }

    }
}
