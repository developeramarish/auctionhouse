﻿using Core.Common.Domain;

namespace UserPayments.Domain.Events
{
    public sealed class BuyNowPaymentCreated : Event
    {
        public Guid PaymentId { get; set; }

        public Guid AuctionId { get; set; }

        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public BuyNowPaymentCreated() : base("buyNowPaymentCreated")
        {
        }
    }
}
