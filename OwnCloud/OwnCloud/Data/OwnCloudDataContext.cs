using System;
using System.Data.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.Data.Linq.Mapping;
using System.IO.IsolatedStorage;

namespace OwnCloud.Data
{
    public class OwnCloudDataContext : DataContext, INotifyPropertyChanged
    {
        public static string DbConnectionString = "Data Source=isostore:/owncloud.sdf";

        public OwnCloudDataContext() : base(DbConnectionString)
        {
            // For later purpose:
            // New tables will not be automaticly generated if the database does exists
            if (!DatabaseExists()) CreateDatabase();
            
            // Delete some wrong stored data
            //Accounts.DeleteAllOnSubmit(Accounts);
            //SubmitChanges();
            new TemporaryData().Remove("EditAccountForm");
            new TemporaryData().Remove("AddAccountForm");

            LoadData();
        }

        public Table<TableCalendar> Calendars;
        public Table<TableEvent> Events;
        public Table<Account> Accounts;

        private DispatcherTimer _deviceStatusTimer;
        private IsolatedStorageFile _isf;

        /// <summary>
        /// Loads an account from the datebase.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Account LoadAccount(object guid)
        {
            var accounts = from acc in Accounts
                           where acc.GUID == int.Parse(guid.ToString())
                           select acc;
            return accounts.First();
        }

        /// <summary>
        /// Example File List
        /// </summary>
        public ObservableCollection<File> Files
        {
            get;
            private set;
        }

        public IsolatedStorageFile Storage
        {
            get
            {
                return _isf;
            }
        }
        
        private long _storageLocalFree = 0;
        /// <summary>
        /// Gets the available storage on local device in bytes
        /// </summary>
        public long LocalStorageFreeBytes
        {
            get
            {
                return _storageLocalFree;
            }
            set
            {
                _storageLocalFree = value;
                OnPropertyChanged("AvailableLocalStorage");
            }
        }

        /// <summary>
        /// Returns the available storage on local device as text
        /// </summary>
        public string AvailableLocalStorage
        {
            get
            {
                return Utility.FormatBytes(_storageLocalFree);
            }
        }

        public void LoadData()
        {
            // init isf
            _isf = IsolatedStorageFile.GetUserStoreForApplication();
            
            // init status timer
            _deviceStatusTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 2),
            };
            _deviceStatusTimer.Tick += delegate
            {
                LocalStorageFreeBytes = _isf.AvailableFreeSpace;
            };
            _deviceStatusTimer.Start();

            // load files
            Files = new ObservableCollection<File>();
            // Example data
            Files.Add(new File() { FileName = "Longfilename does not fit here.txt", FileSize = 234, FileType = "text/plain", FileLastModified = DateTime.Parse("2013-07-05T22:05:21+00:00") });
            Files.Add(new File() { FileName = "Something.html", FileSize = 12940, FileType = "text/html;charset=utf-8", FileLastModified = DateTime.Parse("2013-02-05T23:16:21+00:00") });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
