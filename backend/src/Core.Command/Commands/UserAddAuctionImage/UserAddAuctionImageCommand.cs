﻿using System;
using System.ComponentModel.DataAnnotations;
using Core.Command.Exceptions;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Common;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.UserAddAuctionImage
{
    [AuthorizationRequired]
    [SaveTempAuctionImage]
    public class UserAddAuctionImageCommand : ICommand    {
        public Guid AuctionId { get; }

        [AuctionImage]
        public IFileStreamAccessor Img { get; set; }

        [SaveTempPath]
        public string TempPath { get; set; }

        [Required]
        [MaxLength(5)]
        [ValidAuctionImageExtension]
        public string Extension { get; }

        [SignedInUser]
        public Core.Common.Domain.Users.UserId SignedInUser { get; set; }

        public UserAddAuctionImageCommand(Guid auctionId, IFileStreamAccessor img, string extension)
        {
            //TODO validator
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Img = img;
            Extension = extension;
        }
    }
}
