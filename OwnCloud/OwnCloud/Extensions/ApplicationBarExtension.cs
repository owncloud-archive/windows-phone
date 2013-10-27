using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Shell;

namespace OwnCloud.Extensions
{
    static class ApplicationBarExtension
    {
        
        static List<ApplicationBarIconButton> _Cache = new List<ApplicationBarIconButton>();

        /// <summary>
        /// Translates all contained IconButtons.
        /// The IconButton-Text must begin with "ApplicationBarButton_"
        /// </summary>
        /// <param name="?"></param>
        public static void Translate(this IApplicationBar bar)
        {
            foreach (ApplicationBarIconButton button in bar.Buttons)
            {
                if (!_Cache.Contains(button) && button.Text.StartsWith("ApplicationBarButton_"))
                {
                    button.Text = button.Text.Translate();
                    _Cache.Add(button);
                }
            }
        }
    }
}
