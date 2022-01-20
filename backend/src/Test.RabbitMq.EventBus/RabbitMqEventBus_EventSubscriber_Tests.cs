﻿using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.RabbitMq.EventBus
{
    public class TestSubEvent : Event
    {
        public TestSubEvent() : base("testSubEvent")
        {
        }
    }

    public class TestsSubHandler : EventSubscriber<TestSubEvent>
    {
        private Action<IAppEvent<TestSubEvent>> OnConsume;
        public bool Throws { get; set; }

        public TestsSubHandler(IAppEventBuilder appEventBuilder, Action<IAppEvent<TestSubEvent>> onConsume) : base(appEventBuilder)
        {
            OnConsume = onConsume;
        }

        public override Task Handle(IAppEvent<TestSubEvent> appEvent)
        {
            OnConsume?.Invoke(appEvent);
            if (Throws)
            {
                throw new Exception();
            }
            return Task.CompletedTask;
        }
    }

    [Trait("Category", "Integration")]
    public class RabbitMqEventBus_EventSubscriber_Tests
    {
        [Fact]
        public void Published_event_gets_handled_by_EventSubscriber()
        {
            var failed = false;
            var sem = new SemaphoreSlim(0, 1);

            var ctx = CommandContext.CreateNew("test", Guid.NewGuid());
            var toPublish = new AppEventRabbitMQBuilder()
                .WithReadModelNotificationsMode(ReadModelNotificationsMode.Immediate)
                .WithCommandContext(ctx)
                .WithEvent(new TestSubEvent())
                .Build<TestSubEvent>();

            var handler = new TestsSubHandler(new AppEventRabbitMQBuilder(), (ev) =>
            {
                try
                {
                    ev.Should().BeEquivalentTo(toPublish);
                }
                catch (Exception)
                {
                    failed = true;
                }
                sem.Release();
            });

            var stubImplProvider = SetupImplProvider(handler);

            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                //ConnectionString = TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
                ConnectionString = "host=localhost",
            }, Mock.Of<ILogger<RabbitMqEventBus>>(), stubImplProvider.Object.Get<IServiceScopeFactory>());
            bus.InitEventSubscriptions(stubImplProvider.Object, Assembly.Load("Test.Adapter.RabbitMq.EventBus"));

            bus.Publish(toPublish);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
                Assert.False(true);
            Assert.False(failed);
        }

        private static Mock<IImplProvider> SetupImplProvider(TestsSubHandler handler)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(handler);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(provider => provider.Get<IServiceScopeFactory>())
                .Returns(serviceProvider.GetRequiredService<IServiceScopeFactory>());
            return stubImplProvider;
        }
    }
}