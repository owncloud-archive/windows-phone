using System;
using System.ComponentModel;
using System.Windows;

namespace OwnCloud.Model
{
    public class Account : INotifyPropertyChanged
    {
        public Account()
        {
            WebDAVPath = "/remote.php/webdav/";
            CalDAVPath = "/remote.php/caldav/";
        }

        string _domain = "example.com";
        /// <summary>
        /// Server Domain to connect to
        /// </summary>
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
                    MessageBox.Show(LocalizedStrings.Get("Model_Account_ServerDomain_Empty"));
                }
                else
                {
                    _domain = value;
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
                        break;
                    default:
                       MessageBox.Show(String.Format(LocalizedStrings.Get("Model_Account_Protocol_Unsupported"), value));
                        break;
                }
            }
        }

        string _username;
        /// <summary>
        /// An username for the account
        /// </summary>
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
                    MessageBox.Show(LocalizedStrings.Get("Model_Account_Username_Empty"));
                }
                else
                {
                    _username = value;
                }
            }
        }

        string _password;
        /// <summary>
        /// // Password for account
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        /// <summary>
        /// Path to WebDAV-Listening, usually /remote.php/webdav/
        /// </summary>
        public string WebDAVPath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to CalDAV-Listening, usually /remote.php/caldav/
        /// </summary>
        public string CalDAVPath
        {
            get;
            set;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsAnonymous
        {
            get;
            set;
        }

        /// <summary>
        /// The object GUID
        /// </summary>
        public string GUID
        {
            get;
            set;
        }

        /// <summary>
        /// Encrypts username and password text
        /// </summary>
        public void StoreCredentials()
        {
            _username = Utility.EncodeString(_username);
            _password = Utility.EncodeString(_password);
        }

        /// <summary>
        /// Decrypts username and password text
        /// </summary>
        public void RestoreCredentials()
        {
            try
            {
                _username = Utility.DecodeString(_username);
                _password = Utility.DecodeString(_password);
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
    