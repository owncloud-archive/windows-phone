using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;
using OwnCloud.Data;

namespace OwnCloud.View.Controls
{
    public partial class FileDetailViewControl : UserControl
    {
        public FileDetailViewControl()
        {
            InitializeComponent();
        }

        public event EventHandler<System.Windows.Input.GestureEventArgs> OnEnableSyncTap;

        private Storyboard _board;

        /// <summary>
        /// Holds the File-Properties only available if DataContext was set properly.
        /// </summary>
        public File FileProperties
        {
            get
            {
                return (File)DataContext;
            }
        }

        private void EnableSyncTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (OnEnableSyncTap != null)
            {
                if (_board == null)
                {
                    _board = Resources["RotateSyncButton"] as Storyboard;
                    _board.Completed += new EventHandler(delegate
                    {
                        OnEnableSyncTap(this.DataContext, e);
                    });
                }
                _board.Begin();
            }
            e.Handled = true;
        }
    }
}
