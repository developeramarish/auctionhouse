﻿using Auctions.Domain;
using System;

namespace Test.Auctions.Base.Builders
{
    public class GivenAuction
    {
        private AuctionArgs _args = new GivenAuctionArgs().ValidBuyNowAndBid();
        private AuctionBidsId? _auctionBidsId;

        public GivenAuction WithAuctionArgs(AuctionArgs auctionArgs)
        {
            _args = auctionArgs;
            return this;
        }

        public GivenAuction WithAssignedAuctionBidsId(AuctionBidsId? auctionBidsId = null)
        {
            if (auctionBidsId is null) auctionBidsId = new AuctionBidsId(Guid.NewGuid());
            _auctionBidsId = auctionBidsId;
            return this;
        }

        public Auction Build()
        {
            var auction = new Auction(_args);
            if (_auctionBidsId is not null)
            {
                auction.AddAuctionBids(_auctionBidsId);
            }
            return auction;
        }

        public Auction ValidOfTypeBuyNowAndBid(AuctionBidsId? auctionBidsId = null)
        {
            _args = new GivenAuctionArgs().ValidBuyNowAndBid();
            WithAssignedAuctionBidsId(_auctionBidsId);
            return Build();
        }
    }
}
