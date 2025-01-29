using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Tests.Actions;

[TestClass]
public class UserActionsServiceTests
{
    private UserActionsService _sut;
    private Mock<IUserActionRepository> _userActionRepository;
    private Mock<IUserItemRepository> _userItemRepository;
    private Mock<IUserListRepository> _userListRepository;
    private IFixture _fixture;
    private UserAccount _user;
    private UserItem _userItem;

    [TestInitialize]
    public void Setup()
    {
        _fixture = new Fixture();
        _user = _fixture.Create<UserAccount>();
        _userItem = _fixture.Create<UserItem>();
        _userItem.DeletedDateTime = null;

        _userActionRepository = new Mock<IUserActionRepository>();
        _userItemRepository = new Mock<IUserItemRepository>();
        _userListRepository = new Mock<IUserListRepository>();
        var userActionHandlers = new IUserActionHandler<IUserAction>[] {
            new AddItemActionHandler(Mock.Of<ILogger<AddItemActionHandler>>(), _userItemRepository.Object, Mock.Of<IWebhookQueueRepository>()),
            new UpdateItemActionHandler(Mock.Of<ILogger<UpdateItemActionHandler>>(), _userItemRepository.Object, Mock.Of<IWebhookQueueRepository>()),
            new ReorderItemsActionHandler(Mock.Of<ILogger<ReorderItemsActionHandler>>(), _userItemRepository.Object, _userListRepository.Object)
        };
        _sut = new UserActionsService(Mock.Of<ILogger<UserActionsService>>(), _userActionRepository.Object, userActionHandlers);
    }

    [TestMethod]
    public async Task Should_create_action_for_add_useritem()
    {
        await _sut.AddAsync(_user, new AddItemAction(_userItem, new List<(int, int, int)>()));
        _userActionRepository.Verify(x => x.CreateAsync(_user, It.IsAny<string>(), UserActionType.AddItem, It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_create_action_for_update_useritem()
    {
        await _sut.AddAsync(_user, new UpdateItemAction(_userItem, _userItem, new List<(int, int, int)>()));
        _userActionRepository.Verify(x => x.CreateAsync(_user, It.IsAny<string>(), UserActionType.UpdateItem, It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_create_action_for_reorder_useritem()
    {
        await _sut.AddAsync(_user, new ReorderItemsAction(new List<(int, int, int)>(), ((int?)null, (ItemSortOrder?)null, (ItemSortOrder?)null)));
        _userActionRepository.Verify(x => x.CreateAsync(_user, It.IsAny<string>(), UserActionType.ReorderItems, It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task Can_undo_add_action()
    {
        UserAction? addAction = null;
        _userActionRepository
            .Setup(x => x.CreateAsync(_user, It.IsAny<string>(), UserActionType.AddItem, It.IsAny<string>()))
            .Callback((UserAccount user, string description, UserActionType type, string data) =>
            {
                addAction = new UserAction
                {
                    UserAccount = user,
                    Description = description,
                    ActionType = type,
                    UserActionData = data
                };
            });
        await _sut.AddAsync(_user, new AddItemAction(_userItem, new List<(int, int, int)>()));
        addAction.Should().NotBeNull();

        _userActionRepository.Setup(x => x.GetUndoRedoActionAsync(_user)).ReturnsAsync((addAction, (UserAction?)null));
        _userItemRepository.Setup(x => x.GetItemAsync(_user, _userItem.UserItemId, false)).ReturnsAsync(_userItem);

        var beforeUndo = DateTime.UtcNow;
        var undone = await _sut.UndoAsync(_user);
        undone.Should().BeTrue();
        _userItem.DeletedDateTime.Should().BeOnOrAfter(beforeUndo).And.BeOnOrBefore(DateTime.UtcNow);
        _userItem.LastUpdateDateTime.Should().BeOnOrAfter(beforeUndo).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [TestMethod]
    public async Task Can_redo_add_action()
    {
        UserAction? addAction = null;
        _userActionRepository
            .Setup(x => x.CreateAsync(_user, It.IsAny<string>(), UserActionType.AddItem, It.IsAny<string>()))
            .Callback((UserAccount user, string description, UserActionType type, string data) =>
            {
                addAction = new UserAction
                {
                    UserAccount = user,
                    Description = description,
                    ActionType = type,
                    UserActionData = data
                };
            });
        await _sut.AddAsync(_user, new AddItemAction(_userItem, new List<(int, int, int)>()));

        _userActionRepository.Setup(x => x.GetUndoRedoActionAsync(_user)).ReturnsAsync((addAction, (UserAction?)null));
        _userItemRepository.Setup(x => x.GetItemAsync(_user, _userItem.UserItemId, false)).ReturnsAsync(_userItem);

        var beforeActionDate = DateTime.UtcNow;
        var undone = await _sut.UndoAsync(_user);
        undone.Should().BeTrue();
        _userItem.DeletedDateTime.Should().BeOnOrAfter(beforeActionDate);
        _userItem.LastUpdateDateTime.Should().BeOnOrAfter(beforeActionDate);

        _userActionRepository.Setup(x => x.GetUndoRedoActionAsync(_user)).ReturnsAsync(((UserAction?)null, addAction));
        _userListRepository.Setup(x => x.GetListAsync(_user, _userItem.UserListId ?? 0)).ReturnsAsync(_userItem.UserList);
        _userItemRepository.Setup(x => x.GetItemAsync(_user, _userItem.UserItemId, true)).ReturnsAsync(_userItem);

        beforeActionDate = DateTime.UtcNow;
        var redone = await _sut.RedoAsync(_user);
        redone.Should().BeTrue();
        _userItem.DeletedDateTime.Should().BeNull();
        _userItem.LastUpdateDateTime.Should().BeOnOrAfter(beforeActionDate);
    }
}