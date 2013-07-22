using System;
using System.ComponentModel;
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

