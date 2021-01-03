using System;
using SmallLister.Model;

namespace SmallLister.Web.Model
{
    public class UserItemModel
    {
        public int UserItemId { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public DateTime? DueDate { get; set; }
        public string Due { get; set; }
        public string DueSummary { get; set; }
        public string Repeats { get; set; }

        public UserItemModel WithDueDate(DateTime? dueDate)
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
            return this;
        }
        public UserItemModel WithRepeat(ItemRepeat? repeat)
        {
            Repeats = repeat switch
            {
                ItemRepeat.Daily => "Repeats every day",
                ItemRepeat.Weekly => "Repeats every week",
                ItemRepeat.Monthly => "Repeats every month",
                ItemRepeat.Yearly => "Repeats every year",
                _ => ""
            };
            return this;
        }
    }
}