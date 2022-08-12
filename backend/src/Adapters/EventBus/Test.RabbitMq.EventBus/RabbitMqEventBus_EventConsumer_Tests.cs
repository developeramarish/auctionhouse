using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Test.Adapter.RabbitMq.EventBus;
using TestConfigurationAccessor;
using Xunit;

namespace Test.RabbitMq.EventBus
{
    public class TestEvent : Event
    {
        public TestEvent() : base("testEvent")
        {
        }
    }

    public class TestHandler : EventConsumer<TestEvent, TestHandler>
    {
        private Action<IAppEvent<TestEvent>> OnConsume;
        public bool Throws { get; set; }

        public TestHandler(EventConsumerDependencies dependencies, Action<IAppEvent<TestEvent>> onConsume) : base(Mock.Of<ILogger<TestHandler>>(), dependencies)
        {
            OnConsume = onConsume;
        }

        public override Task Consume(IAppEvent<TestEvent> appEvent)
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
    public class RabbitMqEventBus_EventConsumer_Tests
    {
        [Fact]
        public async Task Published_event_gets_handled_by_EventConsumer()
        {
            var failed = false;
            var sem = new SemaphoreSlim(0, 1);


            var ctx = CommandContext.CreateNew("test", Guid.NewGuid());
            var toPublish = new AppEventRabbitMQBuilder()
                .WithReadModelNotificationsMode(ReadModelNotificationsMode.Saga)
                .WithCommandContext(ctx)
                .WithEvent(new TestEvent())
                .Build<TestEvent>();

            var handler = new TestHandler(new EventConsumerDependencies(new AppEventRabbitMQBuilder(), Mock.Of<IEventConsumerCallbacks>())
            , (ev) =>
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

            var bus = new RabbitMqEventBus(TestConfig.Instance.GetRabbitMqSettings(), stubImplProvider.Get<ILogger<RabbitMqEventBus>>(), stubImplProvider.Get<IServiceScopeFactory>());
            bus.InitEventConsumers(stubImplProvider, Assembly.Load("Test.Adapter.RabbitMq.EventBus"));

            await bus.Publish(toPublish);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
                Assert.False(true);
            Assert.False(failed);
        }

        private static ImplProviderMock SetupImplProvider(TestHandler handler)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(b => b.AddXUnit());
            serviceCollection.AddSingleton(handler);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return new ImplProviderMock(serviceProvider);
        }
    }
}