using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using OwnCloud.Data;
using OwnCloud.Data.Calendar;
using OwnCloud.Data.Calendar.ParsedCalendar;

namespace OwnCloud.Net
{
    static class MultistateParser
    {
        public static UnparsedEvent[] ParseCalendarEvents(Stream stream)
        {
            var rootItem = LoadRootItem(stream);

            var responses = rootItem.Elements();

            var calendars = new List<UnparsedEvent>();

            foreach (var response in responses)
            {
                var curEvent = new UnparsedEvent
                    {
                        EventInfo = new EventCalDavInfo {Url = response.GetIfExists(XName.Get("href", XmlNamespaces.NSd))}
                    };

                var propstats = response.Elements(XName.Get("propstat", XmlNamespaces.NSd));

                foreach (var propstat in propstats)
                {
                    if (propstat.GetIfExists(XName.Get("status", XmlNamespaces.NSd)) == "HTTP/1.1 200 OK")
                    {
                        var prop = propstat.Element(XName.Get("prop", XmlNamespaces.NSd));

                        foreach (var propValue in prop.Elements())
                        {
                            switch (propValue.Name.LocalName)
                            {
                                case "getetag":
                                    curEvent.EventInfo.GetETag = propValue.Value;
                                    break;
                                case "calendar-data":
                                    curEvent.RawEventData = propValue.Value;
                                    break;
                            }
                        }
                    }
                }

                calendars.Add(curEvent);

            }

            return calendars.ToArray();
        }

        public static CalendarCalDavInfo[] ParseCalendarCalDavInfo(Stream stream)
        {
            var rootItem = LoadRootItem(stream);

            var responses = rootItem.Elements();

            var calendars = new List<CalendarCalDavInfo>();

            foreach (var res in responses)
            {
                var info = new CalendarCalDavInfo();

                info.Url = res.Elements(XName.Get("href",XmlNamespaces.NSd)).Single().Value;

                var propstat = res.Element(XName.Get("propstat", XmlNamespaces.NSd));
                if (propstat != null)
                {
                    var status = propstat.GetIfExists(XName.Get("status", XmlNamespaces.NSd));
                    if (status != "HTTP/1.1 200 OK") continue;
                }
                else continue;

                foreach (var prop in propstat.Elements(XName.Get("prop", XmlNamespaces.NSd)))
                {
                    info.DisplayName = prop.GetIfExists(XName.Get("displayname", XmlNamespaces.NSd));
                    info.GetCTag = prop.GetIfExists(XName.Get("getctag", XmlNamespaces.NScs));
                }

                calendars.Add(info);
            }

            return calendars.ToArray();
        }

        private static string GetIfExists(this XElement element, XName name)
        {
            var xElement = element.Element(name);
            return xElement != null ? xElement.Value : null;
        }

        private static XElement LoadRootItem(Stream stream)
        {
            return XDocument.Load(stream).Descendants().First();
        }

    }
}
