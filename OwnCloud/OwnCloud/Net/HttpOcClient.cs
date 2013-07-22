using System.Net;
using OwnCloud.Data.Exceptions;

namespace OwnCloud.Net
{
    /// <summary>
    /// Basisklasse für die Http Kommunikation mit dem Owncloud Server
    /// </summary>
    class HttpOcClient
    {

        public HttpOcClient(string ocAddress,OwncloudCredentials credentials)
        {
            Address = new OwncloudAddress(ocAddress);
            Credentials = credentials;
        }

        protected OwncloudAddress Address;
        protected System.Net.HttpWebRequest Request;
        protected bool _busy = false;
        protected OwncloudCredentials Credentials;


        
        #region Common Httpclient functions

        /// <summary>
        /// Resettet den HttpRequest und bereitet ihn auf eine Verbindung vor
        /// </summary>
        protected void ResetHttpRequest(string url)
        {
            if (Request != null)
                Request.Abort();

            Request = WebRequest.CreateHttp(url);
            Request.Credentials = new NetworkCredential(Credentials.Username,Credentials.Password);
            Request.UserAgent = "OwncloudClient for Windows Phone";

        }

        /// <summary>
        /// Versucht eine neue Http Anforderung vorzubereiten. Nur wenn der Client nicht beschäftigt ist.
        /// </summary>
        protected void BeginHttpRequest(string url)
        {
            if (_busy)
                throw new ClientBusyException();

            ResetHttpRequest(url);
        }

        /// <summary>
        /// Markiert die aktuelle Anforderung als beendet
        /// </summary>
        protected void EndHttpRequest()
        {
            _busy = false;
        }

        /// <summary>
        /// Setzt den COntent Type des Requests
        /// </summary>
        /// <param name="type"></param>
        private void SetContentType(string type)
        {
            Request.ContentType = type;
        }

        /// <summary>
        /// Setzt den COntent Type auf Xml
        /// </summary>
        protected void ContentTypeXml()
        {
            SetContentType("application/xml; charset=utf-8");
        }

        /// <summary>
        /// Setzt den Content Type auf Calendar
        /// </summary>
        protected void ContentTypeCalendar()
        {
            SetContentType("text/calendar; charset=utf-8");
        }

        /// <summary>
        /// Setzt die Methode mes Http Requests
        /// </summary>
        /// <param name="meth">Die Http Methode</param>
        protected void SetMethod(string meth)
        {
            Request.Method = meth;
        }

        #endregion


    }
}
