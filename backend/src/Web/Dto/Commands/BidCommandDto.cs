﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class BidCommandDto
    {
        public string AuctionId { get; set; }
        public decimal Price { get; set; }
        public string CorrelationId { get; set; }
    }
}
