using MediatR;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class MarkItemAsDoneRequestHandler(
    ILogger<MarkItemAsDoneRequestHandler> logger,
    IUserAccountRepository userAccountRepository,
    IUserItemRepository userItemRepository,
    IUserListRepository userListRepository,
    IUserActionsService userActionsService,
    IWebhookQueueRepository webhookQueueRepository)
    : IRequestHandler<MarkItemAsDoneRequest, bool>
{
    public async Task<bool> Handle(MarkItemAsDoneRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        var item = await userItemRepository.GetItemAsync(user, request.UserItemId);
        if (item == null)
        {
            logger.LogInformation("Could not find item {UserItemId}", request.UserItemId);
            return false;
        }

        var list = item.UserListId != null ? await userListRepository.GetListAsync(user, item.UserListId.Value) : null;
        if (item.NextDueDate == null || item.Repeat == null)
            item.CompletedDateTime = DateTime.UtcNow;
        else
            item.NextDueDate = CalculateNextDueDate(item.NextDueDate.Value, item.Repeat.Value);
        item.PostponedUntilDate = null;

        await userItemRepository.SaveAsync(item, list, userActionsService);
        await webhookQueueRepository.OnListItemChangeAsync(user, item, WebhookEventType.Modify);
        return true;
    }

    private static DateTime CalculateNextDueDate(DateTime dueDate, ItemRepeat repeat) =>
        repeat switch
        {
            ItemRepeat.Daily => dueDate.AddDays(1),
            ItemRepeat.EveryOtherDay => dueDate.AddDays(2),
            ItemRepeat.EveryThreeDays => dueDate.AddDays(3),
            ItemRepeat.Weekly => dueDate.AddDays(7),
            ItemRepeat.Monthly => dueDate.AddMonths(1),
            ItemRepeat.Yearly => dueDate.AddYears(1),
            ItemRepeat.DailyExcludingWeekend => NextWorkingDay(dueDate),
            ItemRepeat.Weekends => NextWeekendDay(dueDate),
            ItemRepeat.Biweekly => dueDate.AddDays(14),
            ItemRepeat.Triweekly => dueDate.AddDays(21),
            ItemRepeat.FourWeekly => dueDate.AddDays(28),
            ItemRepeat.FiveWeekly => dueDate.AddDays(35),
            ItemRepeat.LastDayMonthly => NextLastDayOfTheMonth(dueDate),
            ItemRepeat.SixWeekly => dueDate.AddDays(42),
            ItemRepeat.BiMonthly => dueDate.AddMonths(2),
            ItemRepeat.Quarterly => dueDate.AddMonths(3),
            ItemRepeat.HalfYearly => dueDate.AddMonths(6),
            _ => throw new ArgumentException($"Unsupported repeat option: {repeat}")
        };

    private static DateTime NextWorkingDay(DateTime date)
    {
        do
            date = date.AddDays(1);
        while (IsWeekend(date));
        return date;
    }

    private static DateTime NextWeekendDay(DateTime date)
    {
        do
            date = date.AddDays(1);
        while (!IsWeekend(date));
        return date;
    }

    private static DateTime NextLastDayOfTheMonth(DateTime date)
    {
        date = date.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        if (date.Day != daysInMonth)
            date = date.AddDays(daysInMonth - date.Day);
        return date;
    }

    private static bool IsWeekend(DateTime date) => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
}