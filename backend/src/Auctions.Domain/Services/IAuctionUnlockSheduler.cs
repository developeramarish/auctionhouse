﻿using Core.Common.Domain.Auctions;

namespace Auctions.Domain.Services
{
    public interface IAuctionUnlockSheduler
    {
        Task SheduleUnlock(Auction auction);
    }
}
