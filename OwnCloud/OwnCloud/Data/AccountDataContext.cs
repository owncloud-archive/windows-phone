using System;
using System.ComponentModel;

namespace OwnCloud.Data
{
    public class AccountDataContext : Entity
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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
            }
        }
    }
}

