using System.IO;
using System.Linq;
using OwnCloud.Data.Calendar.Parsing;
using OwnCloud.Model;

namespace OwnCloud.Data.Calendar
{
    static class EventMetaUpdater
    {
        private static readonly ParserICal CalPaser = new ParserICal();

        /// <summary>
        /// Aktualisiert die Metadaten eines in der Datenbank gespeicherten Events
        /// </summary>
        public static void UpdateEventMetadata(TableEvent eEvent)
        {

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(eEvent.CalendarData);
                writer.Flush();

                stream.Seek(0, SeekOrigin.Begin);

                var parsedCalendar = CalPaser.Parse(stream);

                var parsedEvent = parsedCalendar.Events.FirstOrDefault();

                if (parsedEvent == null) return;

                eEvent.From = parsedEvent.From;
                eEvent.To = parsedEvent.To;
                eEvent.Title = parsedEvent.Title;
                eEvent.IsFullDayEvent = parsedEvent.IsFullDayEvent;

            }

        }

    }
}
