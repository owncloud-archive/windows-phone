using System;
using System.IO;
using System.Linq;
using OwnCloud.Data.Calendar.Parsing;

namespace OwnCloud.Data.Calendar
{
    static class CalendarDataUpdater
    {
        /// <summary>
        /// Refresh the ICAL Event data of a event
        /// </summary>
        /// <param name="dbEvent">The event</param>
        /// <param name="description">The New desciption of the event</param>
        /// <param name="updateUid">True, when a new UID sould be set. Only recommenten, with a new (unsaved) event</param>
        public static void UpdateCalendarData(TableEvent dbEvent, string description,bool updateUid)
        {
            TokenNode calendarNode = calendarNode = ReadTokenNode(dbEvent);

            var icalEvent = calendarNode.Childs.Single().Childs.First(o => o.Name == "VEVENT");

            UpdateStringToken(icalEvent, "SUMMARY", dbEvent.Title);
            UpdateStringToken(icalEvent, "DESCRIPTION", description);
            UpdateDateToken(icalEvent, "DTSTART", dbEvent.From, dbEvent.IsFullDayEvent);
            UpdateDateToken(icalEvent, "DTEND", dbEvent.To, dbEvent.IsFullDayEvent);

            //The uid helps the CalDav Server to uniquie identify the event and is required for events.
            if(updateUid)
                UpdateStringToken(icalEvent,"UID",Guid.NewGuid().ToString());

            using (var tmpStream = new MemoryStream())
            {
                TokenWriter.WriteTokenNode(calendarNode.Childs.Single(), tmpStream);

                tmpStream.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(tmpStream);
                dbEvent.CalendarData = reader.ReadToEnd();
            }
        }

        public static void UpdateDateToken(TokenNode icalEvent, string tokenName, DateTime value, bool isFullDayEvent)
        {
            if (isFullDayEvent)
                UpdateStringToken(icalEvent, tokenName, value.ToString("yyyyMMdd"), "VALUE=DATE-TIME");
            else
                UpdateStringToken(icalEvent, tokenName, value.ToString("yyyyMMdd") + "T"
                    + value.ToString("HHmmss")
                    , "VALUE=DATE-TIME");
        }

        public static void UpdateStringToken(TokenNode icalEvent, string tokenName, string value,string subKey = "")
        {
            var tquery = icalEvent.Tokens.SingleOrDefault(o => o.NamingKey.ToLower() == tokenName.ToLower()) ?? new Token { Key = tokenName };

            tquery.SubKey = subKey;
            tquery.Value = new EncodedTokenValue {DecodedValue = value};

            if (!icalEvent.Tokens.Contains(tquery))
                icalEvent.Tokens.Add(tquery);
        }



        private static TokenNode ReadTokenNode(TableEvent dbEvent)
        {
            var nodeParser = new ParserNodeToken();
            TokenNode calendarNode;
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(dbEvent.CalendarData);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                calendarNode = nodeParser.Parse(stream);
            }
            return calendarNode;
        }
    }
}
