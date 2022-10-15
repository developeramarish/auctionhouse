﻿using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.DomainFramework;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Logging;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;


namespace RabbitMq.EventBus
{
    internal interface IRabbitMqEventBus
    {
        IExchange EventExchange { get; }
        IBus Bus { get; }
    }

    internal class EasyMQBusHolder : IDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly RabbitMqEventBusSubscriptions _eventSubscriptions;
        private readonly RabbitMqEventBusSubscriptions _eventConsumers;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<EasyMQBusHolder> _logger;

        public IBus Bus { get; private set; }
        public IExchange EventExchange { get; }

        public EasyMQBusHolder(RabbitMqSettings settings, IServiceScopeFactory serviceScopeFactory, ILogger<EasyMQBusHolder> logger)
        {
            Bus = RabbitHutch.CreateBus(settings.ConnectionString, s =>
            {
                s.Register<IConsumerErrorStrategy, CustomErrorStrategy>();
            });
            _settings = settings;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            _eventSubscriptions = new(Bus, _logger, _serviceScopeFactory);
            _eventConsumers = new(Bus, _logger, _serviceScopeFactory);

            EventExchange = Bus.Advanced.ExchangeDeclare("Common.Application.Events.IAppEvent`1[[Core.Common.Domain.Event, Common.DomainFramework]], Common.Application",
            cfg =>
            {
            cfg.WithType("topic");
            cfg.AsDurable(true);
            });
        }

        public void InitEventSubscriptions(IImplProvider implProvider, params Assembly[] assemblies)
        {
            _eventSubscriptions.InitSubscriptions(implProvider, new EventSubscriberSeeker(), assemblies);
        }

        public void InitEventConsumers(IImplProvider implProvider, params Assembly[] assemblies)
        {
            _eventConsumers.InitSubscriptions(implProvider, new QueryHandlerSeeker(), assemblies);
        }


        internal void CancelSubscriptions()
        {
            try
            {
                Task.WaitAll(_eventSubscriptions.Subscriptions.Select(e => e.AsTask()).Concat(
                    _eventConsumers.Subscriptions.Select(e => e.AsTask())
                ).ToArray());

                foreach (var subscribption in _eventConsumers.ErrorSubscriptions.Concat(_eventSubscriptions.ErrorSubscriptions))
                {
                    subscribption.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new InfrastructureException("Exception caught while cancelling subscriptions", e);
            }

        }

        public void Dispose()
        {
            Bus.Advanced.Dispose();
            Bus.Dispose();
        }
    }

    internal class RabbitMqEventBus : Common.Application.Events.IEventBus, IRabbitMqEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqEventBus> _logger;
        private readonly EasyMQBusHolder _busHolder;

        public IBus Bus => _busHolder.Bus;
        public IExchange EventExchange => _busHolder.EventExchange;

        public RabbitMqEventBus(RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger, EasyMQBusHolder busHolder)
        {
            settings.ValidateSettings();
            _settings = settings;
            _logger = logger;
            _busHolder = busHolder;
        }

        private void HandleErrorMessage(IMessage<Error> message, MessageReceivedInfo info)
        {
            try
            {
                _logger.LogWarning("Handling error message: {@message} with routingKey: {routingKey}", message, info.RoutingKey);
                //var appEvent = ParseEventFromMessage(message);
                //appEvent.RedeliveryCount++;
                //_bus.PubSub.Publish((IAppEvent<Event>)appEvent, appEvent.Event.GetType().Name);
            }
            catch (Exception)
            {
                _logger.LogError("Cannot handle error message");
            }
        }

        public void SetupErrorQueueSubscribtion()
        {
            var errorQueueName = "EasyNetQ_Default_Error_Queue";
            var queue = Bus.Advanced.QueueDeclare(errorQueueName);
            Bus.Advanced.Consume<Error>(queue, HandleErrorMessage);
        }

        private async Task PublishInternal (IAppEvent<Event> @event)
        {
            try
            {
                await Bus.PubSub.PublishAsync(@event, @event.Event.GetType().Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not publish event");
                throw new InfrastructureException("Could not publish event", e);
            }
        }

        public async Task Publish<T>(IAppEvent<T> @event) where T : Event
        {
            _logger.LogDebug("Publishing event {@event}", @event);
            await PublishInternal(@event);
        }

        public async Task Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
        {
            foreach (var @event in events)
            {
                await Publish(@event);
            }
        }
    }
}