using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Tests.Handlers;

[TestClass]
public class EditItemRequestHandlerTests
{
#pragma warning disable CS8618

    private EditItemRequestHandler _handler;
    private UserItem _userItem;
    private UserItem _updatedUserItemInfo;
    private Fixture _fixture;
    private ClaimsPrincipal _user;
    private UserAccount _userAccount;
#pragma warning restore CS8618
    private readonly Mock<IUserItemRepository> _userItemRepository = new();
    private readonly Mock<IUserListRepository> _userListRepository = new();

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture();
        _user = _fixture.Create<ClaimsPrincipal>();
        _userAccount = _fixture.Create<UserAccount>();
        _userAccount.LastSelectedUserListId = null;
        _userItem = _fixture.Create<UserItem>();
        _userItem.NextDueDate = _userItem.NextDueDate?.Date;
        _updatedUserItemInfo = _fixture.Create<UserItem>();

        var userAccountRepository = new Mock<IUserAccountRepository>();

        userAccountRepository.Setup(x => x.GetUserAccountAsync(_user)).ReturnsAsync(_userAccount);
        _userItemRepository.Setup(x => x.GetItemAsync(_userAccount, _userItem.UserItemId, false)).ReturnsAsync(_userItem);
        _userListRepository.Setup(x => x.GetListAsync(_userAccount, _updatedUserItemInfo.UserListId ?? 0)).ReturnsAsync(_updatedUserItemInfo.UserList);

        _handler = new EditItemRequestHandler(Mock.Of<ILogger<EditItemRequestHandler>>(),
            userAccountRepository.Object, _userItemRepository.Object, _userListRepository.Object,
            Mock.Of<IUserActionsService>(), Mock.Of<IWebhookQueueRepository>());
        // TODO: tests around the IUserActionsService
    }

    [TestMethod]
    public async Task Should_edit_item()
    {
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _updatedUserItemInfo.NextDueDate.GetValueOrDefault().Date &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    public async Task When_editing_item_Should_reset_postponed_date()
    {
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _updatedUserItemInfo.NextDueDate.GetValueOrDefault().Date &&
            x.PostponedUntilDate == null &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    public async Task When_snoozing_item_Should_update_other_properties_Except_due_date()
    {
        _userItem.PostponedUntilDate = null;
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat,
            Snooze = true
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _userItem.NextDueDate.GetValueOrDefault().Date &&
            x.PostponedUntilDate == _userItem.NextDueDate.GetValueOrDefault().Date.AddDays(1) &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    public async Task When_snoozing_item_Should_add_a_day_from_the_current_postponed_date()
    {
        var expectedSnoozeDate = _userItem.PostponedUntilDate.GetValueOrDefault().Date.AddDays(1);
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat,
            Snooze = true
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _userItem.NextDueDate.GetValueOrDefault().Date &&
            x.PostponedUntilDate == expectedSnoozeDate &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    public async Task Given_editing_an_item_When_not_changing_due_date_Should_not_reset_postponed_date()
    {
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _userItem.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _userItem.NextDueDate.GetValueOrDefault().Date &&
            x.PostponedUntilDate == _userItem.PostponedUntilDate &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    public async Task Given_invalid_due_date_should_not_update_item()
    {
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = "22-Jan-2021",
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeFalse();
        redir.Should().Be("");
        _userItemRepository.Verify(x => x.SaveAsync(It.IsAny<UserItem>(), It.IsAny<UserList>(), It.IsAny<IUserActionsService>()), Times.Never);
    }

    [TestMethod]
    public async Task Should_delete_item()
    {
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat,
            Delete = true
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _userItem.Description &&
            x.NextDueDate == _userItem.NextDueDate &&
            x.Notes == _userItem.Notes &&
            x.Repeat == _userItem.Repeat), null, It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(-1)]
    [DataRow(-2)]
    [DataRow(101)]
    public async Task Should_redirect_to_homepage_when_last_selected_list_is_no_specific_list_or_matches(int? lastSelectedListId)
    {
        _userAccount.LastSelectedUserListId = lastSelectedListId;
        _userItem.UserListId = 101;
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _updatedUserItemInfo.NextDueDate.GetValueOrDefault().Date &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(102)]
    public async Task Should_redirect_to_list_of_edited_item_when_last_selected_list_does_not_match(int? lastSelectedListId)
    {
        _userAccount.LastSelectedUserListId = lastSelectedListId;
        _userItem.UserListId = 101;
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/?list=101");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _updatedUserItemInfo.NextDueDate.GetValueOrDefault().Date &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
            It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    public async Task Given_a_new_list_when_editing_Should_save_with_new_list()
    {
        var testNewList = _fixture.Create<UserList>();
        _userListRepository.Setup(x => x.AddListAsync(_userAccount, "Test new list")).ReturnsAsync(testNewList).Verifiable(Times.Once);

        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = "new",
            Newlist = "Test new list",
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _updatedUserItemInfo.NextDueDate.GetValueOrDefault().Date &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), testNewList,
            It.IsAny<IUserActionsService>()), Times.Once);
        _userListRepository.Verify();
    }

    [TestMethod]
    public async Task Given_updating_to_a_new_list_When_no_name_is_provided_Should_save_to_all_list()
    {
        var (result, redir) = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = "new",
            Newlist = "      ",
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
        redir.Should().Be("~/");
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _updatedUserItemInfo.Description &&
            x.NextDueDate == _updatedUserItemInfo.NextDueDate.GetValueOrDefault().Date &&
            x.Notes == _updatedUserItemInfo.Notes &&
            x.Repeat == _updatedUserItemInfo.Repeat), null,
            It.IsAny<IUserActionsService>()), Times.Once);
        _userListRepository.Verify(x => x.AddListAsync(It.IsAny<UserAccount>(), It.IsAny<string>()), Times.Never());
    }
}
