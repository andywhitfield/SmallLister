using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    private Mock<IUserItemRepository> _userItemRepository;

    #pragma warning restore CS8618

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture();
        _user = _fixture.Create<ClaimsPrincipal>();
        var userAccount = _fixture.Create<UserAccount>();
        _userItem = _fixture.Create<UserItem>();
        _userItem.NextDueDate = _userItem.NextDueDate?.Date;
        _updatedUserItemInfo = _fixture.Create<UserItem>();

        var userAccountRepository = new Mock<IUserAccountRepository>();
        _userItemRepository = new Mock<IUserItemRepository>();
        var userListRepository = new Mock<IUserListRepository>();

        userAccountRepository.Setup(x => x.GetUserAccountAsync(_user)).ReturnsAsync(userAccount);
        _userItemRepository.Setup(x => x.GetItemAsync(userAccount, _userItem.UserItemId, false)).ReturnsAsync(_userItem);
        userListRepository.Setup(x => x.GetListAsync(userAccount, _updatedUserItemInfo.UserListId ?? 0)).ReturnsAsync(_updatedUserItemInfo.UserList);

        _handler = new EditItemRequestHandler(Mock.Of<ILogger<EditItemRequestHandler>>(),
            userAccountRepository.Object, _userItemRepository.Object, userListRepository.Object,
            Mock.Of<IUserActionsService>(), Mock.Of<IWebhookQueueRepository>());
        // TODO: tests around the IUserActionsService
    }

    [TestMethod]
    public async Task Should_edit_item()
    {
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
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
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
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
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat,
            Snooze = true
        }), CancellationToken.None);

        result.Should().BeTrue();
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
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat,
            Snooze = true
        }), CancellationToken.None);

        result.Should().BeTrue();
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
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _userItem.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeTrue();
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
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = "22-Jan-2021",
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat
        }), CancellationToken.None);

        result.Should().BeFalse();
        _userItemRepository.Verify(x => x.SaveAsync(It.IsAny<UserItem>(), It.IsAny<UserList>(), It.IsAny<IUserActionsService>()), Times.Never);
    }

    [TestMethod]
    public async Task Should_delete_item()
    {
        var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
        {
            Description = _updatedUserItemInfo.Description ?? "",
            Due = _updatedUserItemInfo.NextDueDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
            List = _updatedUserItemInfo.UserListId.ToString(),
            Notes = _updatedUserItemInfo.Notes,
            Repeat = _updatedUserItemInfo.Repeat,
            Delete = true
        }), CancellationToken.None);
        
        result.Should().BeTrue();
        _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
            x.Description == _userItem.Description &&
            x.NextDueDate == _userItem.NextDueDate &&
            x.Notes == _userItem.Notes &&
            x.Repeat == _userItem.Repeat), null, It.IsAny<IUserActionsService>()), Times.Once);
    }
}
