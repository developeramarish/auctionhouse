﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Services;
using Core.Common.Domain.Users;
using Core.Common.DomainServices;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.BuyNow
{
    public class BuyNowCommandHandlerTransactionDecorator : CommandHandlerBase<BuyNowCommand>
    {
        private BuyNowCommandHandler _buyNowCommandHandler;

        public BuyNowCommandHandlerTransactionDecorator(ILogger<BuyNowCommandHandlerTransactionDecorator> logger, BuyNowCommandHandler buyNowCommandHandler) : base(logger)
        {
            _buyNowCommandHandler = buyNowCommandHandler;
        }

        protected override Task<RequestStatus> HandleCommand(BuyNowCommand request, CancellationToken cancellationToken)
        {
            var transactionOpt = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(15)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOpt))
            {
                var status = _buyNowCommandHandler.Handle(request, cancellationToken);
                scope.Complete();

                return status;
            }
        }
    }

    public class BuyNowCommandHandler : DecoratedCommandHandlerBase<BuyNowCommand>
    {
        private IAuctionPaymentVerification _auctionPaymentVerification;
        private IAuctionRepository _auctionRepository;
        private EventBusService _eventBusService;
        private ILogger<BuyNowCommandHandler> _logger;

        public BuyNowCommandHandler(IAuctionRepository auctionRepository, EventBusService eventBusService, ILogger<BuyNowCommandHandler> logger, IAuctionPaymentVerification auctionPaymentVerification) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
            _auctionPaymentVerification = auctionPaymentVerification;
        }

        protected override async Task<RequestStatus> HandleCommand(BuyNowCommand request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                throw new CommandException($"Invalid auction id: {request.AuctionId}");
            }

            _logger.LogDebug($"User {request.SignedInUser} is buying auction {request.AuctionId}");

            await auction.Buy(new Common.Domain.Auctions.UserId(request.SignedInUser.Value), "test", _auctionPaymentVerification);
            _eventBusService.Publish(auction.PendingEvents, request.CommandContext.CorrelationId, request);

            return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.PENDING);
        }
    }
}