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
    class DAVRequestResult : IDisposable
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
            Items = new List<Item>();
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
            if (_stream.Length == 0) return;

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
                var waitForLockScope = false;
                var waitForLockType = false;
                List<DAVLocking> lockingList = null;
                List<PropertyState> propertyStateList = null;
                PropertyState pItem = null;
                DAVLocking litem = null;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        // look for special elements
                        case XmlNodeType.Element:
                            if (reader.NamespaceURI == XmlNamespaces.NsDav)
                            {
                                switch (reader.LocalName)
                                {
                                    // DAV Elements

                                    // Response
                                    case Elements.Response:
                                        // start a new item
                                        // pItem must be set before d:prop in order to
                                        // catch non-real properties such "href"
                                        item = new Item();
                                        propertyStateList = new List<PropertyState>();
                                        pItem = new PropertyState();
                                        break;

                                    // Resource type
                                    case Elements.Collection:
                                        if (waitForResourceType)
                                        {
                                            item.ResourceType = ResourceType.Collection;
                                        }
                                        break;

                                    // Lock
                                    case Elements.LockEntry:
                                        litem = new DAVLocking();
                                        lockingList.Add(litem);
                                        break;
                                    case Elements.LockScope:
                                        waitForLockScope = true;
                                        break;
                                    case Elements.LockType:
                                        waitForLockType = true;
                                        break;
                                    case Elements.ExclusiveLocking:
                                        if (waitForLockScope)
                                        {
                                            litem.Scope = DAVLocking.LockScope.Exclusive;
                                        }
                                        break;
                                    case Elements.SharedLocking:
                                        if (waitForLockScope)
                                        {
                                            litem.Scope = DAVLocking.LockScope.Shared;
                                        }
                                        break;
                                    case Elements.WriteLocking:
                                        if (waitForLockType)
                                        {
                                            litem.Type = DAVLocking.LockType.Write;
                                        }
                                        break;
                                    case Elements.LockDiscovery:
                                        ///TODO 
                                        break;

                                    // DAV Properties
                                    case Elements.Properties:
                                        // a pItem was already created before
                                        break;

                                    case Properties.ResourceType:
                                        waitForResourceType = true;
                                        break;

                                    case Properties.SupportedLock:
                                        lockingList = new List<DAVLocking>();
                                        break;
                                    
                                    default:
                                        lastElementName = reader.LocalName;
                                        break;
                                }
                            }
                            break;
                        
                        // clean up
                        case XmlNodeType.EndElement:
                            if (reader.NamespaceURI == XmlNamespaces.NsDav)
                            {
                                switch (reader.LocalName)
                                {
                                    // DAV Elements
                                    case Elements.PropertyState:
                                        // save to list and create a new one (which stays maybe temporary)
                                        propertyStateList.Add(pItem);
                                        pItem = new PropertyState();
                                        break;

                                    case Elements.Response:
                                        // clean the list
                                        // the HTTP Status is important
                                        foreach (PropertyState state in propertyStateList)
                                        {
                                            if (state.Status == ServerStatus.OK)
                                            {
                                                item.Properties = state.Properties;
                                            }
                                            else
                                            {
                                                item.FailedProperties.Add(state);
                                            }
                                        }

                                        // Close the item
                                        Items.Add(item);
                                        item = null;

                                        // Reset the property state list
                                        propertyStateList = null;
                                        pItem = null;
                                        break;

                                    // Locking
                                    case Elements.LockType:
                                        waitForLockType = false;
                                        break;
                                    case Elements.LockScope:
                                        waitForLockScope = false;
                                        break;

                                    // DAV Properties
                                    case Properties.ResourceType:
                                        waitForResourceType = false;
                                        break;
                                    case Properties.SupportedLock:
                                        item.Locking = lockingList;
                                        break;

                                }
                            }
                            break;

                        // Grap the text values
                        case XmlNodeType.Text:

                            // no whitespace please
                            if (reader.Value == null) continue;

                            // can't set in empty element
                            if (item == null) continue;

                            switch (lastElementName)
                            {
                                // DAV Elements
                                case Elements.Reference:
                                    string _ref = Uri.UnescapeDataString(reader.Value);
                                    string _localRef = _ref.Substring(uri.LocalPath.Length, _ref.Length - uri.LocalPath.Length);
                                    pItem.Properties.Add(lastElementName, _ref);
                                    pItem.Properties.Add(lastElementName + ".local", _localRef.Trim('/'));
                                    break;

                                // Status element
                                case Elements.Status:
                                    List<string> s = new List<string>(reader.Value.Split(' '));
                                    s.RemoveAt(0);
                                    pItem.Status = (ServerStatus)Enum.Parse(typeof(ServerStatus), s[0], false);
                                    s.RemoveAt(0);
                                    pItem.ServerStatusText = String.Join(" ", s.ToArray());
                                    break;

                                // DAV Properties
                                case Properties.QuotaUsedBytes:
                                case Properties.QuotaAvailableBytes:
                                case Properties.GetContentLength:
                                    pItem.Properties.Add(lastElementName, long.Parse(reader.Value));
                                    break;
                                case Properties.DisplayName:
                                case Properties.GetContentLanguage:
                                case Properties.GetContentType:
                                case Properties.GetETag:
                                    pItem.Properties.Add(lastElementName, reader.Value);
                                    break;
                                case Properties.GetLastModified:
                                case Properties.CreationDate:
                                    pItem.Properties.Add(lastElementName, DateTime.Parse(reader.Value));
                                    break;
                            }
                            lastElementName = "";
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

        public void Dispose()
        {
            _stream.Dispose();
        }

        /// <summary>
        /// Collection of DAV properties
        /// </summary>
        public class PropertyState
        {
            public Dictionary<string, object> Properties
            {
                get;
                set;
            }

            public ServerStatus Status
            {
                get;
                set;
            }

            public string ServerStatusText
            {
                get;
                set;
            }

            public PropertyState()
            {
                Properties = new Dictionary<string, object>();
                Status = ServerStatus.InternalServerError;
                ServerStatusText = "";
            }
        }

        /// <summary>
        /// Item-Sub-Class
        /// </summary>
        public class Item
        {
            public Item()
            {
                ResourceType = DAV.ResourceType.None;
                Locking = new List<DAVLocking>();
                FailedProperties = new List<PropertyState>();
            }

            protected object _GetValue(string key, object defaultValue)
            {
                if (Properties.ContainsKey(key))
                {
                    return Properties[key];
                }
                else
                {
                    return defaultValue;
                }
            }

            /// <summary>
            /// Returns the used bytes in this quota.
            /// </summary>
            public long QuotaUsed
            {
                get
                {
                    return (long)_GetValue(DAV.Properties.QuotaUsedBytes, (long)0);
                }
                set
                {
                }
            }

            /// <summary>
            /// Returns the available bytes in this quota.
            /// </summary>
            public long QuotaAvailable
            {
                get
                {
                    return (long)_GetValue(DAV.Properties.QuotaAvailableBytes, (long)0);
                }
                set
                {
                }
            }

            /// <summary>
            /// ETag of this property.
            /// </summary>
            public string ETag
            {
                get
                {
                    return (string)_GetValue(DAV.Properties.GetETag, "");
                }
                set
                {
                }
            }

            /// <summary>
            /// Returns the last modified date.
            /// </summary>
            public DateTime LastModified
            {
                get
                {
                    return (DateTime)_GetValue(DAV.Properties.GetLastModified, new DateTime());
                }
                set
                {
                }
            }

            /// <summary>
            /// Returns the creation date of this object.
            /// </summary>
            public DateTime CreationDate
            {
                get
                {
                    return (DateTime)_GetValue(DAV.Properties.CreationDate, new DateTime());
                }
                set
                {
                }
            }

            /// <summary>
            /// Returns the displayed name of a reference
            /// </summary>
            public string DisplayName
            {
                get
                {
                    return (string)_GetValue(DAV.Properties.DisplayName, "");
                }
                set
                {
                }
            }

            /// <summary>
            /// Gets locking information.
            /// </summary>
            public List<DAVLocking> Locking
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the resource reference uri
            /// </summary>
            public string Reference
            {
                get
                {
                    return (string)_GetValue(DAV.Elements.Reference, "");
                }
                set
                {
                }
            }

            /// <summary>
            /// The reference uri without requesting uri (relative uri)
            /// </summary>
            public string LocalReference
            {
                get
                {
                    return (string)_GetValue(DAV.Elements.Reference+".local", "");
                }
                set
                {
                }
            }

            /// <summary>
            /// Returns the parent reference name.
            /// </summary>
            public string ParentReference
            {
                get
                {
                    var p = new List<string>(Reference.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
                    p.RemoveAt(p.Count - 1);
                    return String.Join("/", p.ToArray());
                }
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
                get
                {
                    return (string)_GetValue(DAV.Properties.GetContentType, "");
                }
                set
                {
                }
            }

            /// <summary>
            /// Gets the content length.
            /// </summary>
            public long ContentLength
            {
                get
                {
                    return (long)_GetValue(DAV.Properties.GetContentLength, (long)0);
                }
                set
                {
                }
            }

            /// <summary>
            /// Gets the content length.
            /// </summary>
            public string ContentLanguage
            {
                get
                {
                    return (string)_GetValue(DAV.Properties.GetContentLanguage, "");
                }
                set
                {
                }
            }

            /// <summary>
            /// Contains all properties which could be resolved.
            /// </summary>
            public Dictionary<string,object> Properties
            {
                get;
                set;
            }
            
            /// <summary>
            /// Returns all Properties which could not be resolved
            /// on the current item response.
            /// </summary>
            public List<PropertyState> FailedProperties
            {
                get;
                set;
            }
        }
    }
}
