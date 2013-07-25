using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using OwnCloud;
using OwnCloud.Data;

namespace Ocwp.Controls
{
    public partial class CalendarDayControl : UserControl
    {
        private DateTime _targetDate;

        public CalendarDayControl()
        {
            InitializeComponent();
        }

        public DateTime TargetDate
        {
            get { return _targetDate; }
            set { _targetDate = value.Date; }
        }
        
        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            var context = new OwnCloudDataContext();

            var events = context.Events.Where(o => o.To > _targetDate && o.From < _targetDate.AddDays(1)).ToArray();
            context.Dispose();

            if (events.Length == 0) return;

            CmMenu.Items.Clear();

            foreach (var tableEvent in events)
            {
                var item = new MenuItem {Header = tableEvent.Title, DataContext = tableEvent};
                item.Click += EventClick;

                CmMenu.Items.Add(item);
            }

        }

        void EventClick(object sender, RoutedEventArgs e)
        {
            var dbEvent = (sender as MenuItem).DataContext as TableEvent;

            //TODO: Import Edit Page..
            App.Current.RootFrame.Navigate(new Uri("/Pages/AppointmentPage.xaml?url=" + dbEvent.Url, UriKind.Relative));
        }


        #region Styling Functions

        private void BeginOverStyle()
        {
            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
        }

        private void EndOverStyle()
        {
            this.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void CalendarDayControl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BeginOverStyle();
        }

        private void CalendarDayControl_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndOverStyle();
        }

        private void CalendarDayControl_OnMouseLeave(object sender, MouseEventArgs e)
        {
            EndOverStyle();
        }

        private void CalendarDayControl_OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            EndOverStyle();
        }

        #endregion

        
    }
}