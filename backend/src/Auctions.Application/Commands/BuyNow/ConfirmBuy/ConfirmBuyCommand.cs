﻿using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;

namespace Auctions.Application.Commands.BuyNow.ConfirmBuy
{
    public class ConfirmBuyCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class ConfirmBuyCommandHandler : CommandHandlerBase<ConfirmBuyCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;
        private readonly ISagaNotifications _sagaNotifications;
        private readonly IAuctionUnlockScheduler _auctionUnlockScheduler;

        public ConfirmBuyCommandHandler(CommandHandlerBaseDependencies dependencies, IAuctionRepository auctions, OptimisticConcurrencyHandler optimisticConcurrencyHandler, ISagaNotifications sagaNotifications, IAuctionUnlockScheduler auctionUnlockScheduler) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _auctions = auctions;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
            _sagaNotifications = sagaNotifications;
            _auctionUnlockScheduler = auctionUnlockScheduler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<ConfirmBuyCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);
            if (auction is null)
            {
                throw new NullReferenceException();
            }

            OutboxItem[] outboxItems = null!;
            await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                if (repeats > 0) auction = _auctions.FindAuction(request.Command.AuctionId);
                auction.ConfirmBuy(request.Command.TransactionId, _auctionUnlockScheduler);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.UpdateAuction(auction);
                    outboxItems = await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
                    await _sagaNotifications.MarkSagaAsCompleted(request.CommandContext.CorrelationId); //TODO separate UOW for saga notifications
                    uow.Commit();
                }
            });
            auction.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
