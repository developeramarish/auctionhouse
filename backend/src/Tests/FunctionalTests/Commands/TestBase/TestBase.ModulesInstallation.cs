﻿using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.DI;
using Auctions.Application.Commands.BuyNow;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Tests.Base;
using Auctions.Tests.Base.Domain.Services.Fakes;
using Categories.DI;
using ChronicleEfCoreStorage;
using Common.Application;
using Common.Application.Events;
using Common.Tests.Base;
using Common.Tests.Base.Mocks;
using FunctionalTests.Mocks;
using IntegrationService.AuctionPaymentVerification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using ReadModel.Core;
using ReadModel.Core.Services;
using System;
using System.Linq;
using System.Reflection;
using TestConfigurationAccessor;
using UserPayments.DI;
using Users.Application.Commands.SignUp;
using Users.DI;
using Users.Tests.Base.Mocks;
using XmlCategoryTreeStore;

namespace FunctionalTests.Commands
{
    public partial class TestBase
    {
        private IServiceProvider BuildConfiguredServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton(TestConfig.Instance);

            var commandHandlerAssemblies = assemblyNames.Select(n => Assembly.Load(n)).ToArray();
            //missing query dependencies
            new CommonApplicationMockInstaller(services)
                .AddCommandCoreDependencies(
                    null,
                    null,
                       ImplProviderMock.Factory,
                       commandHandlerAssemblies)
                .AddQueryCoreDependencies(
                    implProviderFactory: ImplProviderMock.Factory,
                    commandHandlerAssemblies
                )
                .AddRabbitMqEventBusAdapter(null, rabbitMqSettings: TestConfig.Instance.GetRabbitMqSettings())
                .AddRabbitMqAppEventBuilderAdapter()
                .AddOutboxItemStore(_ => InMemoryOutboxItemStore.Create())
                .AddOutboxItemFinder(_ => InMemoryPostProcessOutboxItemService.Create())
                .AddUserIdentityService(_ => _userIdentityService);

            services.Decorate<IEventBus, InMemoryEventBusDecorator>();

            new AuctionsMockInstaller(services)
                .Domain
                    .AddAuctionCreateSessionStore((prov) => InMemAuctionCreateSessionStore.Instance)
                    .AddAuctionRepository((prov) => FakeAuctionRepository.Instance)
                    .AddCategoryNamesToTreeIdsConversion((prov) => ConvertCategoryNamesToRootToLeafIdsMock.Create())
                    .AddAuctionEndScheduler((prov) => AuctionEndSchedulerMock.Create())
                    .AddAuctionPaymentVerificationAdapter();

            new CategoriesInstaller(services)
                .AddXmlCategoryTreeStoreAdapter(settings: TestConfig.Instance.GetXmlStoreSettings());

            new AuctionBidsInstaller(services)
                .Domain
                 .AddAuctionBidsRepository(_ => InMemoryAuctionBidsRepository.Instance);

            new UsersInstaller(services)
                .Domain
                    .AddUserRepository(_ => InMemoryUserRepository.Instance)
                    .AddUserAuthenticationDataRepository(s => new InMemUserAuthenticationDataRepository());

            new UserPaymentsInstaller(services)
                .Domain
                    .AddUserPaymentsRepository(s => InMemortUserPaymentsRepository.Instance);

            services.AddChronicleSQLServerStorage((sagaType) => sagaType switch
            {
                nameof(BuyNowSaga) => typeof(BuyNowSaga),
                nameof(CreateAuctionSaga) => typeof(CreateAuctionSaga),
                nameof(SignUpSaga) => typeof(SignUpSaga),
                _ => throw new NotImplementedException(),
            }, TestConfig.Instance.GetChronicleSQLServerStorageConnectionString());

            services.AddCommandEfCoreReadModelNotifications(TestConfig.Instance, settings: TestConfig.Instance.GetEfCoreReadModelNotificaitonsOptions());
            services.AddQueryEfCoreReadModelNotifications(TestConfig.Instance, settings: TestConfig.Instance.GetEfCoreReadModelNotificaitonsOptions());

            services.AddLogging(c =>
            {
                c.AddXUnit(_outputHelper);
                c.SetMinimumLevel(LogLevel.Debug);
            });


            services.AddReadModel(TestConfig.Instance.GetReadModelSettings());
            services.AddEventConsumers(typeof(ReadModelInstaller));
            services.AddAutoMapper(typeof(Auctionhouse.Query.QueryMapperProfile).Assembly);



            services.AddSingleton(Mock.Of<IBidRaisedNotifications>());

            AddCustomServices(services);

            return services.BuildServiceProvider();
        }

        protected virtual void AddCustomServices(IServiceCollection services)
        {

        }
    }
}
