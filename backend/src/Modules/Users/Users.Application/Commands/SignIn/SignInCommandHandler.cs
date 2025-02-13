﻿using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using Users.Application.Exceptions;
using Users.Domain.Repositories;

namespace Users.Application.Commands.SignIn
{
    public class SignInCommandHandler : CommandHandlerBase<SignInCommand>
    {
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly ILogger<SignInCommandHandler> _logger;

        public SignInCommandHandler(IUserAuthenticationDataRepository userAuthenticationDataRepository, ILogger<SignInCommandHandler> logger, CommandHandlerBaseDependencies dependencies)
        : base(dependencies)
        {
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<SignInCommand> request,
            IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var authData = _userAuthenticationDataRepository.FindUserAuth(request.Command.Username);
            if (authData != null)
            {
                if (authData.Password.Equals(request.Command.Password))
                {
                    var response = RequestStatus.CreatePending(request.CommandContext);
                    response.SetExtraData(new Dictionary<string, object>()
                    {
                        {"UserId", authData.UserId},
                        {"Username", authData.UserName}
                    });
                    response.MarkAsCompleted();
                    return Task.FromResult(response);
                }
                else
                {
                    throw new InvalidPasswordException("Invalid password");
                }
            }

            throw new UserNotFoundException($"Cannot find user {request.Command.Username}");
        }
    }
}
