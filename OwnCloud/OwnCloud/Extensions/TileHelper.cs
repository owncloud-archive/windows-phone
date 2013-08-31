using System;
using System.Linq;
using Microsoft.Phone.Shell;
using OwnCloud.Data;

namespace OwnCloud.Extensions
{
    public static class TileHelper
    {

        public static void AddCalendarToTile(int _accountID)
        {
            string name = Resource.Localization.AppResources.Tile_KalendarTitle;

            try
            {
                using (var context = new OwnCloudDataContext())
                {
                    var account = context.Accounts.Single(o => o.GUID == _accountID);
                    account.RestoreCredentials();
                    name = account.Username + " " + name;
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
                //Do nothing
            }

            var invokeUrl = new Uri( "/View/Page/CalendarMonthPage.xaml?uid=" + _accountID.ToString(), UriKind.Relative);

            PinUrlToStart(invokeUrl, name, "Resource/Image/CalendarLogo.png");
        }

        public static void AddOnlineFilesToTile(int _accountID)
        {
            string name = Resource.Localization.AppResources.Tile_RemoteFileTitle;

            try
            {
                using (var context = new OwnCloudDataContext())
                {
                    var account = context.Accounts.Single(o => o.GUID == _accountID);
                    account.RestoreCredentials();
                    name = account.Username + " " + name;
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                //Do nothing
            }

            var invokeUrl = new Uri("/View/Page/RemoteFiles.xaml?account=" + _accountID, UriKind.Relative);

            PinUrlToStart(invokeUrl, name, "Resource/Image/RemoteFolderLogo.png");
        }


        private static void PinUrlToStart(Uri invokeUrl, string name, string logoUrl)
        {
            if (ShellTile.ActiveTiles.Any(o => o.NavigationUri.Equals(invokeUrl)))
                return;

            ShellTile.Create(invokeUrl, new StandardTileData()
                {
                    Title = name,
                    BackgroundImage = new Uri(logoUrl, UriKind.Relative)
                });
        }
    }
}
