using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace OwnCloud.Data.Calendar.Request
{
    /// <summary>
    /// A class, that creates a Request, to get all events in a calendar
    /// </summary>
    class CalendarEventRequest
    {
        public bool LoadCalendarData = true;
        public List<string> Urls = new List<string>();

        /// <summary>
        /// Writes the request in a Steam
        /// </summary>
        public void WriteCalendarRequest(Stream target)
        {
            XmlWriter writer = XmlWriter.Create(target, new XmlWriterSettings()
                {
                    Encoding = Encoding.UTF8
                });

            if (Urls.Count == 0)
                WriteCalendarQuery(writer);
            else
                WriteMultiget(writer);


            writer.Flush();
            writer.Close();

        }

        private void WriteCalendarQuery(XmlWriter writer)
        {
            writer.WriteStartElement("calendar-query", XmlNamespaces.NsCaldav);
            writer.WriteAttributeString("xmlns", "d", null, XmlNamespaces.NsDav);
            writer.WriteAttributeString("xmlns", "c", null, XmlNamespaces.NsCaldav);


            writer.WriteStartElement("prop", XmlNamespaces.NsDav);

            writer.WriteStartElement("getetag", XmlNamespaces.NsDav);
            writer.WriteEndElement();

            if (LoadCalendarData)
            {
                writer.WriteStartElement("calendar-data", XmlNamespaces.NsCaldav);
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // End d:Prop

            foreach (var url in Urls)
            {
                writer.WriteStartElement("href", XmlNamespaces.NsDav);
                writer.WriteString(url);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("filter", XmlNamespaces.NsCaldav);

            writer.WriteStartElement("comp-filter", XmlNamespaces.NsCaldav);
            writer.WriteAttributeString("name", "VCALENDAR");

            //Only Events
            writer.WriteStartElement("comp-filter", XmlNamespaces.NsCaldav);
            writer.WriteAttributeString("name", "VEVENT");

            //End c:comp-filter (name=VEVENT)
            writer.WriteEndElement();

            //End c:comp-filter
            writer.WriteEndElement();

            //End c:Filter
            writer.WriteEndElement();


            //End calendar query
            writer.WriteEndElement();
        }

        private void WriteMultiget(XmlWriter writer)
        {
            writer.WriteStartElement("calendar-multiget", XmlNamespaces.NsCaldav);
            writer.WriteAttributeString("xmlns", "d", null, XmlNamespaces.NsDav);
            writer.WriteAttributeString("xmlns", "c", null, XmlNamespaces.NsCaldav);


            writer.WriteStartElement("prop", XmlNamespaces.NsDav);

            writer.WriteStartElement("getetag", XmlNamespaces.NsDav);
            writer.WriteEndElement();

            if (LoadCalendarData)
            {
                writer.WriteStartElement("calendar-data", XmlNamespaces.NsCaldav);
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // End d:Prop

            foreach (var url in Urls)
            {
                writer.WriteStartElement("href", XmlNamespaces.NsDav);
                writer.WriteString(url);
                writer.WriteEndElement();
            }

            //End calendar-multiget
            writer.WriteEndElement();
        }
    }
}
