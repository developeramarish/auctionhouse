﻿using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.Commands
{
    using Adapter.AuctionImageConversion;
    using Auctionhouse.Command.Adapters;
    using Auctions.Application;
    using Auctions.Application.Commands.AddAuctionImage;
    using Auctions.Application.Commands.RemoveImage;
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using Auctions.DI;
    using Auctions.Domain;
    using Auctions.Domain.Services;
    using FluentAssertions;
    using FunctionalTests.Mocks;
    using Xunit.Abstractions;

    [Trait("Category", "Functional")]
    public class AuctionCreateSessionCommands_Tests : TestBase
    {
        private const string TestImagePath = "imageData/1200x600.jpg";
        private readonly IAuctionCreateSessionStore auctionCreateSessionStore;

        public AuctionCreateSessionCommands_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application")
        {
            auctionCreateSessionStore = ServiceProvider.GetRequiredService<IAuctionCreateSessionStore>();
        }

        protected override void ConfigureAuctionsModuleCustomDependencies(AuctionsInstaller installer)
        {
            base.ConfigureAuctionsModuleCustomDependencies(installer);
            installer.Application.AddTempFileServiceAdapter();
            installer.Domain.AddAuctionImageConversionAdapter();
        }

        [Fact]
        public async Task Adds_image_to_AuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);

            var addImg = new AddAuctionImageCommand()
            {
                Img = TestFileStreamAccessor.ForFile(TestImagePath),
                Extension = "jpg",
                ImgNum = 0,
            };
            await SendCommand(addImg);

            auctionCreateSessionStore.GetExistingSession().Should().NotBeNull();
        }

        [Fact]
        public async Task Removes_image_from_AuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);

            var addImg = new AddAuctionImageCommand()
            {
                Img = TestFileStreamAccessor.ForFile(TestImagePath),
                Extension = "jpg",
                ImgNum = 0,
            };
            await SendCommand(addImg);

            var removeImage = new RemoveImageCommand(0);
            await SendCommand(removeImage);

            auctionCreateSessionStore.GetExistingSession().AuctionImages.Any(i => i != null).Should().BeFalse();
            auctionCreateSessionStore.GetExistingSession().Should().NotBeNull();
        }
    }
}