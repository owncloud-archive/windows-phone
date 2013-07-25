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


        /// <summary>
        /// Try to load a account from a Guid
        /// </summary>
        /// <returns></returns>
        public static Account LoadFromGuid(string name)
        {
            
            return null;
        }

        public bool IsEncrypted { get; set; }

        string _domain = "example.com";
        /// <summary>
        /// Server Domain to connect to
        /// </summary>
        ///
        [Column]
        public string ServerDomain
        {
            get
            {
                return _domain;
            }
            set
            {
                if (value.ToString() == String.Empty)
                {
                    MessageBox.Show("Model_Account_ServerDomain_Empty".Translate());
                }
                else
                {
                    _domain = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Only returns the hostname
        /// </summary>
        public string Hostname
        {
            get
            {
                return _domain.IndexOf(':') != -1 ? _domain.Substring(0, _domain.IndexOf(':')) : _domain;
            }
        }

        string _protocol = "https";
        /// <summary>
        /// The used protocol. http or https only.
        /// </summary>
        [Column]
        public string Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                switch (value)
                {
                    case "https":
                    case "http":
                        _protocol = value;
                        NotifyPropertyChanged();
                        break;
                    default:
                       MessageBox.Show("Model_Account_Protocol_Unsupported".Translate(value));
                        break;
                }
            }
        }

        string _username;
        /// <summary>
        /// An username for the account
        /// </summary>
        [Column]
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (!IsAnonymous && String.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show("Model_Account_Username_Empty".Translate());
                }
                else
                {
                    _username = value;
                    NotifyPropertyChanged();
                }
            }
        }

        string _password;
        /// <summary>
        /// // Password for account
        /// </summary>
        [Column]
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Allways return the unencrypted Username
        /// </summary>
        public string DisplayUserName
        {
            get
            {
                bool wasEncr = IsEncrypted;
                if(IsEncrypted)
                    RestoreCredentials();
                string value = Username;
                if(wasEncr)
                    StoreCredentials();
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
        
        /// <summary>
        /// 
        /// </summary>
        [Column]
        public bool IsAnonymous
        {
            get;
            set;
        }

        /// <summary>
        /// Encrypts username and password text
        /// </summary>
        public void StoreCredentials()
        {
            _username = Utility.EncryptString(_username);
            _password = Utility.EncryptString(_password);
            IsEncrypted = true;
        }

        /// <summary>
        /// Decrypts username and password text
        /// </summary>
        public void RestoreCredentials()
        {
            try
            {
                _username = Utility.DecryptString(_username);
                _password = Utility.DecryptString(_password);
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
    