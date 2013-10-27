using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Shell;

namespace OwnCloud.Extensions
{
    public static class TranslationExtension
    {
        static private List<object> _usedObj = new List<object>();

        /// <summary>
        /// Translates the IconButtons in a ApplicationBar. 
        /// This call will work only one time unless ClearTranslationUsedObject() is called.
        /// </summary>
        /// <param name="bar"></param>
        public static void TranslateButtons(this IApplicationBar bar) {
            if (_usedObj.Contains(bar)) return;
            // Translate unsupport XAML bindings
            foreach (ApplicationBarIconButton button in bar.Buttons)
            {
                button.Text = button.Text.Translate();
            }
            _usedObj.Add(bar);
        }

        /// <summary>
        /// This will remove an object from used list.
        /// </summary>
        /// <param name="obj"></param>
        public static void ClearTranslationUsedObject(object obj)
        {
            if (_usedObj.Contains(obj)) _usedObj.Remove(obj);
        }

        /// <summary>
        /// Translates a string
        /// </summary>
        /// <param name="value">Translation index key</param>
        /// <param name="param">String.Format parameters</param>
        /// <returns></returns>
        public static string Translate(this string key, params object[] param)
        {
            return string.Format(LocalizedStrings.Get(key), param);
        }
    }
}
