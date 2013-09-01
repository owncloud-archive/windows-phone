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
using Microsoft.Phone.Controls;
using OwnCloud.Data;
using System.Windows.Navigation;
using OwnCloud.Resource.Localization;

namespace OwnCloud.View.Page
{
    public partial class CalendarDayPage : PhoneApplicationPage
    {
        public CalendarDayPage()
        {
            InitializeComponent();

            

        }

        private int _userId;
        private ScrollViewer _dayScoller;
        private DateTime _startDate;


        public new CalendarDaysDataContext DataContext {
            get { return base.DataContext as CalendarDaysDataContext; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get userid in query
            if (NavigationContext.QueryString.ContainsKey("uid"))
                _userId = int.Parse(NavigationContext.QueryString["uid"]);
            else throw new ArgumentNullException("uid", AppResources.Exception_NoUserID);

            try
            {
                _startDate = DateTime.Parse(NavigationContext.QueryString["startDate"]);
            }
            catch
            {
                _startDate = DateTime.Now;
            }

            base.DataContext = new CalendarDaysDataContext(_startDate);

            base.OnNavigatedTo(e);
        }

        private void LongListSelector_OnLink(object sender, LinkUnlinkEventArgs e)
        {
            DataContext.ItemLinked(sender, e);
        }

        private void LayoutRoot_OnLoaded(object sender, RoutedEventArgs e)
        {
            LlsDays.ScrollTo(_startDate);
        }

        private void HookScrollViewer(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
           _dayScoller = FindChildOfType<ScrollViewer>(element);
           _dayScoller.LayoutUpdated += _dayScoller_LayoutUpdated;
        }

        void _dayScoller_LayoutUpdated(object sender, EventArgs e)
        {
            if (_dayScoller.VerticalOffset < 1)
            {
                DataContext.AddOnTop();
                LlsDays.ScrollTo(DataContext.Days[1]);
                _dayScoller.ScrollToVerticalOffset(2);
            }
        }

        public static T FindChildOfType<T>(DependencyObject root) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }

        private void DynamicCalendarSource_OnOnEventsRequested(object sender, DynamicCalendarSource.LoadEventResult e)
        {
            var validCalendars = App.DataContext.Calendars.Where(o => o._accountId == _userId).Select(o => o.Id).ToArray();
            e.Result = App.DataContext.Events.Where(o => validCalendars.Contains(o.CalendarId));
        }
    }
}