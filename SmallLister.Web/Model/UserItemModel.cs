using System;
using SmallLister.Model;

namespace SmallLister.Web.Model;

public class UserItemModel
{
    public UserItemModel(UserItem userItem)
    {
        UserItemId = userItem.UserItemId;
        Description = userItem.Description ?? "";
        Notes = userItem.Notes;
        SetDateInfo(userItem.NextDueDate, userItem.PostponedUntilDate);
        Repeat = userItem.Repeat;

        var repeatSummary = GetRepeatSummary(userItem);
        RepeatSummary = userItem.PostponedUntilDate != null ? $"{GetPostponedSummary(userItem)}{(repeatSummary == "" ? "" : $". {repeatSummary}")}" : repeatSummary;
    }

    public int UserItemId { get; }
    public string Description { get; }
    public string? Notes { get; }
    public DateTime? DueDate { get; private set; }
    public DateTime? PostponedUntilDate { get; private set; }
    public string? Due { get; private set; }
    public string? DueSummary { get; private set; }
    public ItemRepeat? Repeat { get; }
    public string RepeatSummary { get; }

    public string AppendDueCssClass(string cssClass)
    {
        if (DueDate == null)
            return cssClass;

        var today = DateTime.Today;
        var due = PostponedUntilDate ?? DueDate;
        if (due == today)
            return $"{cssClass} sml-list-item-due";
        if (due < today)
            return $"{cssClass} sml-list-item-overdue";
        return cssClass;
    }

    private string GetRepeatSummary(UserItem userItem) => userItem.Repeat switch
    {
        ItemRepeat.Daily => "Repeats every day",
        ItemRepeat.Weekly => "Repeats every week",
        ItemRepeat.Monthly => "Repeats every month",
        ItemRepeat.Yearly => "Repeats every year",
        ItemRepeat.DailyExcludingWeekend => "Repeats every day, except Saturday and Sunday",
        ItemRepeat.Weekends => "Repeats every Saturday and Sunday",
        ItemRepeat.Biweekly => "Repeats every 2 weeks",
        ItemRepeat.Triweekly => "Repeats every 3 weeks",
        ItemRepeat.FourWeekly => "Repeats every 4 weeks",
        ItemRepeat.LastDayMonthly => "Repeats on the last day of every month",
        ItemRepeat.SixWeekly => "Repeats every 6 weeks",
        ItemRepeat.BiMonthly => "Repeats every 2 months",
        ItemRepeat.Quarterly => "Repeats every 3 months",
        ItemRepeat.HalfYearly => "Repeats every 6 months",
        _ => ""
    };

    private string GetPostponedSummary(UserItem userItem) =>
        $"Postponed from {GetDueDateAndSummary(userItem.NextDueDate).DueSummary}";

    private void SetDateInfo(DateTime? dueDate, DateTime? postponedUntilDate)
    {
        DueDate = dueDate;
        PostponedUntilDate = postponedUntilDate;
        var (due, dueSummary) = GetDueDateAndSummary(postponedUntilDate ?? dueDate);
        Due = string.IsNullOrEmpty(due) ? due : $"Due {due}";
        DueSummary = string.IsNullOrEmpty(dueSummary) ? dueSummary : $"Due {dueSummary}";
    }

    private (string Due, string DueSummary) GetDueDateAndSummary(DateTime? dueDate)
    {
        if (dueDate == null)
        {
            return ("", "");
        }

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

        return (due.ToString("dd MMMM yyyy"), friendlyDueDate);
    }
}