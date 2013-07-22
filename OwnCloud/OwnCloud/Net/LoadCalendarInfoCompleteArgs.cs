using OwnCloud.Data.Calendar.ParsedCalendar;

namespace OwnCloud.Net
{
    class LoadCalendarInfoCompleteArgs
    {
        public bool Success;
        public CalendarCalDavInfo[] CalendarInfo { get; set; }
    }
}
