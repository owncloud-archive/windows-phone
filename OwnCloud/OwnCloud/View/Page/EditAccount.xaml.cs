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

        private void SaveTap(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).UpdateBindingSource();
            Account account = (DataContext as AccountDataContext).CurrentAccount;

            if (!account.CanSave())
            {
                return;
            }

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
                                                MessageBox.Show("EditAccountPage_Connection_Rejected".Translate(state.AssociatedAccount.Hostname, certDetails["CN"], certDetails["ValidAfter"], certDetails["ValidTo"]), "EditAccountPage_Connection_Rejected_Caption".Translate(), MessageBoxButton.OK);
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
            if (MessageBox.Show("EditAccountPage_Confirm_Store".Translate(account.Protocol, account.ServerDomain), "EditAccountPage_Confirm_Store_Caption".Translate(), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
            if(!account.IsAnonymous) account.StoreCredentials();

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
            if (NavigationContext.QueryString.ContainsKey("mode") && NavigationContext.QueryString.ContainsKey("account"))
            {
                _editMode = NavigationContext.QueryString["mode"] == "edit";
                try
                {
                    var accounts = from acc in App.DataContext.Accounts
                                   where acc.GUID == int.Parse(NavigationContext.QueryString["account"])
                                   select acc;
                    var account = accounts.First();

                    if(account.IsEncrypted) account.RestoreCredentials();
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
                if(account.IsEncrypted) account.RestoreCredentials();
                (DataContext as AccountDataContext).CurrentAccount = account;
            }

            // Translate unsupported XAML bindings
            ApplicationBar.TranslateButtons();
        }

        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Account account = (DataContext as AccountDataContext).CurrentAccount;
            // Some Web DAV Test
            var dav_request = DAVRequestHeader.CreateListing();
            dav_request.Headers[Header.Depth] = HeaderAttribute.MethodDepth.ApplyInfinityNoRoot;
            var dav = new WebDAV(new Uri(account.Protocol+"://"+account.ServerDomain + account.WebDAVPath), new NetworkCredential(account.Username, account.Password));
            dav.StartRequest(dav_request, DAVRequestBody.CreateAllPropertiesListing(), null, DAVResult);
        }

        private void DAVResult(DAVRequestResult result, object userObj)
        {
            if (result.Request.ErrorOccured) return;

            Utility.DebugXML(result.GetRawResponse());
            foreach (DAVRequestResult.Item item in result.Items)
            {
                Utility.Debug(String.Format("Name={0:g}, Reference={1:g}, Local={8:g}, Last Modfied={2:g}, Status={3:g}, Quota={4:g}/{5:g}, ETag={6:g}, Type={7:g}", 
                    item.DisplayName,
                    item.Reference,
                    item.LastModified,
                    item.StatusText,
                    item.QuotaUsed,
                    item.QuotaAvailable,
                    item.ETag,
                    item.ResourceType,
                    item.LocalReference
                ));
            }
        }

        

    }
}