using System;
using System.Linq;
using System.IO;
using OwnCloud.Data.Calendar.ParsedCalendar;

namespace OwnCloud.Data.Calendar.Parsing
{
    public class ParserICal
    {
        public CalendarICal Parse(Stream value)
        {
            var nodeParser = new ParserNodeToken();
            var rootNode = nodeParser.Parse(value).Childs[0];

            var resultCal = new CalendarICal();

            TokenNode eventNode = null;
            int i = 0;
            while ((eventNode = FindNextChild(rootNode, i, "VEVENT")) != null)
            {
                resultCal.Events.Add(ParseVEvent(eventNode));
                i++;
            }

            return resultCal;
        }

        private VEvent ParseVEvent(TokenNode node)
        {
            var cEvent = new VEvent();

            var refDate = DateTime.MinValue;
            bool refBool = false;
            string refString = "";

            if (TryFindDate(node, "DTSTART", ref refDate, out refBool))
            {
                cEvent.From = refDate;
                cEvent.IsFullDayEvent = refBool;
            }
            if (TryFindDate(node, "DTEND", ref refDate, out refBool))
            {
                cEvent.To = refDate;
                cEvent.IsFullDayEvent = refBool;
            }
            if ((refString = TryFindString(node, "SUMMARY")) != null)
                cEvent.Title = refString;
            if ((refString = TryFindString(node, "DESCRIPTION")) != null)
                cEvent.Description = refString;
            
            return cEvent;
        }

        private bool TryFindDate(TokenNode node, string tokenName, ref DateTime result, out bool isFullDayTime)
        {
            var tokenQuery = node.Tokens.Where(o => o.NamingKey == tokenName).ToArray();

            if (tokenQuery.Any())
            {
                var dateTime = ParserDateTime.Parse(tokenQuery.First().Value.EncodedValue, out isFullDayTime);

                if (dateTime.HasValue)
                    result = dateTime.Value;
                else
                    return false;

                return true;
            }
            else isFullDayTime = false;

            return false;
        }

        private string TryFindString(TokenNode node, string tokenName)
        {
            var tokenQuery = node.Tokens.Where(o => o.NamingKey == tokenName).ToArray();

            if (tokenQuery.Any())
            {
                return tokenQuery.First().Value.DecodedValue;
            }
            return null;
        }

        private TokenNode FindNextChild(TokenNode rootNode,int minIndex, string objectName)
        {
            var calendarQuery = rootNode.Childs.Where(o => o.Name == objectName);

            if (calendarQuery.Count() > minIndex)
            {
                return calendarQuery.Skip(minIndex).Take(1).Single();
            }

            return null;
        }

    }
}
