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
        /// <summary>
        /// Loads a resource.
        /// </summary>
        /// <param name="uri">The path of the resource or a unique filename.</param>
        /// <throws>Exception</throws>
        /// <returns>A opened file resource stream</returns>
        static public Stream GetStream(string uri)
        {
           StreamResourceInfo info = App.GetResourceStream(new Uri("OwnCloud;component" + uri, UriKind.Relative));
           if (info == null || info.Stream == null)
           {
               throw new Exception(String.Format("Resource \"{0:}\" not found!", uri));
           }
           else
           {
               return info.Stream;
           }
        }
    }
}
