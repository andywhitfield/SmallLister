using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers;
using SmallLister.Web.Handlers.RequestResponse;
using Xunit;

namespace SmallLister.Web.Tests.Handlers;

public class FindItemRequestHandlerTests
{
    [Fact]
    public async Task Should_find_items_by_search_string()
    {
        var fixture = new Fixture();

        var user = fixture.Create<ClaimsPrincipal>();
        var searchQuery = fixture.Create<string>();
        var userAccount = fixture.Create<UserAccount>();
        var userLists = fixture.CreateMany<UserList>().ToList();
        var userItems = fixture.CreateMany<UserItem>().ToList();

        var userAccountRepository = new Mock<IUserAccountRepository>();
        userAccountRepository.Setup(x => x.GetUserAccountAsync(user)).ReturnsAsync(userAccount);
        var userListRepository = new Mock<IUserListRepository>();
        userListRepository.Setup(x => x.GetListsAsync(userAccount)).ReturnsAsync(userLists);
        userListRepository.Setup(x => x.GetListCountsAsync(userAccount)).ReturnsAsync((0, 0, 0, 0, new Dictionary<int, int>()));
        var userItemRepository = new Mock<IUserItemRepository>();
        userItemRepository.Setup(x => x.FindItemsByQueryAsync(userAccount, searchQuery)).ReturnsAsync(userItems);

        var handler = new FindItemRequestHandler(userAccountRepository.Object, userListRepository.Object, userItemRepository.Object);
        var response = await handler.Handle(new FindItemRequest(user, searchQuery), CancellationToken.None);

        response.Should().NotBeNull();
        response.Items.Should().HaveCount(userItems.Count);
        response.Items.Select(i => i.UserItemId).Should().BeEquivalentTo(userItems.Select(i => i.UserItemId));
        response.Items.Select(i => i.Description).Should().BeEquivalentTo(userItems.Select(i => i.Description));
    }

    [Fact]
    public async Task Should_trim_search_string()
    {
        var fixture = new Fixture();
        var searchQuery = fixture.Create<string>();
        var userItemRepository = new Mock<IUserItemRepository>();
        userItemRepository.Setup(x => x.FindItemsByQueryAsync(It.IsAny<UserAccount>(), searchQuery)).ReturnsAsync(new List<UserItem>());
        var userListRepository = new Mock<IUserListRepository>();
        userListRepository.Setup(x => x.GetListsAsync(It.IsAny<UserAccount>())).ReturnsAsync(new List<UserList>());
        var handler = new FindItemRequestHandler(Mock.Of<IUserAccountRepository>(), userListRepository.Object, userItemRepository.Object);
        var response = await handler.Handle(new FindItemRequest(fixture.Create<ClaimsPrincipal>(), $"     {searchQuery} "), CancellationToken.None);

        response.Should().NotBeNull();
        userItemRepository.Verify(x => x.FindItemsByQueryAsync(It.IsAny<UserAccount>(), searchQuery), Times.Once);
    }
}