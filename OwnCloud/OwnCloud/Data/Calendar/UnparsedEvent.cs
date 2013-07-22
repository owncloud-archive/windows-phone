using OwnCloud.Data.Calendar.ParsedCalendar;

namespace OwnCloud.Data.Calendar
{
    /// <summary>
    /// Represents a unparsed ICAL calendar
    /// </summary>
    public class UnparsedEvent
    {
        /// <summary>
        /// Raw ics calendar data
        /// </summary>
        public string RawEventData { get; set; }

        /// <summary>
        /// Metainfo for this event
        /// </summary>
        public EventCalDavInfo EventInfo { get; set; }
    }
}
