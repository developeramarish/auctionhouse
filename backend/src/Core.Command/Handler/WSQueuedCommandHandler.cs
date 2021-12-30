using System;
using System.Threading;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.RequestStatusSender;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.Handler
{
    public class WSQueuedCommandHandler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<WSQueuedCommandHandler> _logger;

        public WSQueuedCommandHandler(IMediator mediator,
            ILogger<WSQueuedCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public virtual void Handle(QueuedCommand command)
        {
            //TODO
            _ = _mediator.Send((IRequest<RequestStatus>)command.AppCommand, CancellationToken.None).Result;
        }
    }
}