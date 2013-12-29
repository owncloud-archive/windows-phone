using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OwnCloud.Data;
using OwnCloud.Extensions;

namespace OwnCloud
{
    public partial class MainPage : PhoneApplicationPage
    {

        public MainPage()
        {
            InitializeComponent();
            DataContext = App.DataContext;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            App.DataContext.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, App.DataContext.Accounts);
            
            // anybody there who knows why the LINQ binding isn't working as expected?
            AccountsList.ItemsSource = null;
            AccountsList.ItemsSource = App.DataContext.Accounts;

            if (App.DataContext.Accounts.Count() == 0)
            {
                //FilesPanoramaItem.Visibility = System.Windows.Visibility.Collapsed;
            }

            // trigger selection
            PanoramaSelectionChanged(MainPanorama, new RoutedEventArgs());

        }

        private void SettingsAccountsTab(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Page/AccountList.xaml", UriKind.Relative));
        }

        private void OpenCalendarTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var currentAccount = ((FrameworkElement) sender).DataContext as Account;

            //Navigate to the calendar page with te userID
            if (currentAccount != null)
                NavigationService.Navigate(new Uri("/View/Page/CalendarMonthPage.xaml?uid=" + String.Format(@"{0:g}", currentAccount.GUID), UriKind.Relative));
        }

        private void RemoteAccountTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var currentAccount = (sender as ListBox).SelectedItem;
            if (currentAccount != null)
            {
                if (_locked)
                {
                    NavigationService.Navigate(new Uri("/View/Page/EditAccount.xaml?mode=edit&account=" + ((Account)currentAccount).GUID, UriKind.Relative));
                }
                else
                {
                    NavigationService.Navigate(new Uri("/View/Page/RemoteFiles.xaml?account=" + ((Account)currentAccount).GUID, UriKind.Relative));
                }
            }
		}

        // the original margin before dragging
        private Thickness originMargin;

        private bool _locked = false;

        // moves an object
        // if the direction is right (positive) it will set the opacity of the dragged object too
        private void AccountsList_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (_locked)
            {
                FrameworkElement control = (FrameworkElement)sender;

                // sets the step each call of ManipulationDelta should have in percent

                control.Margin = new Thickness(control.Margin.Left + e.DeltaManipulation.Translation.X, control.Margin.Top, control.Margin.Right, control.Margin.Bottom);
                control.Opacity = 1.0 - ((control.Margin.Left - originMargin.Left) / control.ActualWidth);
                e.Handled = true;
            }
        }

        private void AccountsList_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            FrameworkElement control = (FrameworkElement)sender;
            originMargin = control.Margin;
        }

        private void AccountsList_ManipulationComplete(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            FrameworkElement control = (FrameworkElement)sender;
            if (control.Opacity > 0)
            {
                // move back the element
                AccountList_ManipulationRestore(control);
            }
            else
            {
                // ask to delete
                Account account = (Account)control.DataContext;
                if (MessageBox.Show("AccountsPage_Confirm_Delete".Translate(account.ServerDomain), "AccountsPage_Confirm_Delete_Caption".Translate(), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    // delete object from db
                    App.DataContext.Accounts.DeleteOnSubmit(account);
                    App.DataContext.SubmitChanges();
                    // "reload"
                    App.DataContext.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, App.DataContext.Accounts);
                    AccountsList.ItemsSource = null;
                    AccountsList.ItemsSource = App.DataContext.Accounts;
                }
                else
                {
                    AccountList_ManipulationRestore(control);
                }
            }
        }

        private void AccountList_ManipulationRestore(FrameworkElement control)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(0.5));
            Storyboard sb = new Storyboard();
            Timeline marginAni = ThicknessAnimation.Create(control, FrameworkElement.MarginProperty, duration, control.Margin, originMargin);
            DoubleAnimation opacityAni = new DoubleAnimation();
            opacityAni.From = control.Opacity;
            opacityAni.To = 1.0;
            opacityAni.Duration = duration;
            Storyboard.SetTargetProperty(opacityAni, new PropertyPath(FrameworkElement.OpacityProperty));
            Storyboard.SetTarget(opacityAni, control);
            sb.Children.Add(marginAni);
            sb.Children.Add(opacityAni);
            sb.Begin();
        }

        private void LockAccountsTap(object sender, EventArgs e)
        {
            _locked = !_locked;
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            button.Text = _locked ? "ApplicationBarButton_Unlock".Translate() : "ApplicationBarButton_Lock".Translate();
            button.IconUri = new Uri(_locked ? "/Assets/Icons/unlock.png" : "/Assets/Icons/lock.png", UriKind.Relative);
        }

        private void CalendarPinToStart(object sender, RoutedEventArgs e)
        {
            var accountID = ((sender as FrameworkElement).DataContext as Account).GUID;

            Extensions.TileHelper.AddCalendarToTile(accountID);
        }

        private void RemoteFilesPinToStart(object sender, RoutedEventArgs e)
        {
            var accountID = ((sender as FrameworkElement).DataContext as Account).GUID;

            Extensions.TileHelper.AddOnlineFilesToTile(accountID);
        }

        private void PanoramaSelectionChanged(object sender, RoutedEventArgs e)
        {
            if(ApplicationBar != null) ApplicationBar.IsVisible = false;

            var panoramaItem = (PanoramaItem)(sender as Panorama).SelectedItem;
            switch (panoramaItem.Name)
            {
                case "AccountPanoramaItem":
                    ApplicationBar = (ApplicationBar)Resources["AccountApplicationBar"];
                    ApplicationBar.IsVisible = true;
                    ApplicationBar.TranslateButtons();
                    break;
                case "FilesPanoramaItem":
                    ApplicationBar = (ApplicationBar)Resources["FilesApplicationBar"];
                    ApplicationBar.IsVisible = true;
                    ApplicationBar.TranslateButtons();
                    break;
            }
        }

        private void AddAccountTap(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Page/EditAccount.xaml", UriKind.Relative));
        }

        private void ChooseAccountTap(object sender, EventArgs e)
        {
            // dynamicly create menu list
            var button = sender as ApplicationBarIconButton;
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.MenuItems.Clear();

            foreach (var acc in App.DataContext.Accounts)
            {
                var item = new ApplicationBarMenuItem(acc.ServerDomain);
                ApplicationBar.MenuItems.Add(item);
            }
        }

        private void PanoramaManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            
        }
    }
}