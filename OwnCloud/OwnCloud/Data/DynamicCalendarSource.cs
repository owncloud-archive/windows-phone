using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwnCloud.Data
{
    /// <summary>
    /// Provide some Events
    /// </summary>
    public class DynamicCalendarSource
    {
        public IEnumerable<TableEvent> LoadEvents(object sender)
        {
            var result = OnOnEventsRequested(sender);

            return result ?? new List<TableEvent>();
        }

        public event EventsRequested OnEventsRequested;
        protected virtual IEnumerable<TableEvent> OnOnEventsRequested(object sender)
        {
            var result = new LoadEventResult();

            EventsRequested handler = OnEventsRequested;
            if (handler != null) handler(sender,result);

            return result.Result;
        }

        public delegate void EventsRequested(object sender,LoadEventResult e);

        public class LoadEventResult
        {
            public IEnumerable<TableEvent> Result;
        }
    }
}
