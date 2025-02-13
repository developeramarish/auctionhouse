﻿using Auctions.Domain;
using Auctions.Tests.Base.Domain.Services.Fakes;
using static Auctions.Tests.Base.Domain.ModelBuilders.AuctionTestConstants;

namespace Auctions.Tests.Base.Domain.ModelBuilders
{
    public class GivenAuctionArgs
    {
        private BuyNowPrice? _buyNowPrice = BUY_NOW_PRICE;
        private UserId _ownerId = UserId.New();
        private Product _product = new Product(PRODUCT_NAME, PRODUCT_DESCRIPTION, Condition.New);
        private string[] _tags = new[] { TAG_1 };
        private AuctionName _name = NAME;
        private string[] _categories = CATEGORY_IDS;
        private bool _buyNowOnly = false;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public GivenAuctionArgs WithBuyNowOnly(bool buyNowOnly)
        {
            _buyNowOnly = buyNowOnly;
            return this;
        }

        public GivenAuctionArgs WithBuyNowOnlyPrice(bool buyNowOnly)
        {
            _buyNowOnly = buyNowOnly;
            return this;
        }

        public GivenAuctionArgs WithBuyNowOnlyPrice(BuyNowPrice? buyNowPrice)
        {
            _buyNowPrice = buyNowPrice;
            return this;
        }

        public GivenAuctionArgs WithOwner(UserId ownerId)
        {
            _ownerId = ownerId;
            return this;
        }

        public GivenAuctionArgs WithEndDate(DateTime dateTime)
        {
            _endDate = dateTime;
            return this;
        }

        public GivenAuctionArgs WithStartDate(DateTime date)
        {
            _startDate = date;
            return this;
        }

        public AuctionArgs Build()
        {
            var auctionArgsBuilder = new AuctionArgs.Builder()
                .SetBuyNow(_buyNowPrice)
                .SetStartDate(_startDate ?? DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(_endDate ?? DateTime.UtcNow.AddDays(5))
                .SetOwner(_ownerId)
                .SetProduct(_product)
                .SetBuyNowOnly(_buyNowOnly)
                .SetTags(_tags)
                .SetName(_name);
            auctionArgsBuilder.SetCategories(_categories, new FakeConvertCategoryNamesToRootToLeafIds()).GetAwaiter().GetResult();
            return auctionArgsBuilder.Build();
        }

        public AuctionArgs ValidForBuyNowAndBidAuctionType()
        {
            var auctionArgsBuilder = new AuctionArgs.Builder()
                .SetBuyNow(BUY_NOW_PRICE)
                .SetStartDate(_startDate ?? DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(_endDate ?? DateTime.UtcNow.AddDays(5))
                .SetOwner(UserId.New())
                .SetProduct(new Product(PRODUCT_NAME, PRODUCT_DESCRIPTION, Condition.New))
                .SetBuyNowOnly(false)
                .SetTags(new[] { TAG_1 })
                .SetName(NAME);
            auctionArgsBuilder.SetCategories(CATEGORY_IDS, new FakeConvertCategoryNamesToRootToLeafIds()).GetAwaiter().GetResult();
            return auctionArgsBuilder.Build();
        }
    }
}
