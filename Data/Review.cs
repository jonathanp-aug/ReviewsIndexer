using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ReviewsIndexer.Data
{
    public class Review
    {
        public static Review ParseHtmlNode(HtmlNode node, CultureInfo culture)
        {
            var review = new Review();
            review.ID = node.GetAttributeValue("id", string.Empty);
            review.Title = node.SelectSingleNode("//a[@data-hook='review-title']/span").InnerText;
            review.Content = node.SelectSingleNode("//span[@data-hook='review-body']/span").InnerText;

            var date = node.SelectSingleNode("//span[@data-hook='review-date']").InnerText;
            var match = Regex.Match(date, @".*\s(?<dateValue>[A-Z][a-z]+\s[0-9]+,\s[0-9]+)");
            if (match.Success)
                review.Date = DateTime.Parse(match.Groups["dateValue"].Value, culture);

            return review;
        }

        public string ID { get; set; }

		public DateTime Date { get; set; }

		public string Title { get; set; }

		public string Content { get; set; }

		public int Rating { get; set; }        
    }
}
