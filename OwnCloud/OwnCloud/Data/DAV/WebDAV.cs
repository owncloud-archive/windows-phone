using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using OwnCloud.Net;

namespace OwnCloud.Data.DAV
{
    /// <summary>
    /// Provides methods and objects to deal with any WebDAV implementation
    /// </summary>
    class WebDAV
    {
        ICredentials _credit;
        Uri _host;
        Uri _relativeHost;

        struct RequestStruct
        {
            public HttpWebRequest Request;
            public DAVRequestBody Body;
            public Action<DAVRequestResult, object> Callback;
            public object UserObject;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="host">A valid host</param>
        /// <param name="credentials">Username and Password if necessary</param>
        public WebDAV(Uri host, ICredentials credentials = null)
        {
            _credit = credentials;
            _host = host;
        }

        /// <summary>
        /// If a Exception is thrown this member will turn to true
        /// </summary>
        public bool ErrorOccured
        {
            get
            {
                return LastException != null;
            }
            private set
            {
            }
        }

        /// <summary>
        /// The last Occured WebException.
        /// </summary>
        public WebException LastException
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts an asynchronous DAV-HTTP-Request.
        /// </summary>
        /// <param name="header">The DAV-Request header to used.</param>
        /// <param name="body">The DAV-Request-body to used.</param>
        /// <param name="userObject">User defined object to deliver to respose method.</param>
        /// <param name="response">A handler to call after the event completes.</param>
        public void StartRequest(DAVRequestHeader header, DAVRequestBody body, object userObject, Action<DAVRequestResult, object> response)
        {
            LastException = null;
            _relativeHost = new Uri(_host + header.RequestedResource);

            header.Headers[Header.Host] = _host.DnsSafeHost;
            // this is needed to modify some headers
            HttpWebRequest.RegisterPrefix("http", System.Net.Browser.WebRequestCreator.ClientHttp);
            HttpWebRequest.RegisterPrefix("https", System.Net.Browser.WebRequestCreator.ClientHttp);
            HttpWebRequest request = HttpWebRequest.CreateHttp(_host + header.RequestedResource);
            request.AllowReadStreamBuffering = false;
            request.Method = header.RequestedMethod.ToString();
            foreach (KeyValuePair<string, string> current in header.Headers)
            {
                // API restriction
                switch(current.Key) {
                    case "Accept":
                        request.Accept = current.Value;
                        break;
                    case "Content-Type":
                        request.ContentType = current.Value;
                        break;
                    default:
                        request.Headers[current.Key] = current.Value;
                        break;
                }
            }
            if (_credit != null)
            {
                request.Credentials = _credit;
            }

            if (body != null)
            {
                request.BeginGetRequestStream(_EndRequestStream, new RequestStruct
                {
                    Callback = response,
                    Request = request,
                    UserObject = userObject,
                    Body = body
                });
            }
            else
            {
                request.BeginGetResponse(_EndRequest, new RequestStruct
                {
                    Callback = response,
                    Request = request,
                    UserObject = userObject
                });
            }
        }

        /// <summary>
        /// Starts an asynchronous DAV-HTTP-Request.
        /// </summary>
        /// <param name="header">The DAV-Request header to used.</param>
        /// <param name="userObject">User defined object to deliver to respose method.</param>
        /// <param name="response">A handler to call after the event completes.</param>
        public void StartRequest(DAVRequestHeader header, object userObject, Action<DAVRequestResult, object> response)
        {
            StartRequest(header, null, userObject, response);
        }

        void _EndRequest(IAsyncResult result)
        {
            RequestStruct obj = (RequestStruct)result.AsyncState;
            try
            {
                // must catch "NotFoundError" when 5xx occurs (likely is)
                HttpWebResponse response = obj.Request.EndGetResponse(result) as HttpWebResponse;
                DAVRequestResult requestResult = new DAVRequestResult(this, response, _relativeHost);
                obj.Callback(requestResult, obj.UserObject);
            }
            catch (WebException we)
            {
                LastException = we;
                obj.Callback(new DAVRequestResult(this, ServerStatus.InternalServerError), obj.UserObject);
            }
        }

        void _EndRequestStream(IAsyncResult result)
        {
            RequestStruct obj = (RequestStruct)result.AsyncState;
            DAVRequestBody body = obj.Body;
            try
            {
                Stream stream = obj.Request.EndGetRequestStream(result);
                body.XmlBody.Seek(0, SeekOrigin.Begin);
                body.XmlBody.CopyTo(stream, 4096);
                stream.Close();

                obj.Request.BeginGetResponse(_EndRequest, obj);
            }
            catch (WebException we)
            {
                LastException = we;
                obj.Callback(new DAVRequestResult(this, ServerStatus.LocalFailure), obj.UserObject);
            }
        }

    }
}
