using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Media.Animation;
using OwnCloud.Data;
using OwnCloud.Data.DAV;
using OwnCloud.Net;
using OwnCloud.Extensions;
using OwnCloud.View.Controls;

namespace OwnCloud
{
    public partial class EditAccount : PhoneApplicationPage
    {

        private TemporaryData _accountForm = new TemporaryData();
        private bool _editMode = false;

        public struct AsyncHttpResponse
        {
            public HttpWebRequest Request;
            public Account AssociatedAccount;

            public AsyncHttpResponse(HttpWebRequest request, Account account)
            {
                Request = request;
                AssociatedAccount = account;
            }
        }

        public EditAccount()
        {
            DataContext = new AccountDataContext();
            InitializeComponent();
            // NavigationContext is not available in constructor
            this.Loaded += new RoutedEventHandler(PageLoaded);
            // Translate unsupported XAML bindings
            ApplicationBar.TranslateButtons();
        }

        private void ProtocolButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var button = sender as Button;
            button.Content = button.Content.ToString() == "http" ? "https" : "http";
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            // Save form details if the user has switched
            // and want to come back
            // exclude Password
            Account form = (DataContext as AccountDataContext).CurrentAccount.GetCopy();
            form.Password = "";
            _accountForm.Store(_editMode ? "EditAccountForm" : "AddAccountForm", form);

            // Clear data context
            DataContext = null;
        }

        ProgressOverlayPopup _overlay;

        private void SaveTap(object sender, EventArgs e)
        {
            // this works but the on blur event is called by leaving the page
            // and will manipulate the current object so locking is required for
            // credentials
            (sender as ApplicationBarIconButton).UpdateBindingSource();

            Account account = (DataContext as AccountDataContext).CurrentAccount;

            if (!account.CanSave())
            {
                return;
            }

            // show overlay
            if (_overlay == null)
            {
                _overlay = new ProgressOverlayPopup("EditAccountPage_CheckingConnection".Translate());
                _overlay.ShowCompleted += new EventHandler(OverlayFadeIn);
            }
            _overlay.Show();
        }

        private void OverlayFadeIn(object obj, EventArgs e)
        {
            // Test Connection
            Account account = (DataContext as AccountDataContext).CurrentAccount;
            //this.Overlay.PerformanceBar.IsIndeterminate = true;
            
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0:g}://{1:g}", account.Protocol, account.ServerDomain));
                request.BeginGetResponse(new AsyncCallback(OnHTTPResponse), new AsyncHttpResponse(request, account));
            }
            catch (Exception)
            {
                // uri malformed
                OnConnectFailed(account);
            }
            
        }

        private void OnHTTPResponse(IAsyncResult result)
        {
            AsyncHttpResponse state = (AsyncHttpResponse)result.AsyncState;
            var success = false;

            try
            {
                state.Request.EndGetResponse(result);
                success = true;
            }
            catch (WebException)
            {
                if (state.AssociatedAccount.Protocol == "https")
                {
                    // try to fetch certificate
                    try
                    {
                        var tls = new TLSHandshake();
                        tls.SetServerNameExtension(state.AssociatedAccount.Hostname);
                        var socket = new StreamSocket();
                        socket.Connect(state.AssociatedAccount.Hostname, state.AssociatedAccount.GetPort(true));
                        socket.Write(tls.CreateClientHello());

                        DateTime startTime = DateTime.Now;
                        while (true)
                        {
                            var data = socket.Read();
                            if (data.Length > 0)
                            {
                                var cert = tls.FindPacket(data, TLSHandshake.TLS_HANDSHAKE_CERTIFICATE);
                                if (cert.Length > 0)
                                {
                                    var details = tls.GetCertificateDetails(cert);
                                    if (details.Count > 0)
                                    {
                                        var certDetails = details[0];
                                        if (certDetails.ContainsKey("CN"))
                                        {
                                            Dispatcher.BeginInvoke(() =>
                                            {
                                                MessageBox.Show("EditAccountPage_Connection_Rejected".Translate(state.AssociatedAccount.Hostname, certDetails["CN"], certDetails["ValidAfter"], certDetails["ValidTo"]), "EditAccountPage_Connection_Rejected_Caption".Translate(), MessageBoxButton.OK);
                                                _overlay.Hide();
                                            });
                                            return;
                                        }
                                    }
                                    break;
                                }
                            }

                            if (DateTime.Now.Subtract(startTime).TotalSeconds > 5)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Host not reachable, no SSL host or TLS version not supported
                    }
                }
            }
            catch (Exception)
            {
                // HTTPWebRequest has failed
            }

            if (success)
            {
                // Testing DAV
                //TODO: Add your additional connection test statement here
                // To complete the test all fragments must have been fired.
                EventCollector collector = new EventCollector()
                {
                    Complete = () =>
                    {
                        OnConnectTestComplete(success, state.AssociatedAccount);
                    }
                };
                collector.WaitFor(state.AssociatedAccount.WebDAVPath);
                collector.WaitFor(state.AssociatedAccount.CalDAVPath);

                // define paths to test
                Queue<string> pathsToTest = new Queue<string>();
                pathsToTest.Enqueue(state.AssociatedAccount.WebDAVPath);
                pathsToTest.Enqueue(state.AssociatedAccount.CalDAVPath);

                // create master instance
                WebDAV davTest = new WebDAV(state.AssociatedAccount.GetUri(), state.AssociatedAccount.GetCredentials());

                // call tests
                while (pathsToTest.Count > 0)
                {
                    var path = pathsToTest.Dequeue();
                    davTest.StartRequest(DAVRequestHeader.CreateListing(path), path, (requestResult, userObj) =>
                    {
                        if (requestResult.Status != ServerStatus.MultiStatus)
                        {
                            // all other states are fail states
                            success = false;
                            Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("EditAccountPage_CheckingConnection_DAVTestFailed".Translate(userObj, requestResult.StatusText), "Error_Caption".Translate(), MessageBoxButton.OK);
                            });
                        }
                        collector.Raise(userObj);
                    });
                }                
            }
            else
            {
                OnConnectTestComplete(success, state.AssociatedAccount);
            }
            
        }


        private void OnConnectTestComplete(bool success, Account account)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _overlay.Hide();
                if (success)
                {
                    StoreAccount(account);
                }
                else
                {
                    OnConnectFailed(account);
                }
            });
            
        }

        private void OnConnectFailed(Account account)
        {
            if (MessageBox.Show("EditAccountPage_Confirm_Store".Translate(account.Protocol, account.ServerDomain), "EditAccountPage_Confirm_Store_Caption".Translate(), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                StoreAccount(account);
            }
        }

        private void StoreAccount(Account account)
        {

            // encrypt data
            account.StoreCredentials();

            // edit/insert
            if (!_editMode) {
                App.DataContext.Accounts.InsertOnSubmit(account);  
            }
            App.DataContext.SubmitChanges();

            // temporary data can be deleted
            _accountForm.Remove(_editMode ? "EditAccountForm" : "AddAccountForm");
            NavigationService.GoBack();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = App.DataContext.EnablePhoneStatusBar;
            if (NavigationContext.QueryString.ContainsKey("mode") && NavigationContext.QueryString.ContainsKey("account"))
            {
                _editMode = NavigationContext.QueryString["mode"] == "edit";
                try
                {
                    var account = App.DataContext.LoadAccount(NavigationContext.QueryString["account"]);
                    account.RestoreCredentials();
                    (DataContext as AccountDataContext).CurrentAccount = account;
                }
                catch (Exception)
                {
                    // should not happen
                }
            }
            else
            {
                _editMode = false;
            }

            (DataContext as AccountDataContext).PageMode = _editMode ? "EditAccountPage_EditAccount".Translate() : "EditAccountPage_AddAccount".Translate();

            // If there are any stored form details
            // include here if DataContext is empty
            if ((DataContext as AccountDataContext).CurrentAccount == null)
            {
                Account account = (Account)_accountForm.Restore(_editMode ? "EditAccountForm" : "AddAccountForm", new Account());
                account.RestoreCredentials();
                (DataContext as AccountDataContext).CurrentAccount = account;
            }
        }

    }
}