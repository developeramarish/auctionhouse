﻿namespace Auctions.Application.CommandAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AuctionImageAttribute : Attribute
    {
    }
}