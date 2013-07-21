using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace OwnCloud.Data
{
    /// <summary>
    /// Copies any object to temporary IsolatedStorage and can be accessed
    /// as long the application is open, eg. for Form Details.
    /// </summary>
    class TemporaryData
    {
        /// <summary>
        /// Tries to restore a object from isolatedstorage or returns
        /// the default object.
        /// </summary>
        /// <param name="formName"></param>
        /// <param name="defaultObject"></param>
        /// <returns></returns>
        public object Restore(string formName, Object defaultObject)
        {
            try
            {
                return IsolatedStorageSettings.ApplicationSettings[formName];
            }
            catch (Exception)
            {
                return defaultObject;
            }
        }

        /// <summary>
        /// Stores or updates an object in isolatedstorage.
        /// </summary>
        /// <param name="formName"></param>
        /// <param name="storeObject"></param>
        public void Store(string formName, Object storeObject)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(formName))
            {
                IsolatedStorageSettings.ApplicationSettings[formName] = storeObject;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(formName, storeObject);
            }
        }

        /// <summary>
        /// Removes a stored object
        /// </summary>
        /// <param name="formName"></param>
        public void Remove(string formName)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(formName))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(formName);
            }
        }
    }
}
