namespace OwnCloud.Data.Calendar.ParsedCalendar
{
    /// <summary>
    /// A CalDav Calendar
    /// </summary>
    class CalendarCalDav
    {
        /// <summary>
        /// Relative server URL to this calendar
        /// </summary>
        public string Url
        {
            get { return _info.Url; }
        }
        
        private CalendarCalDavInfo _info = new CalendarCalDavInfo();
        public CalendarCalDavInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }


    }
}
