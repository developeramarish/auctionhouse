﻿using System;
using Core.Common.Domain.Users.Events;
using Core.Common.Exceptions;
using ReflectionMagic;

namespace Core.Common.Domain.Users
{
    public partial class User
    {
        public override Event GetRemovedEvent()
        {
            return new UserRemoved();
        }

        protected override void Apply(Event @event)
        {
            try
            {
                this.AsDynamic().ApplyEvent(@event);
            }
            catch (Exception)
            {
                throw new DomainException($"Unrecognized event: {@event.EventName}");
            }
        }

        protected override UserUpdateEventGroup CreateUpdateEventGroup()
        {
            return new UserUpdateEventGroup();
        }

        private void ApplyEvent(UserRegistered @event)
        {
            AggregateId = @event.UserIdentity.UserId;
            Register(@event.UserIdentity.UserName);
        }

        private void ApplyEvent(CreditsAdded ev) => AddCredits(ev.CreditsCount);
        private void ApplyEvent(CreditsReturned ev) => ReturnCredits(ev.CreditsCount);
        private void ApplyEvent(CreditsWithdrawn ev) => WithdrawCredits(ev.CreditsCount);
        private void ApplyEvent(CreditsCanceled ev) => CancelCredits(ev.Ammount);

    }
}