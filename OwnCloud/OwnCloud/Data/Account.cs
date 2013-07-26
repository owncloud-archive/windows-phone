using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;
using System.Data.Linq.Mapping;
using OwnCloud.Extensions;

namespace OwnCloud.Data
{
    [Table(Name="Accounts")]
    public class Account : Entity
    {
        public Account()
        {
            WebDAVPath = "/remote.php/webdav/";
            CalDAVPath = "/remote.php/caldav/";
            Protocol = "https";
            ServerDomain = "example.com";
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

        public bool IsEncrypted { get; set; }

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
        /// Allways return the unencrypted Username
        /// </summary>
        public string DisplayUserName
        {
            get
            {
                bool wasEncr = IsEncrypted;
                if(IsEncrypted) RestoreCredentials();
                string value = Username;
                if(wasEncr) StoreCredentials();
                return value;
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
            Username = Utility.EncryptString(Username);
            Password = Utility.EncryptString(Password);
            IsEncrypted = true;
        }

        /// <summary>
        /// Decrypts username and password text
        /// </summary>
        public void RestoreCredentials()
        {
            try
            {
                Username = Utility.DecryptString(Username);
                Password = Utility.DecryptString(Password);
                IsEncrypted = false;
            }
            catch (Exception cryptEx)
            {
                Utility.Debug("RestoreCredentials Exception: " + cryptEx.Message);
            }
        }

        public Account GetCopy()
        {
            return (Account)this.MemberwiseClone();
        }
    }
}
    