using System.Collections.Generic;
using System.Linq;
using OwnCloud.Data;
using OwnCloud.Data.Calendar;
using OwnCloud.Data.Calendar.ParsedCalendar;
using OwnCloud.Data.Calendar.Request;

namespace OwnCloud.Net
{
    /// <summary>
    /// Eine Klasse zum Syncronisieren eines lokalen Kalendars mit dem Server
    /// </summary>
    class CalendarSync
    {
        private OwnCloudDataContext _context;
        private OwnCloudDataContext Context 
        {
            get { return _context ?? (_context = new OwnCloudDataContext()); }
        }
        private TableCalendar[] _localCalendar;
        private OcCalendarClient _ocClient;
        private readonly List<CalendarCalDavInfo> _calendarsToUpdate = new List<CalendarCalDavInfo>();
        private CalendarCalDavInfo _currentUpdatingCalendar = null;
        private readonly List<UnparsedEvent> _eventToUpdate = new List<UnparsedEvent>();

        private string _ocAdress;
        private OwncloudCredentials _credentials;

        public void Sync(string ocAdress, OwncloudCredentials credentials)
        {
            _ocAdress = ocAdress;
            _credentials = credentials;

            LoadLocalCalendar();
            BeginLoadServerCalendar();
        }

        private void LoadLocalCalendar()
        {
            _localCalendar = Context.Calendars.ToArray();
        }

        private void BeginLoadServerCalendar()
        {
            _ocClient = new OcCalendarClient(_ocAdress, _credentials);

            _ocClient.LoadCalendarInfoComplete += LoadCalendarInfoComplete;
            _ocClient.LoadCalendarInfo();
        }

        void LoadCalendarInfoComplete(object sender, LoadCalendarInfoCompleteArgs e)
        {
            if (!e.Success)
            {
                OnSyncComplete(false);
                return;
            }

            //Herrausfinden welche Kalendar aktualisiert werden müssen
            foreach (var calendar in Context.Calendars)
            {
                var serverCalendar = e.CalendarInfo.SingleOrDefault(o => o.Url.ToLower() == calendar.Url.ToLower());

                if (serverCalendar == null)
                    continue;

                if (serverCalendar.GetCTag != calendar.GetCTag)
                {
                    _calendarsToUpdate.Add(serverCalendar);

                    calendar.GetCTag = serverCalendar.GetCTag;
                    _context.SubmitChanges();
                }
            }
            
            UpdateNextLocalCalendar();
        }

        private void UpdateNextLocalCalendar()
        {
            if (_calendarsToUpdate.Count == 0)
            {
                OnSyncComplete(true);
                return;
            }

            _currentUpdatingCalendar = _calendarsToUpdate.First();
            _calendarsToUpdate.Remove(_currentUpdatingCalendar);
            _ocClient.LoadCalendarDataComplete += LoadEventInfoComplete;
            _ocClient.LoadCalendarData(_currentUpdatingCalendar, new CalendarEventRequest{LoadCalendarData = false});
        }

        void LoadEventInfoComplete(LoadCalendarDataCompleteArgs e)
        {
            _ocClient.LoadCalendarDataComplete -= LoadEventInfoComplete;

            if (!e.Success)
            {
                OnSyncComplete(false);
                return;
            }

            //Herrausfinden, welche Termine aktualisiert werden müssen
            foreach (var unparsedEvent in e.Events)
            {
                var dbEvent = _context.Events.SingleOrDefault(o => o.Url == unparsedEvent.EventInfo.Url);

                if (dbEvent == null || dbEvent.GetETag != unparsedEvent.EventInfo.GetETag)
                    _eventToUpdate.Add(unparsedEvent);
            }

            //Herrausfinden, welche Termine gelöscht werden müssen
            var dbCalendar = _context.Calendars.SingleOrDefault(o => o.Url == _currentUpdatingCalendar.Url);
            if (dbCalendar != null)
            {
                var dbEvents = _context.Events.Where(o => o.CalendarId == dbCalendar.Id);
                foreach (var dbEvent in dbEvents)
                {
                    if (e.Events.SingleOrDefault(o => o.EventInfo.Url == dbEvent.Url) == null)
                    {
                        _context.Events.DeleteOnSubmit(dbEvent);
                    }
                }
            }
            _context.SubmitChanges();

            if (_eventToUpdate.Count == 0)
                EndDownloadCalendar();
            else
                LoadEventDetails();
        }

        private void EndDownloadCalendar()
        {
            _calendarsToUpdate.Clear();
            _eventToUpdate.Clear();
            UpdateNextLocalCalendar();
        }

        private void LoadEventDetails()
        {
            _ocClient = new OcCalendarClient(_ocAdress,_credentials);
            _ocClient.LoadCalendarDataComplete += LoadEventDetailsComplete;
            _ocClient.LoadCalendarData(_currentUpdatingCalendar, new CalendarEventRequest { LoadCalendarData = true, Urls = 
            _eventToUpdate.Select(o => o.EventInfo.Url).ToList()});
        }

        public void LoadEventDetailsComplete(LoadCalendarDataCompleteArgs e)
        {
            _ocClient.LoadCalendarDataComplete -= LoadEventDetailsComplete;

            if (!e.Success)
            {
                OnSyncComplete(false);
                return;
            }

            var serverCalendar = _context.Calendars.Single(o => o.Url == _currentUpdatingCalendar.Url);

            foreach (var serverEvent in e.Events)
            {
                var dbEvent = _context.Events.SingleOrDefault(o => o.Url == serverEvent.EventInfo.Url) ??
                              new TableEvent {CalendarId = serverCalendar.Id, Url = serverEvent.EventInfo.Url};

                dbEvent.GetETag = serverEvent.EventInfo.GetETag;
                dbEvent.CalendarData = serverEvent.RawEventData;

                EventMetaUpdater.UpdateEventMetadata(dbEvent);

                if (dbEvent.EventId == 0)
                    _context.Events.InsertOnSubmit(dbEvent);


            }



            _context.SubmitChanges();

            EndDownloadCalendar();

        }



        public event SyncCompleteHandler SyncComplete;
        protected virtual void OnSyncComplete(bool success)
        {
            SyncCompleteHandler handler = SyncComplete;
            if (handler != null) handler(success);
        }
    }
}
