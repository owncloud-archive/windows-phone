namespace OwnCloud.Data.Calendar.ParsedCalendar
{
    /// <summary>
    /// Metainformation for a single caldav event
    /// </summary>
    public class EventCalDavInfo
    {
        /// <summary>
        /// Der ETag eines Events wird bei jeder Änderung von CalDav Server geändert
        /// </summary>
        public string GetETag { get; set; }
        /// <summary>
        /// Ruft die relative Server Url ab, oder legt diese fest.
        /// </summary>
        public string Url { get; set; }
    }
}
