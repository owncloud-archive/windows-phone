using System;
using System.Windows;
using System.Data.Linq;
using System.ComponentModel;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns true if file previews are enabled globally.
        /// Default: true
        /// </summary>
        public bool EnableFilePreview
        {
            get
            {
                bool value;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("EnableFilePreview", out value))
                {
                    return true;
                }
                return value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["EnableFilePreview"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
                OnPropertyChanged("FilePreviewSettingsVisibility");
            }
        }

        /// <summary>
        /// Returns true if the remote file preview is limited to WLAN-connections only.
        /// Default: false
        /// </summary>
        public bool EnableRemoteFilePreviewWLANOnly
        {
            get
            {
                bool value;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("EnableRemoteFilePreviewWLANOnly", out value))
                {
                    return false;
                }
                return value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["EnableRemoteFilePreviewWLANOnly"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        /// <summary>
        /// If remote files are bigger than a choosen size the file will be skipped for preview.
        /// This settings is enabled as default.
        /// Default: true
        /// </summary>
        public bool EnableSkipRemoteFiles
        {
            get
            {
                bool value;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("EnableSkipRemoteFiles", out value))
                {
                    return true;
                }
                return value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["EnableSkipRemoteFiles"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        /// <summary>
        /// This is the limit for skipping files on remote file preview.
        /// Default: 100 (KiB)
        /// </summary>
        public uint SkipRemoteFilesLimit
        {
            get
            {
                uint value;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("SkipRemoteFilesLimit", out value))
                {
                    return 100;
                }
                return value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["SkipRemoteFilesLimit"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        /// <summary>
        /// Returns or sets the Amount of Bytes (in KiB) to Skip a file on remote file preview.
        /// </summary>
        public string SkipRemoteFilesOverBytes
        {
            get
            {
                return SkipRemoteFilesLimit.ToString();
            }
            set
            {
                uint result;
                if (UInt32.TryParse(value, out result))
                {
                    SkipRemoteFilesLimit = result;
                }
                else
                {
                    // invalid settings are turned to default
                    IsolatedStorageSettings.ApplicationSettings.Remove("SkipRemoteFilesLimit");
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }
        }

        /// <summary>
        /// Returns true if files are getting deleted in Cloud Storage too if removed from device.
        /// Default: false
        /// </summary>
        public bool EnableRemoteDeleting
        {
            get
            {
                bool value;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("EnableRemoteDeleting", out value))
                {
                    return false;
                }
                return value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["EnableRemoteDeleting"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        /// <summary>
        /// Simple controls the visibility of the file preview settings panel
        /// </summary>
        public Visibility FilePreviewSettingsVisibility
        {
            get
            {
                return EnableFilePreview ? Visibility.Visible : Visibility.Collapsed;
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
