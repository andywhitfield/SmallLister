using System;
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

namespace SmallLister.Web.Tests.Handlers;

[TestClass]
public class MarkItemAsDoneRequestHandlerTests
{
    #pragma warning disable CS8618

    private UserItem _userItem;
    private MarkItemAsDoneRequestHandler _handler;
    private Mock<IUserItemRepository> _userItemRepository;
    private ClaimsPrincipal _user;

    #pragma warning restore CS8618

    [TestInitialize]
    public void Setup()
    {
        var fixture = new Fixture();
        _user = fixture.Create<ClaimsPrincipal>();
        var userAccount = fixture.Create<UserAccount>();
        _userItem = fixture.Create<UserItem>();

        var userAccountRepository = new Mock<IUserAccountRepository>();
        userAccountRepository.Setup(x => x.GetUserAccountAsync(_user)).ReturnsAsync(userAccount);

        _userItemRepository = new Mock<IUserItemRepository>();
        _userItemRepository.Setup(x => x.GetItemAsync(userAccount, _userItem.UserItemId, false)).ReturnsAsync(_userItem);

        var userListRepository = new Mock<IUserListRepository>();
        _handler = new MarkItemAsDoneRequestHandler(Mock.Of<ILogger<MarkItemAsDoneRequestHandler>>(), userAccountRepository.Object,
            _userItemRepository.Object, userListRepository.Object, Mock.Of<IUserActionsService>(), Mock.Of<IWebhookQueueRepository>());
        // TODO: tests around the IUserActionsService
    }

    [TestMethod]
    public async Task Non_repeating_item_should_be_completed()
    {
        _userItem.CompletedDateTime = null;
        _userItem.Repeat = null;

        var result = await _handler.Handle(new MarkItemAsDoneRequest(_user, _userItem.UserItemId), CancellationToken.None);
        result.Should().BeTrue();

        _userItem.CompletedDateTime.Should().NotBeNull();
        _userItem.PostponedUntilDate.Should().BeNull();
        _userItemRepository.Verify(x => x.SaveAsync(_userItem, It.IsAny<UserList>(), It.IsAny<IUserActionsService>()), Times.Once);
    }

    [TestMethod]
    [DataRow("2021-01-18", ItemRepeat.Daily, "2021-01-19")]
    [DataRow("2021-01-15", ItemRepeat.DailyExcludingWeekend, "2021-01-18")]
    [DataRow("2021-01-16", ItemRepeat.DailyExcludingWeekend, "2021-01-18")]
    [DataRow("2021-01-17", ItemRepeat.DailyExcludingWeekend, "2021-01-18")]
    [DataRow("2021-01-15", ItemRepeat.Weekends, "2021-01-16")]
    [DataRow("2021-01-16", ItemRepeat.Weekends, "2021-01-17")]
    [DataRow("2021-01-17", ItemRepeat.Weekends, "2021-01-23")]
    [DataRow("2021-01-18", ItemRepeat.Weekly, "2021-01-25")]
    [DataRow("2021-01-18", ItemRepeat.Biweekly, "2021-02-01")]
    [DataRow("2021-01-18", ItemRepeat.Triweekly, "2021-02-08")]
    [DataRow("2021-01-18", ItemRepeat.FourWeekly, "2021-02-15")]
    [DataRow("2021-01-18", ItemRepeat.Monthly, "2021-02-18")]
    [DataRow("2021-01-31", ItemRepeat.Monthly, "2021-02-28")]
    [DataRow("2021-01-31", ItemRepeat.LastDayMonthly, "2021-02-28")]
    [DataRow("2021-02-28", ItemRepeat.LastDayMonthly, "2021-03-31")]
    [DataRow("2021-01-18", ItemRepeat.SixWeekly, "2021-03-01")]
    [DataRow("2021-01-18", ItemRepeat.BiMonthly, "2021-03-18")]
    [DataRow("2021-11-18", ItemRepeat.BiMonthly, "2022-01-18")]
    [DataRow("2021-01-18", ItemRepeat.Quarterly, "2021-04-18")]
    [DataRow("2021-10-18", ItemRepeat.Quarterly, "2022-01-18")]
    [DataRow("2021-01-18", ItemRepeat.HalfYearly, "2021-07-18")]
    [DataRow("2021-07-18", ItemRepeat.HalfYearly, "2022-01-18")]
    [DataRow("2021-02-28", ItemRepeat.Yearly, "2022-02-28")]
    [DataRow("2020-02-29", ItemRepeat.Yearly, "2021-02-28")]
    public async Task Repeating_item_should_update_next_due_date(string currentDueDate, ItemRepeat repeat, string expectedDueDate)
    {
        _userItem.CompletedDateTime = null;
        _userItem.NextDueDate = DateTime.ParseExact(currentDueDate, "yyyy-MM-dd", null);
        _userItem.Repeat = repeat;

        var result = await _handler.Handle(new MarkItemAsDoneRequest(_user, _userItem.UserItemId), CancellationToken.None);
        result.Should().BeTrue();

        _userItem.CompletedDateTime.Should().BeNull();
        _userItem.PostponedUntilDate.Should().BeNull();
        _userItem.NextDueDate.Should().Be(DateTime.ParseExact(expectedDueDate, "yyyy-MM-dd", null));
        _userItemRepository.Verify(x => x.SaveAsync(_userItem, It.IsAny<UserList>(), It.IsAny<IUserActionsService>()), Times.Once);
    }
}