using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using OwnCloud.Model;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace OwnCloud
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DispatcherTimer _deviceStatusTimer;
        private IsolatedStorageFile _isf;

        public MainPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
                _isf = IsolatedStorageFile.GetUserStoreForApplication();
                App.ViewModel.LocalStorageFreeBytes = _isf.AvailableFreeSpace;

                // Install a device status timer
                
                _deviceStatusTimer = new DispatcherTimer();
                _deviceStatusTimer.Interval = new TimeSpan(0, 0, 2);
                _deviceStatusTimer.Tick += delegate
                {
                    App.ViewModel.LocalStorageFreeBytes = _isf.AvailableFreeSpace;
                };
                _deviceStatusTimer.Start();
            }
        }

        private void SettingsAccountsTab(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Page/Accounts.xaml", UriKind.Relative));
        }

        private void OpenCalendarTap(object sender, GestureEventArgs e)
        {
            var currentAccount = ((FrameworkElement) sender).DataContext as Account;

            //Navigate to the calendar page with te userID
            if (currentAccount != null)
                NavigationService.Navigate(new Uri("/View/Page/CalendarMonthPage.xaml?uid=" + String.Format(@"{0:g}", currentAccount.GUID), UriKind.Relative));
        }
    }
}