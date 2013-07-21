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
