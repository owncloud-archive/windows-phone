using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Resources;

namespace OwnCloud.Resource
{
    class ResourceLoader
    {

        public class ResourceInfo
        {
            public Stream Stream;
            public Uri WorkingURI;
        }

        /// <summary>
        /// Loads a resource.
        /// </summary>
        /// <param name="uri">The path of the resource or a unique filename.</param>
        /// <throws>Exception</throws>
        /// <returns>A opened file resource stream</returns>
        static public Stream GetStream(string uri)
        {
            ResourceInfo info = ResourceStatus(uri);
           if (info == null || info.Stream == null)
           {
               throw new Exception(String.Format("Resource \"{0:}\" not found!", uri));
           }
           else
           {
               return info.Stream;
           }
        }

        /// <summary>
        /// Checks if a resource is existing by open the stream
        /// </summary>
        /// <param name="uri">The path of the resource or a unique filename.</param>
        /// <returns></returns>
        static public bool ResourceExists(string uri)
        {
            ResourceInfo info = ResourceStatus(uri);
            return info == null ? false : info.Stream != null;
        }

        static public ResourceInfo ResourceStatus(string uri)
        {
            Uri[] test = new Uri[2] {
                new Uri("/OwnCloud;component" + uri, UriKind.Relative),
                new Uri(uri.TrimStart('/'), UriKind.Relative)
            };

            foreach (Uri current in test)
            {
                StreamResourceInfo info = App.GetResourceStream(current);
                if (info != null && info.Stream != null)
                {
                    return new ResourceInfo
                    {
                        Stream = info.Stream,
                        WorkingURI = current
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the stream uri if a stream could open on the given relative uri.
        /// </summary>
        /// <param name="uri">The path of the resource or a unique filename.</param>
        /// <returns></returns>
        static public Uri GetStreamUri(string uri)
        {
            ResourceInfo result = ResourceStatus(uri);
            if (result != null)
            {
                return result.WorkingURI;
            }
            return null;
        }
    }
}
