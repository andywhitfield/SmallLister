using System;
using SmallLister.Model;

namespace SmallLister.Web.Model
{
    public class UserItemModel
    {
        public UserItemModel(UserItem userItem)
        {
            UserItemId = userItem.UserItemId;
            Description = userItem.Description;
            Notes = userItem.Notes;
            SetDueDate(userItem.NextDueDate);
            Repeat = userItem.Repeat;
            RepeatSummary = userItem.Repeat switch
            {
                ItemRepeat.Daily => "Repeats every day",
                ItemRepeat.Weekly => "Repeats every week",
                ItemRepeat.Monthly => "Repeats every month",
                ItemRepeat.Yearly => "Repeats every year",
                _ => ""
            };
        }

        public int UserItemId { get; }
        public string Description { get; }
        public string Notes { get; }
        public DateTime? DueDate { get; private set; }
        public string Due { get; private set; }
        public string DueSummary { get; private set; }
        public ItemRepeat? Repeat { get; }
        public string RepeatSummary { get; }

        public string AppendDueCssClass(string cssClass)
        {
            if (DueDate == null)
                return cssClass;
            
            var today = DateTime.Today;
            if (DueDate == today)
                return $"{cssClass} sml-list-item-due";
            if (DueDate < today)
                return $"{cssClass} sml-list-item-overdue";
            return cssClass;
        }

        private void SetDueDate(DateTime? dueDate)
        {
            DueDate = dueDate;
            if (dueDate == null)
            {
                Due = DueSummary = "";
            }
            else
            {
                string friendlyDueDate;
                var due = dueDate.Value.Date;
                var today = DateTime.Today;
                if (due == today)
                {
                    friendlyDueDate = "today";
                }
                else if (due < today)
                {
                    var days = (today - due).TotalDays;
                    if (days == 1)
                        friendlyDueDate = "yesterday";
                    else if (days < 7)
                        friendlyDueDate = $"last {due:dddd}";
                    else if (days < 180)
                        friendlyDueDate = due.ToString("d MMM");
                    else
                        friendlyDueDate = due.ToString("d MMM yyyy");
                }
                else
                {
                    var days = (due - today).TotalDays;
                    if (days == 1)
                        friendlyDueDate = "tomorrow";
                    else if (days < 7)
                        friendlyDueDate = due.ToString("dddd");
                    else if (days < 180)
                        friendlyDueDate = due.ToString("d MMM");
                    else
                        friendlyDueDate = due.ToString("d MMM yyyy");
                }
                Due = $"Due {due:dd MMMM yyyy}";
                DueSummary = $"Due {friendlyDueDate}";
            }
        }
    }
}