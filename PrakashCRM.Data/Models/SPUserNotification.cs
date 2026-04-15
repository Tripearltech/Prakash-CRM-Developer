using System;
using System.Collections.Generic;

namespace PrakashCRM.Data.Models
{
    public class SPUserNotification
    {
        public string Id { get; set; }
        public string UserNo { get; set; }
        public string ActivityType { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceType { get; set; }
        public string LinkUrl { get; set; }
        public string IconClass { get; set; }
        public string IconColorClass { get; set; }
        public string CreatedOnUtc { get; set; }
        public bool IsRead { get; set; }
        public string ReadOnUtc { get; set; }
        public string SourceUserNo { get; set; }
        public string SourceUserName { get; set; }
    }

    public class SPUserNotificationFeedResponse
    {
        public List<SPUserNotification> Notifications { get; set; } = new List<SPUserNotification>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class SPNotificationReadRequest
    {
        public string Id { get; set; }
    }
}