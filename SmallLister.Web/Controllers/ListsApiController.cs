using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [ApiController]
    [Authorize]
    public class ListsApiController : ControllerBase
    {
        private readonly ILogger<ListsApiController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;

        public ListsApiController(ILogger<ListsApiController> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }

        [HttpPut("~/api/lists/{userListId}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Move(int userListId, MoveRequest moveRequest)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var list = await _userListRepository.GetListAsync(user, userListId);
            if (list == null)
                return NotFound();
            UserList precedingList = null;
            if (moveRequest.sortOrderPreviousListItemId != null)
            {
                precedingList = await _userListRepository.GetListAsync(user, moveRequest.sortOrderPreviousListItemId.Value);
                if (precedingList == null)
                    return NotFound();
            }
            
            await _userListRepository.UpdateOrderAsync(list, precedingList);
            return NoContent();
        }
    }
}