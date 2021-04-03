namespace SmallLister.Data
{
    public class UserItemFilter
    {
        public bool Overdue { get; set; }
        public bool DueToday { get; set; }
        public bool WithDueDate { get; set; }
    }
}