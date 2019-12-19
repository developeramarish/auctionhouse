﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common.Command;

namespace Core.Command.Commands.ResetPassword
{
    public class RequestResetPasswordCommand : ICommand
    {
        [Required] [EmailAddress] public string Email { get; }

        public RequestResetPasswordCommand(string email)
        {
            Email = email;
        }
    }
}