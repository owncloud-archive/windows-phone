using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Shell;

namespace OwnCloud.Extensions
{
    public static class TranslationExtension
    {
        /// <summary>
        /// Translates the IconButtons in a ApplicationBar
        /// </summary>
        /// <param name="bar"></param>
        public static void TranslateButtons(this IApplicationBar bar) {
            // Translate unsupport XAML bindings
            foreach (ApplicationBarIconButton button in bar.Buttons)
            {
                button.Text = button.Text.Translate();
            }
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
