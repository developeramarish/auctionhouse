﻿using ReadModel.Core;

namespace Auctionhouse.Query.Adapters
{
    internal static class BidRaisedNotificationsInstaller
    {
        public static ReadModelInstaller AddBidRaisedNotificationsAdapter(this ReadModelInstaller installer)
        {
            installer.Services.AddTransient<BidRaisedNotifications>();
            installer.AddBidRaisedNotifications((prov) => prov.GetRequiredService<BidRaisedNotifications>());
            return installer;
        }
    }
}
