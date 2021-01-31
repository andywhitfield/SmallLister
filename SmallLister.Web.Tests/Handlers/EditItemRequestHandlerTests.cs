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
using SmallLister.Web.Model.Request;
using Xunit;

namespace SmallLister.Web.Tests.Handlers
{
    public class EditItemRequestHandlerTests
    {
        private readonly EditItemRequestHandler _handler;
        private readonly UserItem _userItem;
        private readonly UserItem _updatedUserItemInfo;
        private readonly Fixture _fixture;
        private readonly ClaimsPrincipal _user;
        private readonly Mock<IUserItemRepository> _userItemRepository;

        public EditItemRequestHandlerTests()
        {
            _fixture = new Fixture();
            _user = _fixture.Create<ClaimsPrincipal>();
            var userAccount = _fixture.Create<UserAccount>();
            _userItem = _fixture.Create<UserItem>();
            _updatedUserItemInfo = _fixture.Create<UserItem>();

            var userAccountRepository = new Mock<IUserAccountRepository>();
            _userItemRepository = new Mock<IUserItemRepository>();
            var userListRepository = new Mock<IUserListRepository>();

            userAccountRepository.Setup(x => x.GetUserAccountAsync(_user)).ReturnsAsync(userAccount);
            _userItemRepository.Setup(x => x.GetItemAsync(userAccount, _userItem.UserItemId)).ReturnsAsync(_userItem);
            userListRepository.Setup(x => x.GetListAsync(userAccount, _updatedUserItemInfo.UserListId.Value)).ReturnsAsync(_updatedUserItemInfo.UserList);

            _handler = new EditItemRequestHandler(Mock.Of<ILogger<EditItemRequestHandler>>(),
                userAccountRepository.Object, _userItemRepository.Object, userListRepository.Object,
                Mock.Of<IUserActionsService>());
            // TODO: tests around the IUserActionsService
        }

        [Fact]
        public async Task Should_edit_item()
        {
            var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
            {
                Description = _updatedUserItemInfo.Description,
                Due = _updatedUserItemInfo.NextDueDate.Value.ToString("yyyy-MM-dd"),
                List = _updatedUserItemInfo.UserListId.ToString(),
                Notes = _updatedUserItemInfo.Notes,
                Repeat = _updatedUserItemInfo.Repeat
            }), CancellationToken.None);

            result.Should().BeTrue();
            _userItemRepository.Verify(x => x.SaveAsync(It.Is<UserItem>(x =>
                x.Description == _updatedUserItemInfo.Description &&
                x.NextDueDate == _updatedUserItemInfo.NextDueDate.Value.Date &&
                x.Notes == _updatedUserItemInfo.Notes &&
                x.Repeat == _updatedUserItemInfo.Repeat), _updatedUserItemInfo.UserList,
                It.IsAny<IUserActionsService>()), Times.Once);
        }

        [Fact]
        public async Task Given_invalid_due_date_should_not_update_item()
        {
            var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
            {
                Description = _updatedUserItemInfo.Description,
                Due = "22-Jan-2021",
                List = _updatedUserItemInfo.UserListId.ToString(),
                Notes = _updatedUserItemInfo.Notes,
                Repeat = _updatedUserItemInfo.Repeat
            }), CancellationToken.None);

            result.Should().BeFalse();
            _userItemRepository.Verify(x => x.SaveAsync(It.IsAny<UserItem>(), It.IsAny<UserList>(), It.IsAny<IUserActionsService>()), Times.Never);
        }

        [Fact]
        public async Task Should_delete_item()
        {
            var result = await _handler.Handle(new EditItemRequest(_user, _userItem.UserItemId, new AddOrUpdateItemRequest
            {
                Description = _updatedUserItemInfo.Description,
                Due = _updatedUserItemInfo.NextDueDate.Value.ToString("yyyy-MM-dd"),
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
}