using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace OwnCloud.Model
{
    public class AccountListDataContext : INotifyPropertyChanged
    {
        public AccountListDataContext()
        {
            this.Accounts = new ObservableCollection<Account>();
            Loaddata();
        }

        public ObservableCollection<Account> Accounts
        {
            get;
            set;
        }

        public void Loaddata()
        {
            string[] accountFileObjects = Serialize.ListFiles("Accounts");
            foreach (string filename in accountFileObjects)
            {
                try
                {
                    Account acc = (Account)Serialize.ReadFile(@"Accounts\" + filename, typeof(Account));
                    acc.GUID = filename;
                    Accounts.Add(acc);
                }
                catch (Exception ex)
                {
                    Utility.Debug(ex.Message);
                }
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
