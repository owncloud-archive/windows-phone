using System;
using System.Data.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
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
            if (!this.DatabaseExists()) this.CreateDatabase();
            LoadData();
        }

        public Table<TableCalendar> Calendars;
        public Table<TableEvent> Events;
        public Table<Account> Accounts;

        private DispatcherTimer _deviceStatusTimer;
        private IsolatedStorageFile _isf;

        /// <summary>
        /// Example File List
        /// </summary>
        public ObservableCollection<File> Files
        {
            get;
            private set;
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
            Files.Add(new File() { Filename = "Longfilename does not fit here.txt", Filesize = 234, Filetype = "text/plain", FileLastModified = DateTime.Parse("2013-07-05T22:05:21+00:00") });
            Files.Add(new File() { Filename = "Something.html", Filesize = 12940, Filetype = "text/html;charset=utf-8", FileLastModified = DateTime.Parse("2013-02-05T23:16:21+00:00") });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
