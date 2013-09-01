using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwnCloud.View.Controls
{
     public class TemplateLogListSelector : LongListSelector
     {
        public object FindTemplateChild(string name)
        {
            return GetTemplateChild(name);
        }
    }
}
