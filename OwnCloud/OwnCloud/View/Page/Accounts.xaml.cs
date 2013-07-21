using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using OwnCloud.Model;

namespace OwnCloud
{
    public partial class AccountsPage : PhoneApplicationPage
    {
        public AccountsPage()
        {
                InitializeComponent();
        }

        private void AddAccountTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Page/EditAccount.xaml", UriKind.Relative));
            
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            AccountsList.ItemsSource = new AccountListDataContext().Accounts;
        }

        private void AccountListTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListBox box = (ListBox)sender;
            NavigationService.Navigate(new Uri(String.Format("/View/Page/EditAccount.xaml?account={0:g}&mode=edit", ((Account)box.SelectedItem).GUID), UriKind.Relative));
        }


        // the original margin before dragging
        private Thickness originMargin;

        // moves an object
        // if the direction is right (positive) it will set the opacity of the dragged object too
        private void AccountsList_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            FrameworkElement control = (FrameworkElement)sender;

            // sets the step each call of ManipulationDelta should have in percent

            control.Margin = new Thickness(control.Margin.Left + e.DeltaManipulation.Translation.X, control.Margin.Top, control.Margin.Right, control.Margin.Bottom);
            control.Opacity = 1.0 - ((control.Margin.Left - originMargin.Left) / control.ActualWidth);
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
                if (MessageBox.Show(string.Format(LocalizedStrings.Get("AccountsPage_Confirm_Delete"), account.ServerDomain), LocalizedStrings.Get("AccountsPage_Confirm_Delete_Caption"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    // delete object from list
                    Serialize.RemoveFile(@"Accounts\" + account.GUID);
                    // "reload"
                    AccountsList.ItemsSource = new AccountListDataContext().Accounts;
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

    }
}