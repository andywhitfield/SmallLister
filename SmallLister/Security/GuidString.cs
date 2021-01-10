using System;

namespace SmallLister.Security
{
    public static class GuidString
    {
        public static string NewGuidString()
        {
            var base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace('+', '-').Replace('/', '_');
            return base64Guid.Substring(0, base64Guid.Length - 2);
        }
    }
}