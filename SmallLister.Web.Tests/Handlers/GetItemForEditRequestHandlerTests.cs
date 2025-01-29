using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Tests.Handlers;

[TestClass]
public class GetItemForEditRequestHandlerTests
{
    #pragma warning disable CS8618

    private GetItemForEditRequestHandler _handler;
    private ClaimsPrincipal _user;
    private UserItem _userItem;
    private List<UserList> _userLists;

    #pragma warning restore CS8618

    [TestInitialize]
    public void Setup()
    {
        var userAccountRepository = new Mock<IUserAccountRepository>();
        var userListRepository = new Mock<IUserListRepository>();
        var userItemRepository = new Mock<IUserItemRepository>();

        var fixture = new Fixture();
        _user = fixture.Create<ClaimsPrincipal>();
        var userAccount = fixture.Create<UserAccount>();
        _userItem = fixture.Create<UserItem>();
        _userLists = fixture.CreateMany<UserList>().ToList();

        userAccountRepository.Setup(x => x.GetUserAccountAsync(_user)).ReturnsAsync(userAccount);
        userItemRepository.Setup(x => x.GetItemAsync(userAccount, _userItem.UserItemId, false)).ReturnsAsync(_userItem);
        userListRepository.Setup(x => x.GetListsAsync(userAccount)).ReturnsAsync(_userLists);

        _handler = new GetItemForEditRequestHandler(Mock.Of<ILogger<GetItemForEditRequestHandler>>(),
            userAccountRepository.Object, userListRepository.Object, userItemRepository.Object);
    }

    [TestMethod]
    public async Task Selected_list_should_default_to_all()
    {
        var response = await _handler.Handle(new GetItemForEditRequest(_user, _userItem.UserItemId), CancellationToken.None);

        response.Should().NotBeNull();
        response.IsValid.Should().BeTrue();

        response.UserItem?.UserItemId.Should().Be(_userItem.UserItemId);

        response.SelectedList.Should().NotBeNull();
        response.SelectedList?.UserListId.Should().Be(IndexViewModel.AllList);

        response.Lists.Should().HaveCount(_userLists.Count + 1);
        response.Lists.First().Name.Should().Be("All");
        response.Lists.Skip(1).Select(l => l.Name).Should().BeEquivalentTo(_userLists.Select(l => l.Name));
    }

    [TestMethod]
    public async Task Selected_list_should_be_list_of_requested_item()
    {
        _userItem.UserList = _userLists.First();
        _userItem.UserListId = _userItem.UserList.UserListId;
        var response = await _handler.Handle(new GetItemForEditRequest(_user, _userItem.UserItemId), CancellationToken.None);

        response.Should().NotBeNull();
        response.IsValid.Should().BeTrue();

        response.UserItem?.UserItemId.Should().Be(_userItem.UserItemId);

        response.SelectedList.Should().NotBeNull();
        response.SelectedList?.UserListId.Should().Be(_userItem.UserListId.ToString());

        response.Lists.Should().HaveCount(_userLists.Count + 1);
        response.Lists.First().Name.Should().Be("All");
        response.Lists.Skip(1).Select(l => l.Name).Should().BeEquivalentTo(_userLists.Select(l => l.Name));
    }
}