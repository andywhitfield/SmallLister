namespace SmallLister.Model;

// Note, the order is important as the int value is stored in the DB, so any additions (or deletions) need to preserve the enum int value
public enum ItemRepeat
{
    Daily,
    Weekly,
    Monthly,
    Yearly,
    DailyExcludingWeekend,
    Weekends,
    Biweekly,
    Triweekly,
    FourWeekly,
    LastDayMonthly,
    SixWeekly,
    BiMonthly,
    Quarterly,
    HalfYearly,
    EveryOtherDay,
    EveryThreeDays,
    FiveWeekly
}