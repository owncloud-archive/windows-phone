using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Phone.Controls;

namespace OwnCloud.Data
{
    public class CalendarDaysDataContext : Entity
    {
        public CalendarDaysDataContext(DateTime startDate)
        {
            _startDate = startDate.Date;

            Days = new ObservableCollection<DateTime>();
            for (var i = -10; i < 10; i++)
            {
                Days.Add(_startDate.AddDays(i));
            }

        }

        private DateTime _startDate;

        private ObservableCollection<DateTime> _days;
        public ObservableCollection<DateTime> Days
        {
            get { return _days; }
            set { _days = value; OnPropertyChanged("Days"); }
        }


        public void ItemLinked(object sender, LinkUnlinkEventArgs e)
        {
            var last = Days.Last();
            
            if (e.ContentPresenter.Content.Equals(last))
            {
                Days.Add(last.AddDays(1));
            }
        }

        public void AddOnTop()
        {
            var first = Days.First();
            Days.Insert(0,(first.AddDays(-1)));
        }

    }


    public delegate void AddedItemsOnTopHandler(int count);

}
