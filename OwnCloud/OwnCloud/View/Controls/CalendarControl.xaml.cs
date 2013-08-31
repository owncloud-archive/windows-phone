using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Ocwp.Controls;
using OwnCloud.Data;
using OwnCloud.Extensions;

namespace OwnCloud.View.Controls
{
    public partial class CalendarControl
    {
        public CalendarControl()
        {
            InitializeComponent();
            
            Unloaded += CalendarControl_Unloaded;
        }

        void CalendarControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_context != null)
            {
                Context.Dispose();
                _context = null;
            }
        }


        #region private Fields

        private int _weekCount = 0;
        private Data.OwnCloudDataContext _context;
        private Data.OwnCloudDataContext Context
        {
            get { return _context ?? (_context = new OwnCloudDataContext()); }
        }
        private DateTime _firstDayOfCalendarMonth;
        private DateTime _lastDayOfCalendarMonth;
        private Dictionary<int, StackPanel> _dayPanels = new Dictionary<int, StackPanel>(); 

        #endregion

        #region Public Properties

        private int? _accountID;
        public int? AccountID
        {
            get { return _accountID; }
            set 
            {
                if (_accountID == null)
                {
                    _accountID = value;
                }
                else
                    _accountID = value; 
            }
        }


        public DateTime SelectedDate
        {
            get { return (DateTime)GetValue(SelectedDateProperty); }
            set
            {
                SetValue(SelectedDateProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SelectedDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime), typeof(CalendarControl), new PropertyMetadata(DateTime.MinValue, OnSelectedDateChaged));

        private static void OnSelectedDateChaged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CalendarControl).SelectedDateChanged(e);
        }

        private void SelectedDateChanged(DependencyPropertyChangedEventArgs e)
        {
            OnDateChanging();

            if ((DateTime)e.NewValue > (DateTime)e.OldValue)
                SlideTopBegin.Begin();
            else SlideBottomBegin.Begin();
        }

        

        #endregion

        #region private Functions

        private void SlideTopBegin_OnCompleted(object sender, EventArgs e)
        {
            ChangeDate();
            SlideTopEnd.Begin();
        }

        private void SlideBottomBegin_OnCompleted(object sender, EventArgs e)
        {
            ChangeDate();
            SlideBottomEnd.Begin();
        }

        public void ChangeDate()
        {
            OnDateChanged();

            _firstDayOfCalendarMonth = SelectedDate.FirstOfMonth().FirstDayOfWeek().Date;
            _lastDayOfCalendarMonth = SelectedDate.LastOfMonth().LastDayOfWeek().AddDays(1);

            _weekCount = SelectedDate.GetMonthCount();
            ResetGridLines();

            if (_accountID == null)
                return;

            RefreshAppointments();
        }

        private void ResetGridLines()
        {
            GrdCalendarLines.Children.Clear();
            GrdCalendarLines.SetGridRows(_weekCount + 1);
            GrdCalendarLines.SetGridColumns(7);

            GrdDayIndicator.Children.Clear();
            GrdDayIndicator.SetGridRows(_weekCount + 1);
            GrdDayIndicator.SetGridColumns(7);

            GrdAppointments.Children.Clear();
            GrdAppointments.SetGridRows(_weekCount + 1);
            GrdAppointments.SetGridColumns(7);

            var firstDay = SelectedDate.FirstOfMonth().FirstDayOfWeek();
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < _weekCount; j++)
                {
                    DateTime fieldDate = firstDay.AddDays((j * 7) + i);

                    Color dayIndicatorColor = Colors.White;
                    if (fieldDate.Date == DateTime.Now.Date)
                        dayIndicatorColor = (Color)Application.Current.Resources["PhoneAccentColor"];
                    else if (fieldDate.Month != SelectedDate.Month)
                        dayIndicatorColor = Colors.Gray;

                    var dayIndicator = new TextBlock
                        {
                            

                            VerticalAlignment = VerticalAlignment.Bottom,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Text = fieldDate.Day.ToString(CultureInfo.InvariantCulture),
                            Foreground =
                                new SolidColorBrush(dayIndicatorColor)
                        };
                    Grid.SetColumn(dayIndicator, i);
                    Grid.SetRow(dayIndicator, j + 1);
                    GrdDayIndicator.Children.Add(dayIndicator);

                    var dayOpenControl = new CalendarDayControl {TargetDate = fieldDate};
                    Grid.SetColumn(dayOpenControl, i);
                    Grid.SetRow(dayOpenControl, j + 1);
                    GrdCalendarLines.Children.Add(dayOpenControl);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                var vRect = new Rectangle();
                vRect.Fill = new SolidColorBrush(Colors.White);
                vRect.Width = 2;
                vRect.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetRow(vRect, 1);
                Grid.SetRowSpan(vRect, _weekCount);
                Grid.SetColumn(vRect, i);

                GrdCalendarLines.Children.Add(vRect);
            }
            for (int i = 0; i < _weekCount + 1; i++)
            {
                var hRect = new Rectangle();
                hRect.Fill = new SolidColorBrush(Colors.White);
                hRect.Height = 2;
                hRect.VerticalAlignment = VerticalAlignment.Bottom;
                Grid.SetColumnSpan(hRect, 7);
                Grid.SetRow(hRect, i);

                GrdCalendarLines.Children.Add(hRect);
            }

            var targetDate = _firstDayOfCalendarMonth;
            for (int i = 0; i < 7; i++)
            {

                TextBlock dayBlock = new TextBlock();
                Grid.SetColumn(dayBlock, i);
                dayBlock.VerticalAlignment = VerticalAlignment.Bottom;
                dayBlock.HorizontalAlignment = HorizontalAlignment.Center;
                dayBlock.Text = targetDate.ToString("ddd");
                GrdCalendarLines.Children.Add(dayBlock);

                targetDate = targetDate.AddDays(1);
            }


        }

        /// <summary>
        /// Aktualisiert die Termine für den Kalendar im angegebenen Monat
        /// </summary>
        public void RefreshAppointments()
        {
            var calendarEvents = Context.Calendars.Where(o => o._accountId == AccountID).Select(o => o.Events.Where(q => q.To > _firstDayOfCalendarMonth && q.From < _lastDayOfCalendarMonth));
            
            //merge all calendar events
            IEnumerable<TableEvent> events = new TableEvent[0];

            foreach (var calendar in calendarEvents)
                events = events.Concat(calendar);

            events = events
                .OrderByDescending(o => o.To - o.From) //Längere Event sollen oben angezeigt werden
                .ToArray();

            
            //Refresh events to get the changes, if a sync was completed
// ReSharper disable CoVariantArrayConversion
            Context.Refresh(RefreshMode.OverwriteCurrentValues, events);
// ReSharper restore CoVariantArrayConversion

            //Delete displayed events
            GrdAppointments.Children.Clear();
            _dayPanels.Clear();

            //INsert new events
            PutEvent(events);
        }

        /// <summary>
        /// Puts the events into the control
        /// </summary>
        /// <param name="events"></param>
        private void PutEvent(IEnumerable<TableEvent> events)
        {
            foreach (var tableEvent in events)
            {
                var currentDate = tableEvent.From.Date;
                var endDate = tableEvent.To.Date;

                if (endDate == currentDate)
                    endDate = endDate.AddSeconds(1);

                while (currentDate < endDate)
                {
                    StackPanel dPanel = GetDayStackPanel(currentDate);

                    if (dPanel == null) { currentDate = currentDate.AddDays(1); continue; }

                    var tb = new TextBlock
                        {
                            Text = ((tableEvent.IsFullDayEvent || currentDate.Date != tableEvent.From.Date)
                                       ? ""
                                       : (tableEvent.From.ToString("HH:mm "))) +
                                         tableEvent.Title,
                            FontSize = 11
                        };
                    dPanel.Children.Add(tb);

                    currentDate = currentDate.AddDays(1);
                }

            }
        }

        /// <summary>
        /// Gibt das StackPanel zurück, in dem die Termine für einen Tag leigen
        /// </summary>
        /// <returns></returns>
        private StackPanel GetDayStackPanel(DateTime date)
        {
            if (date < _firstDayOfCalendarMonth || date > _lastDayOfCalendarMonth)
                return null;

            int sIndex = (int)(date.Date - _firstDayOfCalendarMonth).TotalDays;

            if (_dayPanels.ContainsKey(sIndex))
                return _dayPanels[sIndex];

            StackPanel newSPanel = new StackPanel();
            newSPanel.Orientation = Orientation.Vertical;
            Grid.SetColumn(newSPanel, sIndex % 7);
            Grid.SetRow(newSPanel, sIndex / 7 + 1);
            GrdAppointments.Children.Add(newSPanel);
            _dayPanels[sIndex] = newSPanel;

            return newSPanel;
        }



        #endregion

        #region Private events

        private void GestureListener_OnDragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            if (e.Direction == Orientation.Horizontal) return;

            SelectedDate = e.VerticalChange > 0 ? SelectedDate.AddMonths(-1) : SelectedDate.AddMonths(1);
        }

        #endregion

        #region public events

        private void OnDateChanging()
        {
            if (DateChanging != null)
                DateChanging(this, new RoutedEventArgs());
        }
        public event RoutedEventHandler DateChanging;

        private void OnDateChanged()
        {
            if (DateChanged != null)
                DateChanged(this, new RoutedEventArgs());
        }
        public event RoutedEventHandler DateChanged;

        #endregion
        
    }
}