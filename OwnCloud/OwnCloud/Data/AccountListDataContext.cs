using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace OwnCloud.Data
{
    public class AccountListDataContext : Entity
    {
        public AccountListDataContext()
        {
            Loaddata();
        }

        public ObservableCollection<Account> Accounts
        {
            get;
            set;
        }

        public void Loaddata()
        {
            Accounts = new ObservableCollection<Account>();
            foreach (Account account in App.DataContext.Accounts)
            {
                Accounts.Add(account);
            }
        }
    }
}
