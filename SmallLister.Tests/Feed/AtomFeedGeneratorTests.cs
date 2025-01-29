using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallLister.Feed;
using SmallLister.Model;

namespace SmallLister.Tests.Feed
{
    [TestClass]
    public class AtomFeedGeneratorTests
    {
        [TestMethod]
        [DataRow(UserFeedType.Daily)]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Generate_empty_feed(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, new List<UserItem>(), new UserFeed
            {
                UserFeedIdentifier = "mytestfeed",
                FeedType = userFeedType,
                UserAccount = new()
            });
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain("<id>https://smalllister.nosuchblogger.com/feed/mytestfeed</id>");
            atomXmlString.Should().NotContain("<entry>");
        }

        [TestMethod]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Generate_feed_entries(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.ItemDisplay = UserFeedItemDisplay.Description;
            userFeed.FeedType = userFeedType;
            var items = fixture.Build<UserItem>().CreateMany();
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, items, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");
            atomXmlString.Should().Contain("<entry>");
        }

        [TestMethod]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Create_description_for_due_item(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.FeedType = userFeedType;
            var item = fixture.Create<UserItem>();
            item.PostponedUntilDate = null;
            item.NextDueDate = DateTime.Today;
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, new[] { item }, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");
            atomXmlString.Should().Contain("You have an item that is due today");
        }

        [TestMethod]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Create_description_for_item_due_yesterday(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.FeedType = userFeedType;
            var item = fixture.Create<UserItem>();
            item.PostponedUntilDate = null;
            item.NextDueDate = DateTime.Today.AddDays(-1);
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, new[] { item }, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");
            atomXmlString.Should().Contain("You have an overdue item! It was due yesterday");
        }

        [TestMethod]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Create_description_for_item_postponed_until_yesterday(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.FeedType = userFeedType;
            var item = fixture.Create<UserItem>();
            item.PostponedUntilDate = DateTime.Today.AddDays(-1);
            item.NextDueDate = DateTime.Today.AddDays(-10);
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, new[] { item }, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");
            atomXmlString.Should().Contain("You have an overdue item! It was due yesterday");
        }

        [TestMethod]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Create_description_for_item_due_in_the_last_week(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.FeedType = userFeedType;
            var items = fixture.CreateMany<UserItem>(5); // will be "last <Day of week>" up to 6 days ago, except for yesterday which is a special case
            var dueDate = DateTime.Today.AddDays(-2);
            foreach (var item in items)
            {
                item.PostponedUntilDate = null;
                item.NextDueDate = dueDate;
                dueDate = dueDate.AddDays(-1);
            }
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, items, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");

            dueDate = DateTime.Today.AddDays(-2);
            foreach (var item in items)
            {
                atomXmlString.Should().Contain($"You have an overdue item! It was due last {dueDate.ToString("dddd")}");
                dueDate = dueDate.AddDays(-1);
            }
        }

        [TestMethod]
        [DataRow(UserFeedType.Due)]
        [DataRow(UserFeedType.Overdue)]
        public void Create_description_for_item_due_over_a_week_ago(UserFeedType userFeedType)
        {
            var generator = new AtomFeedGenerator();
            var fixture = new Fixture();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.FeedType = userFeedType;
            var items = fixture.CreateMany<UserItem>(14);
            var dueDate = DateTime.Today.AddDays(-7);
            foreach (var item in items)
            {
                item.PostponedUntilDate = null;
                item.NextDueDate = dueDate;
                dueDate = dueDate.AddDays(-1);
            }
            var atomDoc = generator.GenerateFeed("https://smalllister.nosuchblogger.com", DateTime.UtcNow, items, userFeed);
            atomDoc.Should().NotBeNull();

            var atomXmlString = atomDoc.ToXmlString();
            Console.WriteLine(atomXmlString);
            atomXmlString.Should().Contain($"<id>https://smalllister.nosuchblogger.com/feed/{userFeed.UserFeedIdentifier}</id>");

            dueDate = DateTime.Today.AddDays(-7);
            foreach (var item in items)
            {
                atomXmlString.Should().Contain($"You have an overdue item! It was due on {dueDate.ToString("d MMM yyyy")}");
                dueDate = dueDate.AddDays(-1);
            }
        }
    }
}