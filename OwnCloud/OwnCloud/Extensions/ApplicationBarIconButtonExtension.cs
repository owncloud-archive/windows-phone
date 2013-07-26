using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Shell;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace OwnCloud.Extensions
{
    public static class ApplicationBarIconButtonExtension
    {
        /// <summary>
        /// Does an autocommit on the last focused element binding.
        /// Works with textboxes and textblocks so far.
        /// </summary>
        /// <param name="button">The calling ApplicationBarIconButton</param>
        public static void UpdateBindingSource(this ApplicationBarIconButton button)
        {
            var obj = FocusManager.GetFocusedElement();
            if (obj != null)
            {
                if (obj.GetType() == typeof(TextBox))
                {
                    (obj as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }
                else if (obj.GetType() == typeof(TextBlock))
                {
                    (obj as TextBlock).GetBindingExpression(TextBlock.TextProperty).UpdateSource();
                }
            }
        }
    }
}
