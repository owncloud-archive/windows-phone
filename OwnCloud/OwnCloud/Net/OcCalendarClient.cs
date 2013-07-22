using System;
using System.IO;
using OwnCloud.Data;
using OwnCloud.Data.Calendar.ParsedCalendar;
using OwnCloud.Data.Calendar.Request;

namespace OwnCloud.Net
{
    /// <summary>
    /// Client den Owncloud Kalendar
    /// </summary>
    class OcCalendarClient : HttpOcClient
    {
        #region ctor

        /// <summary>
        /// Erstellt eine neue Instanz des Kalendarclients
        /// </summary>
        /// <param name="ocAddress">Pfad der Owncloudinstanz</param>
        /// <param name="credentials"></param>
        public OcCalendarClient(string ocAddress, OwncloudCredentials credentials)
            : base(ocAddress, credentials)
        { }


        #endregion

        #region LoadCalendarInfo

        public void LoadCalendarInfo()
        {
            this.BeginHttpRequest(Address.Combine("remote.php/caldav/calendars/" + Credentials.Username + "/"));

            ContentTypeXml();
            this.SetMethod("PROPFIND");

            Request.BeginGetRequestStream(CalendarInfoWriteRequest, null);
        }

        private void CalendarInfoWriteRequest(IAsyncResult ar)
        {
            var requestStream = Request.EndGetRequestStream(ar);

            CalendarRequest.WriteCalendarRequest(requestStream);

            requestStream.Close();

            Request.BeginGetResponse(CalendarInfoGetResponse, null);
        }

        private void CalendarInfoGetResponse(IAsyncResult ar)
        {
            try
            {
                var response = Request.EndGetResponse(ar);

                EndHttpRequest();
                OnLoadCalendarInfoComplete(new LoadCalendarInfoCompleteArgs
                {
                    Success = true,
                    CalendarInfo = MultistateParser.ParseCalendarCalDavInfo(response.GetResponseStream())
                });
            }
            catch (Exception ex)
            {
                EndHttpRequest();
                OnLoadCalendarInfoComplete(new LoadCalendarInfoCompleteArgs { Success = false });
            }
        }

        #endregion

        #region LoadCalendarData

        /// <summary>
        /// Lädt die Termine eines Kalendars
        /// </summary>
        /// <param name="cInfo">Die Information über den Kalendar</param>
        /// <param name="request">The request</param>
        public void LoadCalendarData(CalendarCalDavInfo cInfo, CalendarEventRequest request = null)
        {
            this.BeginHttpRequest(Address.CombineServerAddress(cInfo.Url));

            ContentTypeXml();
            this.SetMethod("REPORT");

            Request.Headers["Depth"] = "1";
            Request.Headers["Prefer"] = "return-minimal";

            if (request == null)
                request = new CalendarEventRequest();

            Request.BeginGetRequestStream(CalendarDataWriteRequest, request);
        }

        /// <summary>
        /// Lädt die Termine eines Kalendars
        /// </summary>
        /// <param name="calendarUrl">Die Server url des Kaldendars</param>
        public void LoadCalendarData(string calendarUrl)
        {
            LoadCalendarData(new CalendarCalDavInfo{ Url = calendarUrl});
        }

        private void CalendarDataWriteRequest(IAsyncResult ar)
        {
            var requestStream = Request.EndGetRequestStream(ar);

            var request = ar.AsyncState as CalendarEventRequest;

            request.WriteCalendarRequest(requestStream);

            requestStream.Close();

            Request.BeginGetResponse(CalendarDataGetResponse, null);
        }

        private void CalendarDataGetResponse(IAsyncResult ar)
        {
            try
            {
                var response = Request.EndGetResponse(ar);

                EndHttpRequest();
                var param = new LoadCalendarDataCompleteArgs
                    {
                        Success = true,
                        Events = MultistateParser.ParseCalendarEvents(response.GetResponseStream())
                    };
                try
                {
                    OnLoadCalendarDataComplete(param);
                }
                catch (Exception ex)
                {
                    throw;
                }
                
            }
            catch (Exception)
            {
                EndHttpRequest();
                OnLoadCalendarDataComplete(new LoadCalendarDataCompleteArgs {Success = false});
            }
        }

        #endregion

        #region Save Event

        public void SaveEvent(TableEvent eventToSave)
        {
            this.BeginHttpRequest(Address.CombineServerAddress(eventToSave.Url));

            ContentTypeCalendar();
            SetMethod("PUT");


            Request.BeginGetRequestStream(SaveEventWriteRequest, eventToSave);
        }

        private void SaveEventWriteRequest(IAsyncResult result)
        {
            var eventToSave = result.AsyncState as TableEvent;

            //Write Calendar to Request
            var requestStream = Request.EndGetRequestStream(result);
            using(var writer = new StreamWriter(requestStream))
            {
                writer.Write(eventToSave.CalendarData);
            }
            requestStream.Dispose();

            Request.BeginGetResponse(EndSaveEventRequest, null);
        }

        private void EndSaveEventRequest(IAsyncResult ar)
        {
            try
            {
                var response = Request.EndGetResponse(ar);

                EndHttpRequest();
                OnSaveEventComplete(true);
            }
            catch (Exception)
            {
                EndHttpRequest();
                OnSaveEventComplete(false);
            }
        }

        #endregion

        #region Delete Event

        public void DeleteEvent(string eventUrl)
        {
            this.BeginHttpRequest(Address.CombineServerAddress(eventUrl));

            SetMethod("DELETE");

            Request.BeginGetResponse(EndDeleteEvent, null);
        }

        private void EndDeleteEvent(IAsyncResult ar)
        {
            try
            {
                var response = Request.EndGetResponse(ar);

                EndHttpRequest();
                OnDeleteEventComplete(true);
            }
            catch (Exception)
            {
                OnDeleteEventComplete(false);
                throw;
            }
        }

        #endregion


        #region Events

        public event LoadCalendarInfoCompleteHandler LoadCalendarInfoComplete;
        protected virtual void OnLoadCalendarInfoComplete(LoadCalendarInfoCompleteArgs e)
        {
            LoadCalendarInfoCompleteHandler handler = LoadCalendarInfoComplete;
            if (handler != null) handler(this, e);
        }

        public event LoadCalendarDataCompleteHandler LoadCalendarDataComplete;
        protected virtual void OnLoadCalendarDataComplete(LoadCalendarDataCompleteArgs e)
        {
            if (LoadCalendarDataComplete != null) 
                LoadCalendarDataComplete(e); //Method not found exception
        }

        public event SaveEventCompleteHandler SaveEventComplete;
        protected virtual void OnSaveEventComplete(bool success)
        {
            if (SaveEventComplete != null) SaveEventComplete(success);
        }

        public event DeleteEventCompleteEventHandler DeleteEventComplete;
        protected virtual void OnDeleteEventComplete(bool success)
        {
            DeleteEventCompleteEventHandler handler = DeleteEventComplete;
            if (handler != null) handler(success);
        }

        #endregion


    }
}
