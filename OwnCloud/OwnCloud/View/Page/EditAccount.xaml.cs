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
using System.Xml.Serialization;
using System.Windows.Media.Animation;
using OwnCloud.Data;
using OwnCloud.Model;
using OwnCloud.Net;

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
        }

        EventHandler overlayFadeOut_Completed;
        EventHandler overlayFadeIn_Completed;
        Storyboard overlayFadeIn;
        Storyboard overlayFadeOut;

        private void SaveTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Account form = (DataContext as AccountDataContext).CurrentAccount;

            // show overlay
            this.Overlay.Visibility = System.Windows.Visibility.Visible;
            overlayFadeIn = this.Resources["TestConnectionOverlayFadeIn"] as Storyboard;
            overlayFadeOut = this.Resources["TestConnectionOverlayFadeOut"] as Storyboard;


            // aware memory restitant event handler
            if (overlayFadeOut_Completed == null)
            {
                overlayFadeOut_Completed = new EventHandler(delegate
                {
                    this.OverlayProgressBar.IsIndeterminate = false;
                    this.Overlay.Visibility = System.Windows.Visibility.Collapsed;
                });
                overlayFadeOut.Completed += overlayFadeOut_Completed;
            }

            if (overlayFadeIn_Completed == null)
            {
                overlayFadeIn_Completed = new EventHandler(OverlayFadeIn);
                overlayFadeIn.Completed += overlayFadeIn_Completed;
            }

            // start overlay
            overlayFadeIn.Begin();
        }

        private void OverlayFadeIn(object obj, EventArgs e)
        {
            // Test Connection
            Account account = (DataContext as AccountDataContext).CurrentAccount;
            this.OverlayProgressBar.IsIndeterminate = true;
            HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0:g}://{1:g}", account.Protocol, account.ServerDomain));
            request.BeginGetResponse(new AsyncCallback(OnHTTPResponse), new AsyncHttpResponse(request, account));
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
                        socket.Connect(state.AssociatedAccount.Hostname, TLSHandshake.TLS_HTTP_PORT);
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
                                                MessageBox.Show(String.Format(LocalizedStrings.Get("EditAccountPage_Connection_Rejected"), state.AssociatedAccount.Hostname, certDetails["CN"], certDetails["ValidAfter"], certDetails["ValidTo"]), LocalizedStrings.Get("EditAccountPage_Connection_Rejected_Caption"), MessageBoxButton.OK);
                                                overlayFadeOut.Begin();
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

            Dispatcher.BeginInvoke(() =>
            {
                if (success)
                {
                    overlayFadeOut.Begin();
                    StoreAccount(state.AssociatedAccount);
                }
                else
                {
                    overlayFadeOut.Begin();
                    OnConnectFailed(state.AssociatedAccount);
                }
            });

        }

        private void OnConnectFailed(Account account)
        {
            if (MessageBox.Show(String.Format(LocalizedStrings.Get("EditAccountPage_Confirm_Store"), account.Protocol, account.ServerDomain), LocalizedStrings.Get("EditAccountPage_Confirm_Store_Caption"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                overlayFadeOut.Begin();
                StoreAccount(account);
            }
            else
            {
                overlayFadeOut.Begin();
            }
        }

        private void StoreAccount(Account account)
        {
            // encrypt data
            account.StoreCredentials();
            // Serialize to disk
            if (_editMode)
            {
                Serialize.WriteFile(String.Format(@"Accounts\{0:g}", account.GUID), account);
            }
            else
            {
                account.GUID = Guid.NewGuid().ToString("N");
                Serialize.WriteFile(String.Format(@"Accounts\{0:g}", account.GUID), account);
            }
            // temporary data can be deleted
            _accountForm.Remove(_editMode ? "EditAccountForm" : "AddAccountForm");
            NavigationService.GoBack();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("mode") && NavigationContext.QueryString.ContainsKey("account"))
            {
                _editMode = NavigationContext.QueryString["mode"] == "edit";
                try
                {
                    Account account = (Account)Serialize.ReadFile(String.Format(@"Accounts\{0:g}", NavigationContext.QueryString["account"]), typeof(Account));
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

            (DataContext as AccountDataContext).PageMode = _editMode ? LocalizedStrings.Get("EditAccountPage_EditAccount") : LocalizedStrings.Get("EditAccountPage_AddAccount");

            // If there are any stored form details
            // include here if DataContext is empty
            if ((DataContext as AccountDataContext).CurrentAccount == null)
            {
                Account account = (Account)_accountForm.Restore(_editMode ? "EditAccountForm" : "AddAccountForm", new Account());
                if(account.GUID != null) account.RestoreCredentials();
                (DataContext as AccountDataContext).CurrentAccount = account;
            }
        }
    }
}