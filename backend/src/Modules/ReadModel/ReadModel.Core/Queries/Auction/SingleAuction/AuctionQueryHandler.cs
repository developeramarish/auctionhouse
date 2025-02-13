﻿using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.Auction.SingleAuction;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.SingleAuction
{
    public class AuctionQueryHandler : QueryHandlerBase<AuctionQuery, AuctionRead>
    {
        private readonly ReadModelDbContext _readModelDbContext;

        public AuctionQueryHandler(ReadModelDbContext readModelDbContext)
        {
            _readModelDbContext = readModelDbContext;
        }

        protected override async Task<AuctionRead> HandleQuery(AuctionQuery request, CancellationToken cancellationToken)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(model => model.AuctionId, request.AuctionId);
            var upd = Builders<AuctionRead>.Update.Inc(f => f.Views, 1);

            //TODO FindOneAndUpdate
            AuctionRead auction = await _readModelDbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.And(Builders<AuctionRead>.Filter.AuctionIsNotLocked(), filter))
                .FirstOrDefaultAsync();
            if (auction == null)
            {
                //throw new ResourceNotFoundException($"Cannot find auction with id: {request.AuctionId}");
                throw new Exception($"Cannot find auction with id: {request.AuctionId}");
            }

            await _readModelDbContext.AuctionsReadModel.UpdateManyAsync(filter, upd);

            return auction;
        }
    }
}
