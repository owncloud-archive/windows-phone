using System.Globalization;
using OwnCloud.Data.Calendar;

namespace OwnCloud.Net
{
    class LoadCalendarDataCompleteArgs
    {
        public UnparsedEvent[] Events;
        public bool Success = true;
    }
}
