using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers;
using SmallLister.Web.Handlers.RequestResponse;
using Xunit;

namespace SmallLister.Web.Tests.Handlers;

public class SnoozeRequestHandlerTest
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ClaimsPrincipal _user;
    private readonly UserItem _userItem;
    private readonly Mock<IUserItemRepository> _userItemRepository;
    private readonly SnoozeRequestHandler _handler;

    public SnoozeRequestHandlerTest()
    {
        _user = _fixture.Create<ClaimsPrincipal>();
        _userItem = _fixture.Create<UserItem>();

        var userAccountRepository = new Mock<IUserAccountRepository>();
        _userItemRepository = new Mock<IUserItemRepository>();
        var userListRepository = new Mock<IUserListRepository>();
        var userActionsService = new Mock<IUserActionsService>();

        _userItemRepository.Setup(x => x.GetItemAsync(It.IsAny<UserAccount>(), _userItem.UserItemId, false)).ReturnsAsync(_userItem);

        _handler = new SnoozeRequestHandler(Mock.Of<ILogger<SnoozeRequestHandler>>(), userAccountRepository.Object,
            _userItemRepository.Object, userListRepository.Object, userActionsService.Object, Mock.Of<IWebhookQueueRepository>());
    }

    [Fact]
    public async Task Given_item_with_no_due_date_Should_not_set_postponed_date()
    {
        _userItem.NextDueDate = null;
        var result = await _handler.Handle(new SnoozeRequest(_user, _userItem.UserItemId), CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_postpone_item_by_a_day()
    {
        _userItem.PostponedUntilDate = null;
        var result = await _handler.Handle(new SnoozeRequest(_user, _userItem.UserItemId), CancellationToken.None);
        result.Should().BeTrue();
        _userItem.PostponedUntilDate.Should().Be(_userItem.NextDueDate.GetValueOrDefault().AddDays(1));
    }

    [Fact]
    public async Task Given_a_postponed_item_Should_postpone_item_by_another_day()
    {
        var currentPostponedDate = _userItem.PostponedUntilDate.GetValueOrDefault();
        var result = await _handler.Handle(new SnoozeRequest(_user, _userItem.UserItemId), CancellationToken.None);
        result.Should().BeTrue();
        _userItem.PostponedUntilDate.Should().Be(currentPostponedDate.AddDays(1));
    }
}