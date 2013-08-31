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
            string name = "";

            try
            {
                using (var context = new OwnCloudDataContext())
                {
                    var account = context.Accounts.Single(o => o.GUID == _accountID);
                    account.RestoreCredentials();
                    name = account.Username + " ";
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
                //Do nothing
            }

            var invokeUrl = new Uri( "/View/Page/CalendarMonthPage.xaml?uid=" + _accountID.ToString(), UriKind.Relative);

            if (ShellTile.ActiveTiles.Any(o => o.NavigationUri.Equals(invokeUrl)))
                return;

            ShellTile.Create(invokeUrl, new StandardTileData()
                {
                    Title = name + Resource.Localization.AppResources.Tile_KalendarTitle,
                    BackgroundImage = new Uri("Resource/Image/CalendarLogo.png", UriKind.Relative)
                });
        }

    }
}
