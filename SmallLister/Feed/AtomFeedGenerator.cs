using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SmallLister.Model;

namespace SmallLister.Feed;

public class AtomFeedGenerator : IFeedGenerator
{
    private readonly XNamespace ns = "http://www.w3.org/2005/Atom";

    public XDocument GenerateFeed(string baseUri, DateTime publishDate, IEnumerable<UserItem> items, UserFeed userFeed)
    {
        var atom = new XDocument(new XDeclaration("1.0", "utf-8", null));
        var docRoot = new XElement(ns + "feed");

        docRoot.Add(new XElement(ns + "title", "SmallLister"));
        docRoot.Add(new XElement(ns + "link", new XAttribute("type", "text/html"), new XAttribute("href", baseUri), new XAttribute("rel", "alternate")));
        docRoot.Add(new XElement(ns + "updated", publishDate.ToString("O")));
        docRoot.Add(new XElement(ns + "id", $"{baseUri}/feed/{userFeed.UserFeedIdentifier}"));

        if (userFeed.FeedType == UserFeedType.Daily)
        {
            if (items.Any())
            {
                var entry = new XElement(ns + "entry");

                var itemDetails = items.Select(i => (i, (i.PostponedUntilDate ?? i.NextDueDate ?? DateTime.Today).Date));
                var overdueCount = itemDetails.Count(i => i.Date < DateTime.Today);
                var dueCount = items.Count() - overdueCount;
                var title = overdueCount > 0 ? $"{overdueCount} overdue" : "";
                if (dueCount > 0)
                {
                    if (title.Length > 0) title += " and ";
                    title += $"{dueCount} due today";
                }

                entry.Add(new XElement(ns + "title", title));
                entry.Add(new XElement(ns + "link", new XAttribute("href", baseUri)));
                entry.Add(new XElement(ns + "updated", publishDate.ToString("O")));
                entry.Add(new XElement(ns + "id", $"daily/{publishDate.ToString("O")}"));

                var content = new XElement(ns + "content", new XAttribute("type", "html"));
                var itemDescription = new StringBuilder($"You have {(overdueCount > 0 ? $"{overdueCount} {Task(overdueCount)} overdue" : "")}");
                if (dueCount > 0)
                {
                    if (overdueCount > 0) itemDescription.Append(" and ");
                    itemDescription.Append($"{dueCount} {Task(dueCount)} due today");
                }
                itemDescription.Append("!");

                if (userFeed.ItemDisplay != UserFeedItemDisplay.None)
                {
                    itemDescription.Append("<ul>");
                    foreach (var item in itemDetails)
                    {
                        itemDescription.Append($"<li><a href=\"{baseUri}/items/{item.i.UserItemId}\">Task due ");
                        var isOverdue = item.Date < DateTime.Today;
                        itemDescription.Append(GetFriendlyDueDate(isOverdue, item.Date));
                        itemDescription.Append(":</a> ");
                        itemDescription.Append(
                            userFeed.ItemDisplay switch
                            {
                                UserFeedItemDisplay.ShortDescription => $"{FirstWord(item.i.Description)}...",
                                UserFeedItemDisplay.Description => item.i.Description,
                                _ => ""
                            }
                        );
                        itemDescription.Append("</li>");
                    }
                    itemDescription.Append("</ul>");
                }

                content.Add(new XCData($"{itemDescription}<p><a href=\"{baseUri}\">Open Small:Lister</a></p>"));
                entry.Add(content);

                docRoot.Add(entry);
            }
        }
        else
        {
            foreach (var item in items)
            {
                var entry = new XElement(ns + "entry");

                var dueDate = (item.PostponedUntilDate ?? item.NextDueDate ?? DateTime.Today).Date;
                var isOverdue = dueDate < DateTime.Today;
                var itemUri = $"{baseUri}/items/{item.UserItemId}";
                entry.Add(new XElement(ns + "title", userFeed.ItemDisplay == UserFeedItemDisplay.Description ? $"{(isOverdue ? "Overdue" : "Due")}: {item.Description}" : (isOverdue ? "Overdue item" : "Due item")));
                entry.Add(new XElement(ns + "link", new XAttribute("href", itemUri)));
                entry.Add(new XElement(ns + "updated", publishDate.ToString("O")));
                entry.Add(new XElement(ns + "id", $"{item.UserItemId}/{isOverdue.ToString().ToLowerInvariant()}/{dueDate.ToString("yyyyMMdd")}"));

                var content = new XElement(ns + "content", new XAttribute("type", "html"));
                var itemDescription = userFeed.ItemDisplay switch
                {
                    UserFeedItemDisplay.None => $"<p>{GetDueDateMessage(isOverdue, dueDate)}.</p>",
                    UserFeedItemDisplay.ShortDescription => $"<p>{GetDueDateMessage(isOverdue, dueDate)}: <b>{FirstWord(item.Description)}...</b></p>",
                    UserFeedItemDisplay.Description => $"<p>{GetDueDateMessage(isOverdue, dueDate)}:</p><p><b>{item.Description}</b></p>",
                    _ => throw new InvalidOperationException("Unknown feed item display option")
                };
                content.Add(new XCData($"{itemDescription}<p><a href=\"{itemUri}\">Open item</a></p>"));
                entry.Add(content);

                docRoot.Add(entry);
            }
        }

        atom.Add(docRoot);
        return atom;
    }

    private string GetDueDateMessage(bool isOverdue, DateTime dueDate)
    {
        return isOverdue
            ? $"You have an overdue item! It was due {GetFriendlyDueDate(isOverdue, dueDate)}"
            : $"You have an item that is due {GetFriendlyDueDate(isOverdue, dueDate)}";
    }

    private string GetFriendlyDueDate(bool isOverdue, DateTime dueDate)
    {
        if (isOverdue)
        {
            var dayDiff = (DateTime.Today - dueDate.Date).TotalDays;
            return dayDiff == 1 ? "yesterday" : dayDiff < 7 ? $"last {dueDate:dddd}" : $"on {dueDate.ToString("d MMM yyyy")}";
        }
        else
        {
            return "today";
        }
    }

    private static string FirstWord(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return "";
        return Regex.Match(description, @"^([\w\-]+)").ToString();
    }

    private static string Task(int taskCount) => $"task{(taskCount == 1 ? "" : "s")}";
}