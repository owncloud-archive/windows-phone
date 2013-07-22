using System.IO;
using System.Text;
using System.Xml;

namespace OwnCloud.Data.Calendar.Request
{
    /// <summary>
    /// A class that creates a request to get all calendars of the CalDav instance
    /// </summary>
    class CalendarRequest
    {

        public static void WriteCalendarRequest(Stream target)
        {
            XmlWriter writer = XmlWriter.Create(target, new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8
            });

            writer.WriteStartElement("propfind", "DAV:");
            writer.WriteAttributeString("xmlns", "d", null, "DAV:");
            writer.WriteAttributeString("xmlns", "cs", null, "http://calendarserver.org/ns/");

            writer.WriteStartElement("prop", "DAV:");

            writer.WriteStartElement("displayname", "DAV:");
            writer.WriteEndElement();

            writer.WriteStartElement("getctag", "http://calendarserver.org/ns/");
            writer.WriteEndElement();

            writer.WriteEndElement();


            writer.WriteEndElement();


            writer.Flush();
            writer.Close();
        }

    }
}
