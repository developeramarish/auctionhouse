﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common.Common;
using MediatR;

namespace Core.Query.Queries.Auction.Auctions.ByCategory
{
    public class AuctionsByCategoryQuery : AuctionsQueryBase, IRequest<IEnumerable<AuctionsQueryResult>>
    {
        public int Page { get; set; } = 0;
        [Required]
        [MinCount(1)]
        public List<string> CategoryNames { get; set; }
    }
}
