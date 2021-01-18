using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using SmallLister.Feed;
using SmallLister.Model;
using Xunit;

namespace SmallLister.Tests.Feed
{
    public class AtomFeedGeneratorTests
    {
        [Fact]
        public void Generate_empty_feed()
        {
            var generator = new AtomFeedGenerator();
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", new List<UserItem>(), new UserFeed
            {
                UserFeedIdentifier = "mytestfeed"
            });
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain("<id>https://smalllister.nosuchblogger.com/feed/mytestfeed</id>");
            atomXmlString.Should().NotContain("<entry>");
        }

        [Fact]
        public void Generate_feed_entries()
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.ItemDisplay = UserFeedItemDisplay.Description;
            var items = fixture.Build<UserItem>().CreateMany();
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", items, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");
            atomXmlString.Should().Contain("<entry>");
        }
    }
}