using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmallLister.Data;
using SmallLister.Feed;
using SmallLister.Model;
using SmallLister.Web.Handlers;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Tests.Handlers
{
    [TestClass]
    public class GetFeedRequestHandlerTests
    {
        [TestMethod]
        public async Task When_items_are_the_same_should_generate_same_xml()
        {
            var fixture = new Fixture();
            var userFeedRepository = new Mock<IUserFeedRepository>();
            var userAccountRepository = new Mock<IUserAccountRepository>();
            var userItemRepository = new Mock<IUserItemRepository>();
            var feedGenerator = new Mock<IFeedGenerator>();

            var baseUri = fixture.Create<string>();
            var feedId = fixture.Create<string>();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.LastUpdateDateTime = null;
            var userAccount = fixture.Create<UserAccount>();
            var items = fixture.CreateMany<UserItem>().ToList();
            userFeedRepository.Setup(x => x.FindByIdentifierAsync(feedId)).ReturnsAsync(userFeed);
            userAccountRepository.Setup(x => x.GetAsync(userFeed.UserAccountId)).ReturnsAsync(userAccount);
            userItemRepository.Setup(x => x.GetItemsAsync(userAccount, null, It.IsAny<UserItemFilter>(), null, null)).ReturnsAsync((items, 1, 1));
            feedGenerator.Setup(x => x.GenerateFeed(baseUri, userFeed.CreatedDateTime, items, userFeed)).Returns(new XDocument(new XElement("feed")));

            var handler = new GetFeedRequestHandler(Mock.Of<ILogger<GetFeedRequestHandler>>(), userFeedRepository.Object,
                userAccountRepository.Object, userItemRepository.Object, feedGenerator.Object);
            var result1 = await handler.Handle(new GetFeedRequest(baseUri, feedId), CancellationToken.None);
            var result2 = await handler.Handle(new GetFeedRequest(baseUri, feedId), CancellationToken.None);

            result1.Should().Be(result2);
            feedGenerator.Verify(x => x.GenerateFeed(baseUri, userFeed.CreatedDateTime, items, userFeed), Times.Exactly(2));
        }

        [TestMethod]
        public async Task When_items_are_different_should_generate_different_xml()
        {
            var fixture = new Fixture();
            var userFeedRepository = new Mock<IUserFeedRepository>();
            var userAccountRepository = new Mock<IUserAccountRepository>();
            var userItemRepository = new Mock<IUserItemRepository>();
            var feedGenerator = new Mock<IFeedGenerator>();

            var baseUri = fixture.Create<string>();
            var feedId = fixture.Create<string>();
            var userFeed = fixture.Create<UserFeed>();
            userFeed.LastUpdateDateTime = null;
            var userFeedLastUpdateDateTime = fixture.Create<DateTime>();
            var userAccount = fixture.Create<UserAccount>();
            var items1 = fixture.CreateMany<UserItem>().ToList();
            var items2 = items1.Skip(1).ToList();
            userFeedRepository.Setup(x => x.FindByIdentifierAsync(feedId)).ReturnsAsync(userFeed);
            userFeedRepository.Setup(x => x.SaveAsync(userFeed)).Callback(() => userFeed.LastUpdateDateTime = userFeedLastUpdateDateTime);
            userAccountRepository.Setup(x => x.GetAsync(userFeed.UserAccountId)).ReturnsAsync(userAccount);
            userItemRepository
                .SetupSequence(x => x.GetItemsAsync(userAccount, null, It.IsAny<UserItemFilter>(), null, null))
                .ReturnsAsync((items1, 1, 1))
                .ReturnsAsync((items2, 1, 1));
            feedGenerator.Setup(x => x.GenerateFeed(baseUri, userFeedLastUpdateDateTime, items1, userFeed)).Returns(new XDocument(new XElement("feed1")));
            feedGenerator.Setup(x => x.GenerateFeed(baseUri, userFeedLastUpdateDateTime, items2, userFeed)).Returns(new XDocument(new XElement("feed2")));

            var handler = new GetFeedRequestHandler(Mock.Of<ILogger<GetFeedRequestHandler>>(), userFeedRepository.Object,
                userAccountRepository.Object, userItemRepository.Object, feedGenerator.Object);
            var result1 = await handler.Handle(new GetFeedRequest(baseUri, feedId), CancellationToken.None);
            var result2 = await handler.Handle(new GetFeedRequest(baseUri, feedId), CancellationToken.None);

            result1.Should().NotBe(result2);
            feedGenerator.Verify(x => x.GenerateFeed(baseUri, userFeedLastUpdateDateTime, items1, userFeed), Times.Once);
            feedGenerator.Verify(x => x.GenerateFeed(baseUri, userFeedLastUpdateDateTime, items2, userFeed), Times.Once);
        }
    }
}