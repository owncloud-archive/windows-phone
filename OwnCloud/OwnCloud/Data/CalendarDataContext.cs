using System.Data.Linq;

namespace OwnCloud.Data
{
    class CalendarDataContext : DataContext
    {
        public static string DbConnectionString = "Data Source=isostore:/Calendar.sdf";

        public CalendarDataContext()
            : base(DbConnectionString)
        {
            if (!this.DatabaseExists())
                this.CreateDatabase();
        }


        public Table<TableCalendar> Calendars;
        public Table<TableEvent> Events;

    }

}
