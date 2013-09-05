using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using OwnCloud.Data;
using OwnCloud.Data.DAV;
using OwnCloud.Extensions;
using OwnCloud.View.Controls;

namespace OwnCloud.View.Page
{
    public partial class RemoteFiles : PhoneApplicationPage
    {
        public RemoteFiles()
        {
            InitializeComponent();
            _context = new FileListDataContext();
            DataContext = _context;
            // Translate unsupported XAML bindings
            // ApplicationBar.TranslateButtons();
        }

        private Account _workingAccount;
        private FileListDataContext _context;

        private void ToggleTray()
        {
            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.SetIsVisible(this, !SystemTray.IsVisible);
            });
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = App.DataContext.EnablePhoneStatusBar;
            try
            {
                _workingAccount = App.DataContext.LoadAccount(NavigationContext.QueryString["account"]);
                _workingAccount.RestoreCredentials();
            }
            catch (Exception)
            {
                // should not happen
            }

            FetchStructure(_workingAccount.WebDAVPath);
        }

        private string _directoryUpReference = "";

        // 
        private void EnableSyncTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            (sender as File).EnableSync = !(sender as File).EnableSync;
        }

        private void FileListTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (sender.GetType() == typeof(ListBox))
            {
                var item = (File)((sender as ListBox).SelectedItem);
                if (item.IsDirectory)
                {
                    // change directory
                    FetchStructure(item.FilePath);
                }
                else
                {
                    // open file in browser
                    var task = new WebBrowserTask();
                    task.Uri = new Uri(_workingAccount.GetUri() + item.FilePath);
                    task.Show();

                }
            }
        }

        private void DirectoryUpTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_directoryUpReference != null)
            {
                FetchStructure(_directoryUpReference);
            }
        }

        private void ToggleDirectoryUpStatus(bool mode)
        {
            if (mode)
            {
                DirectoryUpDetail.Opacity = 1;
            }
            else
            {
                DirectoryUpDetail.Opacity = 0.5;
            }
        }

        private EventHandler _dropFileListFadeOutCompleted;
        private EventCollector _collector = new EventCollector();
        private Storyboard _board;
        private DAVRequestResult _result;
        private ProgressOverlayPopup _overlay;

        /// <summary>
        /// Tries to fetch a given path and refreshes the views.
        /// </summary>
        /// <param name="path"></param>
        private void FetchStructure(string path)
        {
            _board = (Storyboard)Resources["DropFileListFadeOut"] as Storyboard;

            if (_dropFileListFadeOutCompleted == null)
            {
                _dropFileListFadeOutCompleted = new EventHandler(delegate
                {
                    _context.Files.Clear();
                    _collector.Raise(_dropFileListFadeOutCompleted);
                });
                _board.Completed += _dropFileListFadeOutCompleted;
            }

            if (_overlay == null)
            {
                _overlay = new ProgressOverlayPopup()
                {
                    BackgroundColor = Colors.Transparent
                };
                
            }
            _overlay.Show();

            _result = null;
            _board.Begin();
            _collector.WaitFor(_dropFileListFadeOutCompleted);
            _collector.WaitFor("FileListReceived");

            _collector.Complete = () =>
            {
                FetchStructureCompleteHandler(_result);
                Dispatcher.BeginInvoke(() =>
                {
                    ((Storyboard)Resources["DropFileListFadeIn"] as Storyboard).Begin();
                    _overlay.Hide();
                });
            };

            if (_workingAccount != null)
            {
                var dav = new WebDAV(_workingAccount.GetUri(), _workingAccount.GetCredentials());
                dav.StartRequest(DAVRequestHeader.CreateListing(path), DAVRequestBody.CreateAllPropertiesListing(), null, FetchStructureComplete);
            }
        }


        private void FetchStructureComplete(DAVRequestResult result, object userObj)
        {
            _result = result;
            _collector.Raise("FileListReceived");
        }

        private void FetchStructureCompleteHandler(DAVRequestResult result)
        {
            if (result.Status == ServerStatus.MultiStatus && !result.Request.ErrorOccured && result.Items.Count > 0)
            {
                //Utility.DebugXML(result.GetRawResponse());
                var first_item = false;

                foreach (DAVRequestResult.Item item in result.Items)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        // a first element usually is the root folder
                        // and the name is empty
                        if (!first_item)
                        {
                            first_item = true;

                            if (item.Reference == _workingAccount.WebDAVPath)
                            {
                                // cannot go up further
                                ToggleDirectoryUpStatus(false);
                                _directoryUpReference = null;
                                CurrentDirectoryName.Text = "";
                            }
                            else
                            {
                                ToggleDirectoryUpStatus(true);
                                _directoryUpReference = item.ParentReference;
                                CurrentDirectoryName.Text = item.LocalReference;
                            }
                        }
                        else
                        {
                            _context.Files.Add(new File
                            {
                                FileName = item.LocalReference,
                                FilePath = item.Reference,
                                FileSize = item.ContentLength,
                                FileType = item.ContentType,
                                FileCreated = item.CreationDate,
                                FileLastModified = item.LastModified,
                                IsDirectory = item.ResourceType == ResourceType.Collection
                            });
                        }
                    });
                }
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (result.Status == ServerStatus.Unauthorized)
                    {
                        MessageBox.Show("FetchFile_Unauthorized".Translate(), "Error_Caption".Translate(), MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show("FetchFile_Unexpected_Result".Translate(), "Error_Caption".Translate(), MessageBoxButton.OK);
                    }
                });
            }

        }
    }
}