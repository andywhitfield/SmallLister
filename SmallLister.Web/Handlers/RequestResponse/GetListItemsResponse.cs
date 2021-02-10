using System.Collections.Generic;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetListItemsResponse
    {
        public static readonly GetListItemsResponse BadRequest = new GetListItemsResponse();

        private GetListItemsResponse() => IsValid = false;
        public GetListItemsResponse(int dueAndOverdueCount, IEnumerable<UserListModel> lists, UserListModel selectedList, IEnumerable<UserItemModel> items,
            Pagination pagination, string undoAction, string redoAction)
        {
            IsValid = lists != null && items != null;
            DueAndOverdueCount = dueAndOverdueCount;
            Lists = lists;
            SelectedList = selectedList;
            Items = items;
            Pagination = pagination;
            UndoAction = undoAction;
            RedoAction = redoAction;
        }

        public bool IsValid { get; }
        public int DueAndOverdueCount { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public UserListModel SelectedList { get; }
        public IEnumerable<UserItemModel> Items { get; }
        public Pagination Pagination { get; }
        public string UndoAction { get; }
        public string RedoAction { get; }
    }
}