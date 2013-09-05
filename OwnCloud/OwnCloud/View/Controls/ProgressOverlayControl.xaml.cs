using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace OwnCloud.View.Controls
{
    public partial class ProgressOverlayControl : UserControl
    {
        private string _statusText = "";
        private int _zIndex = 1000;
        Popup _owner;
        Grid _base;
        PerformanceProgressBar _bar;
        TextBlock _textBlock;

        /// <summary>
        /// Creates a new overlay progress bar.
        /// </summary>
        /// <param name="statusText">A initial value.</param>
        public ProgressOverlayControl(string statusText = "")
        {
            InitializeComponent();
            this.Visibility = System.Windows.Visibility.Collapsed;

            _statusText = statusText;
            _base = (Grid)this.FindName("LayoutRoot");
            _bar = (PerformanceProgressBar)this.FindName("OverlayProgressBar");
            _textBlock = (TextBlock)this.FindName("OverlayText");
            _textBlock.Text = statusText;            
        }

        /// <summary>
        /// The used Grid to manipulate.
        /// </summary>
        public Grid Grid
        {
            get
            {
                return _base;
            }
        }

        /// <summary>
        /// The used ProgressBar to manipulate.
        /// </summary>
        public PerformanceProgressBar ProgressBar
        {
            get
            {
                return _bar;
            }
        }

        /// <summary>
        /// The used TextBlock to manipulate.
        /// </summary>
        public TextBlock TextBlock
        {
            get
            {
                return _textBlock;
            }
        }

        /// <summary>
        /// The Statustext to be displayed.
        /// </summary>
        public string StatusText
        {
            get
            {
                return _textBlock.Text;
            }
            set
            {
                _textBlock.Text = value;
            }
        }
        
    }
}
