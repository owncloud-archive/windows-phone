using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OwnCloud.Data
{
    /// <summary>
    /// A Groupt, that can contains all calendars dedicated to a oc account 
    /// </summary>
    public class CalendarAccountGroup : ObservableCollection<TableCalendar>, IGrouping<string,TableCalendar>
    {

        public string Key
        {
            get; set;
        }
    }
}
