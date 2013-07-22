using System.Data.Linq.Mapping;

namespace OwnCloud.Data
{
    /// <summary>
    /// Stellt einen CalDav Kalendar zum speichern in der Datenbank da.
    /// </summary>
    [Table(Name = "TableCalendar")]
    class TableCalendar : DbEntity
    {

        private int _calendarId;
        /// <summary>
        /// PK des Kalendars
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get { return _calendarId; }
            set
            {
                if (_calendarId == value) return;
                OnPropertyChanging("Id");
                _calendarId = value;
                OnPropertyChanged("Id");
            }
        }

        private string _url;
        /// <summary>
        /// Die URL des Kalendars relativ zum Server Hostname
        /// </summary>
        [Column] 
        public string Url
        {
            get { return _url; }
            set
            {
                OnPropertyChanging("Url");
                _url = value;
                OnPropertyChanged("Url");
            }
        }

        
        private string _getCTag;
        /// <summary>
        /// Der CTag des Kalendars. Er wird vom CalDavServer bei jeder änderung aktualisiert.
        /// </summary>
        [Column] 
        public string GetCTag
        {
            get { return _getCTag; }
            set { OnPropertyChanging("GetCTag"); _getCTag = value; OnPropertyChanged("GetCTag"); }
        }

        private string _displayName;
        /// <summary>
        /// Der Anzeigename des Kalendars
        /// </summary>
        [Column]
        public string DisplayName
        {
            get { return _displayName; }
            set { OnPropertyChanging("DisplayName"); _displayName = value; OnPropertyChanged("DisplayName"); }
        }


        /// <summary>
        /// Erstellt einen Kalendar zum speichern in der Datenbank aus
        /// </summary>
        /// <param name="calendar">Die Kalendarinformationen</param>
        /// <returns></returns>
        public static TableCalendar FromCalDavCalendarInfo(Calendar.ParsedCalendar.CalendarCalDavInfo calendar)
        {
            return new TableCalendar()
                {
                    Url = calendar.Url
                    , GetCTag = calendar.GetCTag
                    , DisplayName = calendar.DisplayName
                };
        }

    }
}
