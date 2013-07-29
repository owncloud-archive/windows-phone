using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Xml.Linq;

namespace OwnCloud.Data.DAV
{
    class DAVRequestResult
    {

        MemoryStream _stream;

        /// <summary>
        /// Creates a empty object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="status"></param>
        public DAVRequestResult(WebDAV request, ServerStatus status)
        {
            Status = status;
            Request = request;
        }

        /// <summary>
        /// Parses the DAV result.
        /// Note that it will only parse if the server status was 207.
        /// There maybe error outputs on 4xx and 5xx but this will not parsed
        /// by this class.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="uri"></param>
        public DAVRequestResult(WebDAV request, HttpWebResponse response, Uri uri)
        {
            Request = request;
            Items = new List<Item>();
            Status = (ServerStatus)Enum.Parse(typeof(ServerStatus), response.StatusCode.ToString(), false);
            IsMultiState = Status == ServerStatus.MultiStatus;
            _stream = new MemoryStream();
            response.GetResponseStream().CopyTo(_stream);

            // dispose
            response.Dispose();

            _stream.Seek(0, SeekOrigin.Begin);

            // A kingdom for normal DOMDocument support.
            // Why not XDocument? XmlReader is faster and less resource hungry.
            // A huge multitstatus would be first loaded to memory completely.
            //
            // This reader can only go forward. Read-* methods will cause
            // to jump over elements. Hence we stop at the element and
            // store the element name. Then wait for Text-Elements value
            // to capture.
            using(XmlReader reader = XmlReader.Create(_stream, null)) {

                Item item = new Item();
                var waitForResourceType = false;
                var lastElementName = "";
                var magicElementName = "";

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.NamespaceURI == XmlNamespaces.NsDav)
                            {
                                lastElementName = reader.LocalName;
                            }

                            if (reader.LocalName == Elements.Response)
                            {
                                item = new Item();
                            }
                            break;
                            
                        case XmlNodeType.Text:

                            // no whitespace please
                            if (reader.Value == null) continue;

                            // can't set in empty element
                            if (item == null) continue;

                            switch (lastElementName)
                            {
                                case Properties.CreationDate:
                                    item.CreationDate = DateTime.Parse(reader.Value);
                                    break;
                                case Properties.DisplayName:
                                    item.DisplayName = reader.Value;
                                    break;
                                case Properties.GetContentLanguage:
                                    // todo
                                    break;
                                case Properties.GetContentLength:
                                    item.ContentLength = long.Parse(reader.Value);
                                    break;
                                case Properties.GetContentType:
                                    item.ContentType = reader.Value;
                                    break;
                                case Properties.GetETag:
                                    item.ETag = reader.Value;
                                    break;
                                case Properties.GetLastModified:
                                    item.LastModified = DateTime.Parse(reader.Value);
                                    break;
                                case Properties.LockDiscovery:
                                    // todo
                                    break;
                                case Properties.QuotaAvailableBytes:
                                    item.QuotaAvailable = long.Parse(reader.Value);
                                    break;
                                case Properties.QuotaUsedBytes:
                                    item.QuotaUsed = long.Parse(reader.Value);
                                    break;
                                case Properties.ResourceType:
                                    waitForResourceType = true;
                                    break;
                                case Properties.SupportedLock:
                                    // todo
                                    break;
                                case Elements.Collection:
                                    if (waitForResourceType)
                                    {
                                        waitForResourceType = false;
                                        item.ResourceType = ResourceType.Collection;
                                    }
                                    break;
                                case Elements.Reference:
                                    item.Reference = reader.Value;
                                    item.LocalReference = item.Reference.Substring(uri.LocalPath.Length, item.Reference.Length - uri.LocalPath.Length);
                                    break;
                                case Elements.Response:
                                    item = new Item();
                                    break;
                                case Elements.Status:
                                    item.StatusText = reader.Value;
                                    item.Status = (ServerStatus)Enum.Parse(typeof(ServerStatus), item.StatusText.Split(' ')[1], false);
                                    break;
                            }
                            lastElementName = "";
                            break;

                        case XmlNodeType.EndElement:
                            if (reader.NamespaceURI == XmlNamespaces.NsDav)
                            {
                                switch (reader.LocalName)
                                {
                                    case Elements.Response:
                                        if (item != null)
                                        {
                                            Items.Add(item);
                                            item = null;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// The associated DAV Request
        /// </summary>
        public WebDAV Request
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the HTTP status code.
        /// </summary>
        public ServerStatus Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the result content is a multi status response.
        /// </summary>
        public bool IsMultiState
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the raw response excluding headers.
        /// </summary>
        /// <returns></returns>
        public Stream GetRawResponse()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return _stream;
        }

        /// <summary>
        /// Returns all Response properties of this request.
        /// </summary>
        public List<Item> Items
        {
            get;
            private set;
        }

        /// <summary>
        /// Item-Sub-Class
        /// </summary>
        public class Item
        {
            public Item()
            {
                QuotaAvailable = 0;
                QuotaUsed = 0;
                ETag = "";
                LastModified = DateTime.Parse("01.01.1970");
                CreationDate = DateTime.Parse("01.01.1970");
                DisplayName = "";
                Reference = "";
                LocalReference = "";
                Status = ServerStatus.InternalServerError;
                StatusText = "";
                ContentLength = 0;
                ContentType = "";
                ResourceType = DAV.ResourceType.None;
            }

            /// <summary>
            /// Returns the used bytes in this quota.
            /// </summary>
            public long QuotaUsed
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the available bytes in this quota.
            /// </summary>
            public long QuotaAvailable
            {
                get;
                set;
            }

            /// <summary>
            /// ETag of this property.
            /// </summary>
            public string ETag
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the last modified date.
            /// </summary>
            public DateTime LastModified
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the creation date of this object.
            /// </summary>
            public DateTime CreationDate
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the displayed name of a reference
            /// </summary>
            public string DisplayName
            {
                get;
                set;
            }

            /// <summary>
            /// Gets locking information.
            /// </summary>
            public DAVLocking Locking
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the http status of this property.
            /// </summary>
            public ServerStatus Status
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the readable text status of this property.
            /// </summary>
            public string StatusText
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the resource reference uri
            /// </summary>
            public string Reference
            {
                get;
                set;
            }

            /// <summary>
            /// The reference uri without requesting uri (relative uri)
            /// </summary>
            public string LocalReference
            {
                get;
                set;
            }

            /// <summary>
            /// The elements resource type.
            /// </summary>
            public ResourceType ResourceType
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the mime content type.
            /// </summary>
            public string ContentType
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the content length.
            /// </summary>
            public long ContentLength
            {
                get;
                set;
            }
        }
    }
}
