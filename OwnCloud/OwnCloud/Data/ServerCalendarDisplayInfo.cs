using OwnCloud.Data.Calendar.ParsedCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwnCloud.Data
{
    /// <summary>
    /// A class to display a calendar, that exists on the server with the Information, if the Calendar is enabled on the client
    /// </summary>
    class ServerCalendarDisplayInfo
    {
        private bool _isClientEnabled = false;
        /// <summary>
        /// Indicates, if the calendar is enabled on the client
        /// </summary>
        public bool IsClientEnabled
        {
            get { return _isClientEnabled; }
            set { _isClientEnabled = value; }
        }

        private CalendarCalDavInfo _calendarInfo;
        public CalendarCalDavInfo CalendarInfo
        {
            get { return _calendarInfo; }
            set { _calendarInfo = value; }
        }


    }
}
