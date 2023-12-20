using System;
using SmallLister.Model;

namespace SmallLister.Actions.Serialization;

public class UserItemDataModel
{
    public UserItemDataModel() { }
    public UserItemDataModel(UserItem userItem)
    {
        UserItemId = userItem.UserItemId;
        UserAccountId = userItem.UserAccountId;
        UserListId = userItem.UserListId;
        Description = userItem.Description;
        Notes = userItem.Notes;
        NextDueDate = userItem.NextDueDate;
        PostponedUntilDate = userItem.PostponedUntilDate;
        Repeat = userItem.Repeat;
        SortOrder = userItem.SortOrder;
        CompletedDateTime = userItem.CompletedDateTime;
        DeletedDateTime = userItem.DeletedDateTime;
    }

    public int UserItemId { get; set; }
    public int UserAccountId { get; set; }
    public int? UserListId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? NextDueDate { get; set; }
    public DateTime? PostponedUntilDate { get; set; }
    public ItemRepeat? Repeat { get; set; }
    public int SortOrder { get; set; }
    public DateTime? CompletedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}