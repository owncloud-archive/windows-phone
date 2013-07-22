namespace OwnCloud.Data.Calendar.ParsedCalendar
{
    /// <summary>
    /// Represents metainfo for a calDav calendar
    /// </summary>
    public class CalendarCalDavInfo
    {
        /// <summary>
        /// The cTag is changed on every Calendar change by the server
        /// </summary>
        public string GetCTag { get; set; }
        /// <summary>
        /// Display name of the calendar
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Get the relative server url to this calendar
        /// </summary>
        public string Url { get; set; }
    }
}
