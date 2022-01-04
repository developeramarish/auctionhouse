﻿namespace Users.Domain.Exceptions
{
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidPasswordException(string message) : base(message)
        {
        }
    }
}