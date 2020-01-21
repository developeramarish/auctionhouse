﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsCanceled : Event
    {
        public decimal Ammount { get; }
        public UserIdentity User { get; }

        public CreditsCanceled(decimal ammount, UserIdentity user) : base("creditsCanceled")
        {
            Ammount = ammount;
            User = user;
        }
    }
}
