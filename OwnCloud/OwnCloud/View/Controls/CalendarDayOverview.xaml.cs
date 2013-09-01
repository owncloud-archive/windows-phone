using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OwnCloud.Extensions;
using OwnCloud.Data;

namespace OwnCloud.View.Controls
{
    public partial class CalendarDayOverview
    {
        public CalendarDayOverview()
        {
            InitializeComponent();
        }

        public DynamicCalendarSource EventSource
        {
            get; set;
        }


        private void CalendarDayOverview_OnLoaded(object sender, RoutedEventArgs e)
        {
            BackgroundGrid.SetGridRows(24);
            AppointmentGrid.SetGridRows(24);
            
            //Add horizontal lines
            for (int i = 0; i < 24; i++)
            {
                var r = new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.White)
                        , VerticalAlignment = VerticalAlignment.Bottom
                        , Height = 1
                    };
                Grid.SetRow(r,i);
                Grid.SetColumnSpan(r, 2);
                BackgroundGrid.Children.Add(r);
            }

            UpdateEvents();

        }

        private void UpdateEvents()
        {
            var events =
                EventSource.LoadEvents(this)
                           .Where(o => o.To > SelectedDate)
                           .Where(o => o.From < SelectedDate.AddDays(1))
                           .OrderByDescending(o => o.From - o.To).ToArray();

            PutEvents(events);
        }


        public void PutEvents(IEnumerable<TableEvent> events)
        {
            foreach (var currentEvent in events)
            {
                int startRow = 0;
                if (currentEvent.From.Date == SelectedDate)
                {
                    startRow = currentEvent.From.Hour;
                }

                int endRow = 23;
                if (currentEvent.To.Date == SelectedDate)
                {
                    endRow = Math.Max(currentEvent.To.Hour,1);
                }

                var appointmentControl = new CalendarDayOverviewAppointment();
                Grid.SetRow(appointmentControl,startRow);
                Grid.SetRowSpan(appointmentControl,endRow - startRow + 1);
                appointmentControl.DataContext = currentEvent;
                AppointmentGrid.Children.Add(appointmentControl);

            }
        }


        public DateTime SelectedDate
        {
            get { return ((DateTime)DataContext).Date; }
        }

    }
}