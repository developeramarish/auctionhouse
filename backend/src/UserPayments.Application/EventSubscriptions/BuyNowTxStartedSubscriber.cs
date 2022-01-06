﻿using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Application.Commands.CreateBuyNowPayment;

namespace UserPayments.Application.EventSubscriptions
{
    public class BuyNowTxStartedSubscriber : EventSubscriber<Events.V1.BuyNowTXStarted>
    {
        private ImmediateCommandQueryMediator _mediator;

        public BuyNowTxStartedSubscriber(IAppEventBuilder eventBuilder, ImmediateCommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<Events.V1.BuyNowTXStarted> appEvent)
        {
            var cmd = new CreateBuyNowPaymentCommand
            {
                AuctionId = appEvent.Event.AuctionId,
                BuyerId = appEvent.Event.BuyerId,
                TransactionId = appEvent.Event.TransactionId,
                PaymentMethod = appEvent.Event.PaymentMethod,
            };
            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
