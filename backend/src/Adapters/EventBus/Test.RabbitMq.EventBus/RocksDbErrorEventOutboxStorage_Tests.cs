﻿using Adatper.RabbitMq.EventBus.ErrorEventOutbox;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Tests.Base.Mocks.Events;
using Core.Common.Domain;
using EasyNetQ;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Adapter.RabbitMq.EventBus
{
    internal class GivenErrorEventOutboxItem
    {
        private readonly TestAppEventBuilder _appEventBuilder = new();

        private MessageProperties messageProperties = new()
        {
            DeliveryMode = 1,
        };
        private string messageJson;
        private string routingKey = "route";
        private long timestamp = 1;

        public GivenErrorEventOutboxItem(int redeliveryCount = 0) //TODO AppEvent builder
        {
            var cmdContext = CommandContext.CreateNew("test");

            var appEvent = _appEventBuilder.WithCommandContext(cmdContext)
                .WithEvent(new Event("event"))
                .WithRedeliveryCount(redeliveryCount)
                .Build<Event>();

            messageJson = System.Text.Json.JsonSerializer.SerializeToDocument(appEvent, typeof(TestAppEvent<Event>)).RootElement.ToString();
        }

        public GivenErrorEventOutboxItem WithRoutingKey(string routingKey)
        {
            this.routingKey = routingKey;
            return this;
        }

        public GivenErrorEventOutboxItem WithInvalidMesasgeJson()
        {
            messageJson = "";
            return this;
        }

        public GivenErrorEventOutboxItem WithTimestamp(long timestamp)
        {
            this.timestamp = timestamp;
            return this;
        }

        public ErrorEventOutboxItem Build()
        {
            return new ErrorEventOutboxItem
            {
                MessageProperties = messageProperties,
                MessageJson = messageJson,
                RoutingKey = routingKey,
                Timestamp = timestamp,
            };
        }
    }

    public static class ErrorEventOutboxItemAssertions
    {
        internal static void ShouldBeEquivalentToItem(this ErrorEventOutboxItem? item1, ErrorEventOutboxItem? item2)
        {
            Assert.NotNull(item1);
            item1.Should().BeEquivalentTo(item2,
                opt =>
                {
                    opt.Excluding(i => i.MessageProperties.AppIdPresent);
                    opt.Excluding(i => i.MessageProperties.ClusterIdPresent);
                    opt.Excluding(i => i.MessageProperties.ContentEncodingPresent);
                    opt.Excluding(i => i.MessageProperties.ContentTypePresent);
                    opt.Excluding(i => i.MessageProperties.CorrelationIdPresent);
                    opt.Excluding(i => i.MessageProperties.DeliveryModePresent);
                    opt.Excluding(i => i.MessageProperties.ExpirationPresent);
                    opt.Excluding(i => i.MessageProperties.HeadersPresent);
                    opt.Excluding(i => i.MessageProperties.MessageIdPresent);
                    opt.Excluding(i => i.MessageProperties.PriorityPresent);
                    opt.Excluding(i => i.MessageProperties.ReplyToPresent);
                    opt.Excluding(i => i.MessageProperties.TimestampPresent);
                    opt.Excluding(i => i.MessageProperties.TypePresent);
                    opt.Excluding(i => i.MessageProperties.UserIdPresent);

                    return opt;
                }
            );
        }
    }

    public class RocksDbSerialization_Tests
    {
        [Fact]
        public void Deserialized_serialized_item()
        {
            var errorEventOutboxItem = new GivenErrorEventOutboxItem().Build();
            errorEventOutboxItem.ShouldBeEquivalentToItem(RocksDbSerializationUtils.Deserialize(RocksDbSerializationUtils.Serialize(errorEventOutboxItem)));
        }
    }

    [Collection(nameof(RedeliveryTestCollection))]
    [Trait("Category", "Integration")]
    public class RocksDbErrorEventOutboxStorage_Tests : IDisposable
    {
        private readonly RocksDbFixture _dbFixture = new RocksDbFixture();
        private readonly GlobalRocksDb _db;
        RocksDbErrorEventOutboxStorage storage;

        public RocksDbErrorEventOutboxStorage_Tests()
        {
            var options = Options.Create(new RocksDbOptions
            {
                DatabasePath = _dbFixture.TestDbPath,
            });
            _db = new GlobalRocksDb(options);
            storage = new(_db);
        }

        private RocksDb OpenWrite()
        {
            return RocksDb.Open(new DbOptions().SetCreateIfMissing(true), _dbFixture.TestDbPath);
        }

        private RocksDb OpenReadOnly()
        {
            return RocksDb.OpenReadOnly(new DbOptions().SetCreateIfMissing(true), _dbFixture.TestDbPath, true);
        }
        
        private byte[] GetSavedItem(ErrorEventOutboxItem errorEventOutboxItem)
        {
            using var rocksDb = OpenReadOnly();
            return rocksDb.Get(errorEventOutboxItem.Timestamp.ToBytes());
        }

        [Fact]
        public void Saves_item()
        {
            var errorEventOutboxItem = new GivenErrorEventOutboxItem().Build();

            storage.Save(errorEventOutboxItem);

            var itemBytes = GetSavedItem(errorEventOutboxItem);
            itemBytes.Should().NotBeNull();
            RocksDbSerializationUtils.Deserialize(itemBytes).ShouldBeEquivalentToItem(errorEventOutboxItem);
        }

        [Fact]
        public void Saves_item_and_generates_timestamp()
        {
            var errorEventOutboxItem = new GivenErrorEventOutboxItem()
                .WithTimestamp(default)
                .Build();

            storage.Save(errorEventOutboxItem);

            var itemBytes = GetSavedItem(errorEventOutboxItem);
            itemBytes.Should().NotBeNull();
            var deserialized = RocksDbSerializationUtils.Deserialize(itemBytes);
            deserialized.ShouldBeEquivalentToItem(errorEventOutboxItem);
            deserialized.Timestamp.Should().NotBe(default);
        }

        [Fact]
        public void Upates_item()
        {
            var errorEventOutboxItem = new GivenErrorEventOutboxItem()
                .WithTimestamp(2).Build();
            var rockDb = OpenWrite();
            rockDb.Put(errorEventOutboxItem.Timestamp.ToBytes(), RocksDbSerializationUtils.Serialize(errorEventOutboxItem));
            rockDb.Dispose();

            errorEventOutboxItem.MessageJson = "";
            storage.Update(errorEventOutboxItem);

            var itemBytes = GetSavedItem(errorEventOutboxItem);
            itemBytes.Should().NotBeNull();
            RocksDbSerializationUtils.Deserialize(itemBytes).ShouldBeEquivalentToItem(errorEventOutboxItem);
        }

        [Fact]
        public void Finds_unprocessed()
        {
            const int totalItems = 5;
            var rocksDb = OpenWrite();
            var unprocessedItems = Enumerable.Range(0, totalItems).Select(i => new GivenErrorEventOutboxItem()
                .WithTimestamp(i + 1).Build()).ToArray();
            foreach (var item in unprocessedItems)
            {
                rocksDb.Put(item.Timestamp.ToBytes(), RocksDbSerializationUtils.Serialize(item));
            }
            rocksDb.Dispose();

            int i = 0;
            var found = storage.FindUnprocessed(200);

            found.Should().NotBeEmpty();
            found.Length.Should().Be(totalItems);
            foreach (var item in found)
            {
                item.ShouldBeEquivalentToItem(unprocessedItems[i++]);
            }
        }

        [Fact]
        public void Deletes_item()
        {
            var errorEventOutboxItem = new GivenErrorEventOutboxItem()
                    .WithTimestamp(3).Build();
            var rockDb = OpenWrite();
            rockDb.Put(errorEventOutboxItem.Timestamp.ToBytes(), RocksDbSerializationUtils.Serialize(errorEventOutboxItem));
            rockDb.Dispose();

            storage.Delete(errorEventOutboxItem);

            GetSavedItem(errorEventOutboxItem).Should().BeNull();
        }

        public void Dispose()
        {
            _db.Dispose();
            _dbFixture.Dispose();
        }
    }
}
