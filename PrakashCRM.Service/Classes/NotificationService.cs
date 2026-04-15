using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace PrakashCRM.Service.Classes
{
    public class NotificationService
    {
        public const string ActivityNewInquiry = "NewInquiry";
        public const string ActivityInquiryToQuote = "InquiryToQuote";
        public const string ActivitySalesQuotePendingApproval = "SalesQuotePendingApproval";
        public const string ActivitySalesQuoteApproved = "SalesQuoteApproved";
        public const string ActivityOrderScheduled = "OrderScheduled";
        public const string ActivityInvoiceGenerated = "InvoiceGenerated";
        public const string ActivityItemPriceUpdated = "ItemPriceUpdated";

        private const int MaxNotificationsPerUser = 10000;
        private static readonly TimeSpan InvoiceSyncCooldown = TimeSpan.FromMinutes(2);
        private static readonly object FileLock = new object();
        private static readonly object InvoiceSyncTrackerLock = new object();
        private static Dictionary<string, DateTime> _invoiceSyncTracker;
        private readonly string _filePath;

        public NotificationService()
        {
            _filePath = ResolveStorePath();
        }

        public SPUserNotificationFeedResponse GetNotifications(string userNo, int skip, int top, bool includeRead, string category = "", string excludeCategory = "")
        {
            userNo = (userNo ?? string.Empty).Trim();
            SyncInvoiceNotifications(userNo);

            var allItems = GetUserNotificationsInternal(userNo, includeRead, category);

            int unreadCount;
            if (!string.IsNullOrWhiteSpace(excludeCategory))
            {
                var unreadItems = GetUserNotificationsInternal(userNo, true, category, excludeCategory);
                unreadCount = unreadItems.Count(x => !x.IsRead);
            }
            else
            {
                unreadCount = allItems.Count(x => !x.IsRead);
            }

            return new SPUserNotificationFeedResponse
            {
                Notifications = allItems.Skip(Math.Max(skip, 0)).Take(Math.Max(top, 0)).ToList(),
                UnreadCount = unreadCount,
                TotalCount = allItems.Count
            };
        }

        public bool MarkAsRead(string userNo, string id)
        {
            userNo = (userNo ?? string.Empty).Trim();
            id = (id ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(userNo) || string.IsNullOrWhiteSpace(id))
                return false;

            var userKeys = ResolveNotificationUserKeys(userNo);

            lock (FileLock)
            {
                var items = LoadNotifications();
                var target = items.FirstOrDefault(x => userKeys.Contains((x.UserNo ?? string.Empty).Trim())
                    && string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                    return false;

                target.IsRead = true;
                target.ReadOnUtc = DateTime.UtcNow.ToString("o");
                SaveNotifications(items);
                return true;
            }
        }

        public bool MarkAllAsRead(string userNo)
        {
            userNo = (userNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userNo))
                return false;

            var userKeys = ResolveNotificationUserKeys(userNo);

            lock (FileLock)
            {
                var items = LoadNotifications();
                var didUpdate = false;

                foreach (var item in items.Where(x => userKeys.Contains((x.UserNo ?? string.Empty).Trim()) && !x.IsRead))
                {
                    item.IsRead = true;
                    item.ReadOnUtc = DateTime.UtcNow.ToString("o");
                    didUpdate = true;
                }

                if (didUpdate)
                    SaveNotifications(items);

                return didUpdate;
            }
        }

        public void RecordNewInquiry(string salesPersonCode, string inquiryNo, string customerName)
        {
            var user = ResolveUserBySalespersonCode(salesPersonCode);
            var notificationUserNo = GetNotificationUserNo(user);
            if (user == null || string.IsNullOrWhiteSpace(notificationUserNo))
                return;

            AddNotification(new SPUserNotification
            {
                UserNo = notificationUserNo,
                SourceUserNo = notificationUserNo,
                SourceUserName = BuildFullName(user),
                ActivityType = ActivityNewInquiry,
                Category = "Inquiry",
                Title = "New Customer Inquiry",
                Description = BuildCustomerDescription(customerName, inquiryNo, "Inquiry"),
                ReferenceNo = inquiryNo,
                ReferenceType = "Inquiry",
                LinkUrl = BuildInquiryLink(inquiryNo),
                IconClass = "bx bx-user-plus",
                IconColorClass = "bg-light-primary text-primary"
            });
        }

        public void RecordInquiryConvertedToQuote(string salesPersonCode, string inquiryNo, string quoteNo, string customerName)
        {
            var user = ResolveUserBySalespersonCode(salesPersonCode);
            var notificationUserNo = GetNotificationUserNo(user);
            if (user == null || string.IsNullOrWhiteSpace(notificationUserNo))
                return;

            var description = string.Format(CultureInfo.InvariantCulture,
                "{0} converted to quote {1}{2}",
                string.IsNullOrWhiteSpace(inquiryNo) ? "Inquiry" : inquiryNo,
                quoteNo ?? string.Empty,
                string.IsNullOrWhiteSpace(customerName) ? string.Empty : " for " + customerName.Trim());

            AddNotification(new SPUserNotification
            {
                UserNo = notificationUserNo,
                SourceUserNo = notificationUserNo,
                SourceUserName = BuildFullName(user),
                ActivityType = ActivityInquiryToQuote,
                Category = "Quote",
                Title = "Inquiry Converted To Quote",
                Description = description,
                ReferenceNo = quoteNo,
                ReferenceType = "Quote",
                LinkUrl = BuildSalesQuoteLink(quoteNo),
                IconClass = "bx bx-cart-alt",
                IconColorClass = "bg-light-danger text-danger"
            });
        }

        public void RecordSalesQuoteStatus(string quoteNo, string salesQuoteStatus, string salesPersonCode = "", string customerName = "")
        {
            quoteNo = (quoteNo ?? string.Empty).Trim();
            salesQuoteStatus = (salesQuoteStatus ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(quoteNo))
                return;

            var normalizedStatus = salesQuoteStatus.ToLowerInvariant();
            var isApproved = normalizedStatus == "approved";
            var isPendingApproval = normalizedStatus.StartsWith("approval pending");
            if (!isApproved && !isPendingApproval)
                return;

            var quoteHeader = GetQuoteHeader(quoteNo);
            var resolvedSalesPersonCode = !string.IsNullOrWhiteSpace(salesPersonCode)
                ? salesPersonCode.Trim()
                : (quoteHeader == null ? string.Empty : (quoteHeader.Salesperson_Code ?? string.Empty).Trim());
            var resolvedCustomerName = !string.IsNullOrWhiteSpace(customerName)
                ? customerName.Trim()
                : (quoteHeader == null ? string.Empty : (quoteHeader.Sell_to_Customer_Name ?? string.Empty).Trim());

            var user = ResolveUserBySalespersonCode(resolvedSalesPersonCode);
            var notificationUserNo = GetNotificationUserNo(user);
            if (user == null || string.IsNullOrWhiteSpace(notificationUserNo))
                return;

            AddNotification(new SPUserNotification
            {
                UserNo = notificationUserNo,
                SourceUserNo = notificationUserNo,
                SourceUserName = BuildFullName(user),
                ActivityType = isApproved ? ActivitySalesQuoteApproved : ActivitySalesQuotePendingApproval,
                Category = "Quote",
                Title = isApproved ? "Sales Quote Approved" : "Sales Quote Pending Approval",
                Description = BuildSalesQuoteStatusDescription(resolvedCustomerName, quoteNo, salesQuoteStatus),
                ReferenceNo = quoteNo,
                ReferenceType = "Quote",
                LinkUrl = BuildSalesQuoteLink(quoteNo),
                IconClass = isApproved ? "bx bx-badge-check" : "bx bx-time-five",
                IconColorClass = isApproved ? "bg-light-success text-success" : "bg-light-warning text-warning"
            });
        }

        public void RecordOrderScheduled(string quoteNo, string orderNo)
        {
            var quoteHeader = GetQuoteHeader(quoteNo);
            if (quoteHeader == null || string.IsNullOrWhiteSpace(quoteHeader.Salesperson_Code))
                return;

            var salesOrder = GetSalesOrder(orderNo);
            if (salesOrder == null || (salesOrder.Status != "Open" && salesOrder.Status != "Released"))
                return;

            var user = ResolveUserBySalespersonCode(quoteHeader.Salesperson_Code);
            var notificationUserNo = GetNotificationUserNo(user);
            if (user == null || string.IsNullOrWhiteSpace(notificationUserNo))
                return;

            var description = string.Format(CultureInfo.InvariantCulture,
                "Order {0} scheduled{1}",
                orderNo ?? string.Empty,
                string.IsNullOrWhiteSpace(quoteHeader.Sell_to_Customer_Name) ? string.Empty : " for " + quoteHeader.Sell_to_Customer_Name.Trim());

            AddNotification(new SPUserNotification
            {
                UserNo = notificationUserNo,
                SourceUserNo = notificationUserNo,
                SourceUserName = BuildFullName(user),
                ActivityType = ActivityOrderScheduled,
                Category = "Order",
                Title = salesOrder.Status == "Released" ? "Sales Order Released" : "Sales Order Open",
                Description = description,
                ReferenceNo = orderNo,
                ReferenceType = "SalesOrder",
                LinkUrl = BuildSalesOrderLink(orderNo),
                IconClass = "bx bx-calendar",
                IconColorClass = "bg-light-success text-success"
            });
        }

        public void RecordItemPriceUpdated(string salesPersonCode, string itemNo, string itemDescription, string packingStyleCode, string packingStyleDescription, double? newPrice)
        {
            var user = ResolveUserBySalespersonCode(salesPersonCode);
            var notificationUserNo = GetNotificationUserNo(user);
            if (user == null || string.IsNullOrWhiteSpace(notificationUserNo) || !newPrice.HasValue)
                return;

            var normalizedPrice = newPrice.Value.ToString("0.##", CultureInfo.InvariantCulture);
            var resolvedItemName = !string.IsNullOrWhiteSpace(itemDescription) ? itemDescription.Trim() : (itemNo ?? string.Empty).Trim();
            var resolvedPackingStyle = !string.IsNullOrWhiteSpace(packingStyleDescription) ? packingStyleDescription.Trim() : (packingStyleCode ?? string.Empty).Trim();

            AddNotification(new SPUserNotification
            {
                UserNo = notificationUserNo,
                SourceUserNo = notificationUserNo,
                SourceUserName = BuildFullName(user),
                ActivityType = ActivityItemPriceUpdated + ":" + normalizedPrice,
                Category = "Item",
                Title = "Item Price Updated",
                Description = BuildItemPriceChangeDescription(resolvedItemName, resolvedPackingStyle, normalizedPrice),
                ReferenceNo = string.IsNullOrWhiteSpace(itemNo) ? resolvedItemName : itemNo.Trim(),
                ReferenceType = "Item",
                LinkUrl = BuildItemPriceChangeLink(itemNo, packingStyleCode),
                IconClass = "bx bx-rupee",
                IconColorClass = "bg-light-info text-info"
            });
        }

        public void SyncInvoiceNotifications(string userNo)
        {
            userNo = (userNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userNo))
                return;

            DateTime lastSyncUtc;
            if (TryGetLastInvoiceSyncUtc(userNo, out lastSyncUtc)
                && (DateTime.UtcNow - lastSyncUtc) < InvoiceSyncCooldown)
                return;

            var salesPersonCode = ResolveSalespersonCodeFromUserNo(userNo);
            if (string.IsNullOrWhiteSpace(salesPersonCode))
                return;

            SetLastInvoiceSyncUtc(userNo, DateTime.UtcNow);

            API ac = new API();
            var result = ac.GetData<SPPostedSalesInvoiceList>("PostedSalesInvoicesDotNetAPI", "Salesperson_Code eq '" + EscapeODataValue(salesPersonCode) + "'");
            var resultData = result.Result;
            var invoices = resultData.Item1 == null ? null : resultData.Item1.value;

            if (invoices == null || invoices.Count == 0)
                return;

            var latestInvoices = invoices
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.No))
                .OrderByDescending(x => ParseDate(x.Posting_Date))
                .ThenByDescending(x => x.No)
                .Take(25)
                .ToList();

            lock (FileLock)
            {
                var existingItems = LoadNotifications();
                var existingInvoiceNos = new HashSet<string>(
                    existingItems.Where(x => string.Equals(x.UserNo, userNo, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.ActivityType, ActivityInvoiceGenerated, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(x.ReferenceNo))
                    .Select(x => x.ReferenceNo),
                    StringComparer.OrdinalIgnoreCase);

                var didChange = false;
                foreach (var invoice in latestInvoices)
                {
                    if (existingInvoiceNos.Contains(invoice.No))
                        continue;

                    existingItems.Add(new SPUserNotification
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        UserNo = userNo,
                        SourceUserNo = userNo,
                        SourceUserName = userNo,
                        ActivityType = ActivityInvoiceGenerated,
                        Category = "Invoice",
                        Title = "Invoice Generated",
                        Description = BuildCustomerDescription(invoice.Sell_to_Customer_Name, invoice.No, "Invoice"),
                        ReferenceNo = invoice.No,
                        ReferenceType = "Invoice",
                        LinkUrl = "/SPPostedSalesInvoice/PostedSalesInvoiceList",
                        IconClass = "bx bx-file",
                        IconColorClass = "bg-light-warning text-warning",
                        CreatedOnUtc = NormalizeTimestamp(invoice.Posting_Date),
                        IsRead = false,
                        ReadOnUtc = string.Empty
                    });

                    existingInvoiceNos.Add(invoice.No);
                    didChange = true;
                }

                if (didChange)
                {
                    TrimNotifications(existingItems);
                    SaveNotifications(existingItems);
                }
            }
        }

        private void AddNotification(SPUserNotification notification)
        {
            if (notification == null || string.IsNullOrWhiteSpace(notification.UserNo))
                return;

            lock (FileLock)
            {
                var items = LoadNotifications();
                bool isDuplicate = items.Any(x => string.Equals(x.UserNo, notification.UserNo, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.ActivityType, notification.ActivityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.ReferenceNo ?? string.Empty, notification.ReferenceNo ?? string.Empty, StringComparison.OrdinalIgnoreCase));

                if (isDuplicate)
                    return;

                notification.Id = Guid.NewGuid().ToString("N");
                notification.CreatedOnUtc = string.IsNullOrWhiteSpace(notification.CreatedOnUtc)
                    ? DateTime.UtcNow.ToString("o")
                    : NormalizeTimestamp(notification.CreatedOnUtc);
                notification.IsRead = false;
                notification.ReadOnUtc = string.Empty;

                items.Add(notification);
                TrimNotifications(items);
                SaveNotifications(items);
            }
        }

        private List<SPUserNotification> GetUserNotificationsInternal(string userNo, bool includeRead, string category, string excludeCategory = "")
        {
            userNo = (userNo ?? string.Empty).Trim();
            category = (category ?? string.Empty).Trim();
            excludeCategory = (excludeCategory ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userNo))
                return new List<SPUserNotification>();

            var userKeys = ResolveNotificationUserKeys(userNo);

            lock (FileLock)
            {
                return LoadNotifications()
                    .Where(x => userKeys.Contains((x.UserNo ?? string.Empty).Trim())
                        && (includeRead || !x.IsRead)
                        && (string.IsNullOrWhiteSpace(category) || string.Equals(x.Category ?? string.Empty, category, StringComparison.OrdinalIgnoreCase))
                        && (string.IsNullOrWhiteSpace(excludeCategory) || !string.Equals(x.Category ?? string.Empty, excludeCategory, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(x => ParseDate(x.CreatedOnUtc))
                    .ThenByDescending(x => x.Id)
                    .ToList();
            }
        }

        private HashSet<string> ResolveNotificationUserKeys(string userNo)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            userNo = (userNo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(userNo))
                keys.Add(userNo);

            var profile = ResolveUserByNotificationIdentity(userNo);
            if (profile != null)
            {
                if (!string.IsNullOrWhiteSpace(profile.No))
                    keys.Add(profile.No.Trim());

                if (!string.IsNullOrWhiteSpace(profile.PCPL_Employee_Code))
                    keys.Add(profile.PCPL_Employee_Code.Trim());
            }

            return keys;
        }

        private List<SPUserNotification> LoadNotifications()
        {
            try
            {
                EnsureStoreExists();
                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<SPUserNotification>();

                var items = JsonConvert.DeserializeObject<List<SPUserNotification>>(json);
                return items ?? new List<SPUserNotification>();
            }
            catch
            {
                return new List<SPUserNotification>();
            }
        }

        private void SaveNotifications(List<SPUserNotification> items)
        {
            EnsureStoreExists();
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(items ?? new List<SPUserNotification>(), Formatting.Indented));
        }

        private void EnsureStoreExists()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private static void TrimNotifications(List<SPUserNotification> items)
        {
            var trimmedItems = items
                .GroupBy(x => x.UserNo ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .SelectMany(group => group
                    .OrderByDescending(x => ParseDate(x.CreatedOnUtc))
                    .ThenByDescending(x => x.Id)
                    .Take(MaxNotificationsPerUser))
                .ToList();

            items.Clear();
            items.AddRange(trimmedItems);
        }

        private static string ResolveStorePath()
        {
            var mappedPath = HostingEnvironment.MapPath("~/App_Data/user-notifications.json");
            if (!string.IsNullOrWhiteSpace(mappedPath))
                return mappedPath;

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "user-notifications.json");
        }

        private static string BuildCustomerDescription(string customerName, string referenceNo, string prefix)
        {
            if (!string.IsNullOrWhiteSpace(customerName) && !string.IsNullOrWhiteSpace(referenceNo))
                return string.Format(CultureInfo.InvariantCulture, "{0} {1} for {2}", prefix, referenceNo, customerName.Trim());

            if (!string.IsNullOrWhiteSpace(referenceNo))
                return string.Format(CultureInfo.InvariantCulture, "{0} {1}", prefix, referenceNo);

            return customerName ?? prefix;
        }

        private static string BuildFullName(SPProfile profile)
        {
            if (profile == null)
                return string.Empty;

            return string.Join(" ", new[] { profile.First_Name, profile.Middle_Name, profile.Last_Name }
                .Where(value => !string.IsNullOrWhiteSpace(value))).Trim();
        }

        private static string BuildSalesQuoteStatusDescription(string customerName, string quoteNo, string salesQuoteStatus)
        {
            if (!string.IsNullOrWhiteSpace(customerName) && !string.IsNullOrWhiteSpace(quoteNo))
                return string.Format(CultureInfo.InvariantCulture, "Sales Quote {0} for {1} is {2}", quoteNo, customerName.Trim(), salesQuoteStatus);

            if (!string.IsNullOrWhiteSpace(quoteNo))
                return string.Format(CultureInfo.InvariantCulture, "Sales Quote {0} is {1}", quoteNo, salesQuoteStatus);

            return "Sales Quote is " + salesQuoteStatus;
        }

        private static string BuildItemPriceChangeDescription(string itemDescription, string packingStyleDescription, string newPrice)
        {
            if (!string.IsNullOrWhiteSpace(itemDescription) && !string.IsNullOrWhiteSpace(packingStyleDescription))
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1}), new price is {2}", itemDescription.Trim(), packingStyleDescription.Trim(), newPrice);

            if (!string.IsNullOrWhiteSpace(itemDescription))
                return string.Format(CultureInfo.InvariantCulture, "{0}, new price is {1}", itemDescription.Trim(), newPrice);

            return string.Format(CultureInfo.InvariantCulture, "Item price updated to {0}", newPrice);
        }

        private static Dictionary<string, DateTime> GetInvoiceSyncTracker()
        {
            lock (InvoiceSyncTrackerLock)
            {
                if (_invoiceSyncTracker == null)
                    _invoiceSyncTracker = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

                return _invoiceSyncTracker;
            }
        }

        private static bool TryGetLastInvoiceSyncUtc(string userNo, out DateTime lastSyncUtc)
        {
            lock (InvoiceSyncTrackerLock)
            {
                return GetInvoiceSyncTracker().TryGetValue(userNo, out lastSyncUtc);
            }
        }

        private static void SetLastInvoiceSyncUtc(string userNo, DateTime lastSyncUtc)
        {
            lock (InvoiceSyncTrackerLock)
            {
                GetInvoiceSyncTracker()[userNo] = lastSyncUtc;
            }
        }

        private static string BuildInquiryLink(string inquiryNo)
        {
            return string.IsNullOrWhiteSpace(inquiryNo)
                ? "/SPInquiry/InquiryList"
                : "/SPInquiry/Inquiry?InquiryNo=" + HttpUtility.UrlEncode(inquiryNo.Trim());
        }

        private static string BuildSalesQuoteLink(string quoteNo)
        {
            return string.IsNullOrWhiteSpace(quoteNo)
                ? "/SPSalesQuotes/SalesQuoteList"
                : "/SPSalesQuotes/SalesQuoteList?notificationQuoteNo=" + HttpUtility.UrlEncode(quoteNo.Trim());
        }

        private static string BuildSalesOrderLink(string orderNo)
        {
            return string.IsNullOrWhiteSpace(orderNo)
                ? "/SPSalesOrders/SalesOrdersList"
                : "/SPSalesOrders/SalesOrdersList?notificationOrderNo=" + HttpUtility.UrlEncode(orderNo.Trim());
        }

        private static string BuildItemPriceChangeLink(string itemNo, string packingStyleCode)
        {
            var url = "/SPItems/ItemPriceChange";
            var queryParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(itemNo))
                queryParts.Add("itemNo=" + HttpUtility.UrlEncode(itemNo.Trim()));

            if (!string.IsNullOrWhiteSpace(packingStyleCode))
                queryParts.Add("packingStyle=" + HttpUtility.UrlEncode(packingStyleCode.Trim()));

            return queryParts.Count == 0 ? url : url + "?" + string.Join("&", queryParts);
        }

        private static string GetNotificationUserNo(SPProfile profile)
        {
            if (profile == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(profile.No))
                return profile.No.Trim();

            if (!string.IsNullOrWhiteSpace(profile.PCPL_Employee_Code))
                return profile.PCPL_Employee_Code.Trim();

            return string.Empty;
        }

        private SPProfile ResolveUserBySalespersonCode(string salesPersonCode)
        {
            salesPersonCode = (salesPersonCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(salesPersonCode))
                return null;

            API ac = new API();
            var candidates = new[]
            {
                salesPersonCode,
                salesPersonCode.Contains(",") ? salesPersonCode.Split(',')[0].Trim() : string.Empty
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            foreach (var candidate in candidates)
            {
                var escapedCandidate = EscapeODataValue(candidate);
                var filters = new[]
                {
                    "Salespers_Purch_Code eq '" + escapedCandidate + "'",
                    "No eq '" + escapedCandidate + "'",
                    "PCPL_Employee_Code eq '" + escapedCandidate + "'"
                };

                foreach (var filter in filters)
                {
                    var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", filter);
                    var resultData = result.Result;
                    var profile = resultData.Item1 == null || resultData.Item1.value == null ? null : resultData.Item1.value.FirstOrDefault();
                    if (profile != null)
                        return profile;
                }
            }

            return null;
        }

        private string ResolveSalespersonCodeFromUserNo(string userNo)
        {
            var profile = ResolveUserByNotificationIdentity(userNo);
            return profile == null ? string.Empty : (profile.Salespers_Purch_Code ?? string.Empty);
        }

        private SPProfile ResolveUserByNotificationIdentity(string userNo)
        {
            userNo = (userNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(userNo))
                return null;

            API ac = new API();
            var filters = new[]
            {
                "No eq '" + EscapeODataValue(userNo) + "'",
                "PCPL_Employee_Code eq '" + EscapeODataValue(userNo) + "'",
                "Salespers_Purch_Code eq '" + EscapeODataValue(userNo) + "'"
            };

            foreach (var filter in filters)
            {
                var result = ac.GetData<SPProfile>("EmployeesDotNetAPI", filter);
                var resultData = result.Result;
                var profile = resultData.Item1 == null || resultData.Item1.value == null ? null : resultData.Item1.value.FirstOrDefault();
                if (profile != null)
                    return profile;
            }

            return null;
        }

        private SPSQHeader GetQuoteHeader(string quoteNo)
        {
            quoteNo = (quoteNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(quoteNo))
                return null;

            API ac = new API();
            var result = ac.GetData<SPSQHeader>("SalesQuoteDotNetAPI", "No eq '" + EscapeODataValue(quoteNo) + "'");
            var resultData = result.Result;
            return resultData.Item1 == null || resultData.Item1.value == null ? null : resultData.Item1.value.FirstOrDefault();
        }

        private SPSalesOrdersList GetSalesOrder(string orderNo)
        {
            orderNo = (orderNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(orderNo))
                return null;

            API ac = new API();
            var result = ac.GetData<SPSalesOrdersList>("SalesOrdersListDotNetAPI", "No eq '" + EscapeODataValue(orderNo) + "'");
            var resultData = result.Result;
            return resultData.Item1 == null || resultData.Item1.value == null ? null : resultData.Item1.value.FirstOrDefault();
        }

        private static string EscapeODataValue(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static DateTime ParseDate(string value)
        {
            DateTime parsed;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsed))
                return parsed;

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out parsed))
                return parsed.ToUniversalTime();

            return DateTime.MinValue;
        }

        private static string NormalizeTimestamp(string value)
        {
            var parsed = ParseDate(value);
            if (parsed == DateTime.MinValue)
                return DateTime.UtcNow.ToString("o");

            return parsed.ToUniversalTime().ToString("o");
        }
    }
}