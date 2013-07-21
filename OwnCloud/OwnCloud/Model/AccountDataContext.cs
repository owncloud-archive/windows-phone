using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using OwnCloud.Model;

namespace OwnCloud
{
    public class AccountDataContext : INotifyPropertyChanged
    {
        public AccountDataContext()
        {
        }

        string _pageMode = null;
        public string PageMode
        {
            get
            {
                return _pageMode;
            }
            set
            {
                _pageMode = value;
                NotifyPropertyChanged("PageMode");
            }
        }

        Account _currentAccount = null;
        public Account CurrentAccount
        {
            get
            {
                return _currentAccount;
            }
            set
            {
                _currentAccount = value;
                NotifyPropertyChanged("CurrentAccount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

