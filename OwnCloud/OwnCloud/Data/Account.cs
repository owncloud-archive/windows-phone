using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Windows;
using System.Xml.Serialization;
using System.Data.Linq.Mapping;
using OwnCloud.Extensions;
using System.Net;

namespace OwnCloud.Data
{
    [Table(Name="Accounts")]
    public class Account : Entity
    {
        public Account()
        {
            WebDAVPath = "/owncloud/remote.php/webdav/";
            CalDAVPath = "/owncloud/remote.php/caldav/";
            Protocol = "https";
            Username = "Alex";
            Password = "";
            ServerDomain = "my.flarandr.de";
        }

        private int _id;
        /// <summary>
        /// Primary Key
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int GUID
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id == value) return;
                _id = value;
                NotifyPropertyChanged();
            }
        }

        [Column]
        public bool IsEncrypted { 
            get; 
            set;
        }

        /// <summary>
        /// Server Domain to connect to
        /// </summary>
        ///
        [Column]
        public string ServerDomain
        {
            get;
            set;
        }

        /// <summary>
        /// Only returns the hostname
        /// </summary>
        public string Hostname
        {
            get
            {
                return ServerDomain.IndexOf(':') != -1 ? ServerDomain.Substring(0, ServerDomain.IndexOf(':')) : ServerDomain;
            }
        }

        /// <summary>
        /// The used protocol. http or https only.
        /// </summary>
        [Column]
        public string Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// An username for the account
        /// </summary>
        [Column]
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// // Password for account
        /// </summary>
        [Column]
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the Username & Password for
        /// the account. This works also in encrypted mode.
        /// </summary>
        /// <returns></returns>
        public NetworkCredential GetCredentials()
        {
            var copy = this.GetCopy();
            if (!copy.IsAnonymous && copy.IsEncrypted) copy.RestoreCredentials();
            return copy.IsAnonymous ? new NetworkCredential() : new NetworkCredential(copy.Username, copy.Password);
        }

        /// <summary>
        /// Returns a uri from the used server domain and protocol without trailing slash.
        /// </summary>
        /// <returns></returns>
        public Uri GetUri()
        {
            return new Uri(Protocol + "://" + ServerDomain.Trim('/'), UriKind.Absolute);
        }

        /// <summary>
        /// Allways return the unencrypted Username
        /// </summary>
        public string DisplayUserName
        {
            get
            {
                // don't call RestoreCredentials() here
                return IsEncrypted ? Utility.DecryptString(Username) : Username;
            }
        }

        /// <summary>
        /// Path to WebDAV-Listening, usually /remote.php/webdav/
        /// </summary>
        [Column]
        public string WebDAVPath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to CalDAV-Listening, usually /remote.php/caldav/
        /// </summary>
        [Column]
        public string CalDAVPath
        {
            get;
            set;
        }



        private EntitySet<TableCalendar> _calendars = new EntitySet<TableCalendar>();
        [Association(OtherKey = "_accountId", ThisKey = "GUID", Storage = "_calendars")]
        public EntitySet<TableCalendar> Calendars
        {
            get
            {
                return _calendars;
            }
            set
            {
                _calendars.Assign(value);
            }
        }


        bool _isAnonymous = false;
        /// <summary>
        /// 
        /// </summary>
        [Column]
        public bool IsAnonymous
        {
            get
            {
                return _isAnonymous;
            }
            set
            {
                _isAnonymous = value;
                OnPropertyChanged("CredentialsVisibility");
            }
        }

        /// <summary>
        /// Returns the visibility state for username and password field
        /// </summary>
        public Visibility CredentialsVisibility
        {
            get
            {
                return IsAnonymous ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Determines if all required settings are set.
        /// If not at least one messagebox is shown to the user.
        /// </summary>
        public bool CanSave()
        {
            bool canSave = true;
            switch (Protocol)
            {
                case "http":
                case "https":
                    // ok
                    break;
                default:
                    MessageBox.Show("Model_Account_Protocol_Unsupported".Translate(Protocol));
                    canSave = false;
                    break;
            }

            if (string.IsNullOrWhiteSpace(ServerDomain))
            {
                MessageBox.Show("Model_Account_ServerDomain_Empty".Translate());
                canSave = false;
            }

            if (!IsAnonymous)
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    MessageBox.Show("Model_Account_Username_Empty".Translate());
                    canSave = false;
                }
            }
            return canSave;
        }

        /// <summary>
        /// Encrypts username and password text
        /// </summary>
        public void StoreCredentials()
        {
            if (!IsEncrypted && !IsAnonymous)
            {
                Username = Utility.EncryptString(Username);
                Password = Utility.EncryptString(Password);
                IsEncrypted = true;
            }
        }

        /// <summary>
        /// Decrypts username and password text
        /// </summary>
        public void RestoreCredentials()
        {
            if (IsEncrypted && !IsAnonymous)
            {
                Username = Utility.DecryptString(Username);
                Password = Utility.DecryptString(Password);
                IsEncrypted = false;
            }
        }

        /// <summary>
        /// Gets a copy.
        /// </summary>
        /// <returns></returns>
        public Account GetCopy()
        {
            return (Account)this.MemberwiseClone();
        }
    }
}
    