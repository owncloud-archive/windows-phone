using System.Collections.Generic;

namespace OwnCloud.Data.Calendar.ParsedCalendar
{
    /// <summary>
    /// Represents a parsed ical calendar
    /// </summary>
    class CalendarICal
    {
        private List<VEvent> _events = new List<VEvent>();
        /// <summary>
        /// List of all Events in this calendar
        /// </summary>
        public List<VEvent> Events
        {
            get { return _events; }
            set { _events = value; }
        }

    }
}
