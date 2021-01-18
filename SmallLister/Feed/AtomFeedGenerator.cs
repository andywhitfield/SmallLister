using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SmallLister.Model;

namespace SmallLister.Feed
{
    public class AtomFeedGenerator : IFeedGenerator
    {
        private readonly XNamespace ns = "http://www.w3.org/2005/Atom";

        public XDocument GenerateFeed(string baseUri, IEnumerable<UserItem> items, UserFeed userFeed)
        {
            var atom = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var docRoot = new XElement(ns + "feed");

            docRoot.Add(new XElement(ns + "title", "SmallLister"));
            docRoot.Add(new XElement(ns + "link", new XAttribute("type", "text/html"), new XAttribute("href", baseUri), new XAttribute("rel", "alternate")));
            docRoot.Add(new XElement(ns + "updated", (items.Any() ? items.Max(x => x.NextDueDate ?? DateTime.Today) : DateTime.Today).ToString("O")));
            docRoot.Add(new XElement(ns + "id", $"{baseUri}/feed/{userFeed.UserFeedIdentifier}"));

            foreach (var item in items)
            {
                var entry = new XElement(ns + "entry");

                var dueDate = (item.NextDueDate ?? DateTime.Today).Date;
                var isOverdue = dueDate < DateTime.Today;
                var itemUri = $"{baseUri}/items/{item.UserItemId}";
                entry.Add(new XElement(ns + "title", userFeed.ItemDisplay == UserFeedItemDisplay.Description ? $"{(isOverdue ? "Overdue" : "Due")}: {item.Description}" : (isOverdue ? "Overdue item" : "Due item")));
                entry.Add(new XElement(ns + "link", new XAttribute("href", itemUri)));
                entry.Add(new XElement(ns + "updated", dueDate.ToString("O")));
                entry.Add(new XElement(ns + "id", $"{item.UserItemId}/{isOverdue.ToString().ToLowerInvariant()}/{dueDate.ToString("yyyyMMdd")}"));

                var content = new XElement(ns + "content", new XAttribute("type", "html"));
                var itemDescription = userFeed.ItemDisplay switch
                {
                    UserFeedItemDisplay.None => $"<p>You have an item that is {(isOverdue ? "overdue" : "due")}.</p>",
                    UserFeedItemDisplay.ShortDescription => $"<p>You have an item that is {(isOverdue ? "overdue" : "due")}: <b>{FirstWord(item.Description)}...</b></p>",
                    UserFeedItemDisplay.Description => $"<p>You have an item that is {(isOverdue ? "overdue" : "due")}:</p><p><b>{item.Description}</b></p>",
                    _ => throw new InvalidOperationException("Unknown feed item display option")
                };
                content.Add(new XCData($"{itemDescription}<p><a href=\"{itemUri}\">Open item</a></p>"));
                entry.Add(content);

                docRoot.Add(entry);
            }

            atom.Add(docRoot);
            return atom;
        }

        private string FirstWord(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "";
            return Regex.Match(description, @"^([\w\-]+)").ToString();
        }
    }
}