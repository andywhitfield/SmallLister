using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Tests.Handlers;

[TestClass]
public class GetListItemsRequestHandlerTests
{
    #pragma warning disable CS8618

    private GetListItemsRequestHandler _handler;
    private UserAccount _userAccount;
    private List<UserList> _userLists;
    private List<UserItem> _userItems;
    private ClaimsPrincipal _user;
    private Mock<IUserItemRepository> _userItemRepository;

    #pragma warning restore CS8618

    [TestInitialize]
    public void Setup()
    {
        var fixture = new Fixture();

        _user = fixture.Create<ClaimsPrincipal>();
        var searchQuery = fixture.Create<string>();
        _userAccount = fixture.Create<UserAccount>();
        _userLists = fixture.CreateMany<UserList>().ToList();
        _userItems = fixture.CreateMany<UserItem>().ToList();

        var userAccountRepository = new Mock<IUserAccountRepository>();
        userAccountRepository.Setup(x => x.GetUserAccountAsync(_user)).ReturnsAsync(_userAccount);
        var userListRepository = new Mock<IUserListRepository>();
        userListRepository.Setup(x => x.GetListsAsync(_userAccount)).ReturnsAsync(_userLists);
        userListRepository.Setup(x => x.GetListCountsAsync(_userAccount)).ReturnsAsync((0, 0, 0, 0, new Dictionary<int, int>()));
        _userItemRepository = new Mock<IUserItemRepository>();
        _userItemRepository.Setup(x => x.GetItemsAsync(_userAccount, null, null, null, null)).ReturnsAsync((_userItems, 1, 1));

        _handler = new GetListItemsRequestHandler(
            userAccountRepository.Object, userListRepository.Object, _userItemRepository.Object, Mock.Of<IUserActionsService>(),
            Mock.Of<IUserActionRepository>());
    }

    [TestMethod]
    public async Task Should_get_all_items()
    {
        var response = await _handler.Handle(new GetListItemsRequest(_user, null, null, null), CancellationToken.None);

        response.Should().NotBeNull();
        response.IsValid.Should().BeTrue();
        response.Items.Should().HaveCount(_userItems.Count);
        response.Items.Select(i => i.UserItemId).Should().BeEquivalentTo(_userItems.Select(i => i.UserItemId));
        response.Items.Select(i => i.Description).Should().BeEquivalentTo(_userItems.Select(i => i.Description));
        response.SelectedList?.UserListId.Should().Be("all");
    }

    [TestMethod]
    public async Task Should_get_items_from_last_selected_list()
    {
        var lastSelectedList = _userLists.First();
        _userAccount.LastSelectedUserListId = lastSelectedList.UserListId;
        var itemsOnList = _userItems.Skip(1).ToList();
        _userItemRepository.Setup(x => x.GetItemsAsync(_userAccount, lastSelectedList, null, null, null)).ReturnsAsync((itemsOnList, 1, 1));
        var response = await _handler.Handle(new GetListItemsRequest(_user, null, null, null), CancellationToken.None);

        response.Should().NotBeNull();
        response.IsValid.Should().BeTrue();
        response.Items.Should().HaveCount(itemsOnList.Count);
        response.Items.Select(i => i.UserItemId).Should().BeEquivalentTo(itemsOnList.Select(i => i.UserItemId));
        response.Items.Select(i => i.Description).Should().BeEquivalentTo(itemsOnList.Select(i => i.Description));
        response.SelectedList?.UserListId.Should().Be(lastSelectedList.UserListId.ToString());
    }

    [TestMethod]
    public async Task Should_get_items_on_requested_list()
    {
        var requestedList = _userLists.First();
        var itemsOnList = _userItems.Skip(1).ToList();
        _userItemRepository.Setup(x => x.GetItemsAsync(_userAccount, requestedList, null, null, null)).ReturnsAsync((itemsOnList, 1, 1));
        var response = await _handler.Handle(new GetListItemsRequest(_user, requestedList.UserListId.ToString(), null, null), CancellationToken.None);

        response.Should().NotBeNull();
        response.IsValid.Should().BeTrue();
        response.Items.Should().HaveCount(itemsOnList.Count);
        response.Items.Select(i => i.UserItemId).Should().BeEquivalentTo(itemsOnList.Select(i => i.UserItemId));
        response.Items.Select(i => i.Description).Should().BeEquivalentTo(itemsOnList.Select(i => i.Description));
        response.SelectedList?.UserListId.Should().Be(requestedList.UserListId.ToString());
    }
}