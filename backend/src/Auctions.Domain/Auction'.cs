﻿using Auctions.DomainEvents;
using Auctions.DomainEvents.Update;
using Core.Common.Domain;
using Core.DomainFramework;
using ReflectionMagic;

namespace Auctions.Domain
{
    public partial class Auction
    {
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

        protected override AuctionUpdateEventGroup CreateUpdateEventGroup()
        {
            return new AuctionUpdateEventGroup(Owner);
        }

        private void ApplyEvent(AuctionCreated @event)
        {
            AggregateId = @event.AuctionId;
            Create(new AuctionArgs
            {
                AuctionImages = @event.AuctionImagesSize1Id.Select((id, i) => new AuctionImage(@event.AuctionImagesSize1Id[i],
                @event.AuctionImagesSize2Id[i], @event.AuctionImagesSize3Id[i])).ToArray(),
                BuyNowOnly = @event.BuyNowOnly,
                BuyNowPrice = @event.BuyNowPrice,
                Categories = @event.Category.Select(c => new CategoryId(c)).ToArray(),
                EndDate = @event.EndDate,
                Name = @event.Name,
                Owner = @event.Owner,
                Product = new Product(@event.ProductName, @event.ProductDescription, (Condition)@event.ProductCondition),
                StartDate = @event.StartDate,
                Tags = @event.Tags.Select(t => new Tag(t)).ToArray(),
            }, false);
        }

        private void ApplyEvent(AuctionImageAdded @event) => AddImage(new AuctionImage(@event.AddedImageSize1Id, @event.AddedImageSize2Id, @event.AddedImageSize3Id));
        private void ApplyEvent(AuctionImageRemoved @event) => RemoveImage(@event.ImgNum);
        private void ApplyEvent(AuctionImageReplaced @event) => ReplaceImage(new AuctionImage(@event.ImageSize1Id, @event.ImageSize2Id, @event.ImageSize3Id), @event.ImgNum);

        private void ApplyEvent(AuctionLocked @event)
        {
            Locked = true;
            LockIssuer = @event.LockIssuer;
        }

        private void ApplyEvent(AuctionUnlocked @event)
        {
            Locked = true;
            LockIssuer = UserId.Empty;
        }

        private void ApplyEvent(AuctionBuyNowPriceChanged ev) => UpdateBuyNowPrice(ev.BuyNowPrice);
        private void ApplyEvent(AuctionUpdateEventGroup group) => group.UpdateEvents.ForEach(ev => Apply(ev));
        private void ApplyEvent(AuctionEndDateChanged ev) => UpdateEndDate(ev.Date);
        private void ApplyEvent(AuctionTagsChanged ev) => UpdateTags(ev.Tags.Select(t => new Tag(t)).ToArray());
        private void ApplyEvent(AuctionCategoriesChanged ev) => UpdateCategories(ev.Categories.Select(c => new CategoryId(c)).ToArray());
        private void ApplyEvent(AuctionNameChanged ev) => UpdateName(ev.AuctionName);
        private void ApplyEvent(AuctionDescriptionChanged ev) => UpdateDescription(ev.Description);


    }
}