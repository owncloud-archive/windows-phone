using OwnCloud.Resource;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OwnCloud.Resource.Localization;

namespace OwnCloud
{
    /// <summary>
    /// Provides access to language strings.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }

        static public string Get(string key)
        {
            try
            {
                return (string)_localizedResources.GetType().GetProperty(key).GetValue(_localizedResources, null);
            }
            catch (Exception)
            {
                return String.Format("<Localization: {0:g}>", key);
            }
        }
    }
}