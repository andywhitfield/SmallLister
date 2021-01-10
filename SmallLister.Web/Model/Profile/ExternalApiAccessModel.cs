using System;

namespace SmallLister.Web.Model.Profile
{
    public class ExternalApiAccessModel
    {
        public int UserAccountApiAccessId { get; }
        public string DisplayName { get; }
        public string ApprovedDate { get; }
        public string LastAccessedDate { get; }
        public DateTime LastAccessedDateTime { get; }
        public bool IsRevoked { get; }
        public string RevokedDate { get; }
        public DateTime? RevokedDateTime { get; }
        public ExternalApiAccessModel(int userAccountApiAccessId, string displayName, DateTime approvedDate, DateTime lastAccessedDate, DateTime? revokedDate)
        {
            UserAccountApiAccessId = userAccountApiAccessId;
            DisplayName = displayName;
            ApprovedDate = approvedDate.ToString("R");
            LastAccessedDate = lastAccessedDate.ToString("R");
            LastAccessedDateTime = lastAccessedDate;
            IsRevoked = revokedDate != null;
            RevokedDateTime = revokedDate;
            RevokedDate = revokedDate?.ToString("R") ?? "";
        }
    }
}