using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

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

            using (var context = new OwnCloudDataContext())
            {
                foreach (var account in context.Accounts.ToArray())
                {
                    Accounts.Add(account);
                }
            }
        }
    }
}
