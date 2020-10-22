using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace ReviewsIndexer.Data
{
    public class ProductIndexationTask
    {
        private Thread IndexationEngineThread = null;
        private volatile bool continueIndexation = false;
        private IProductIndexUpdater IndexUpdater;
        private CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");


        public ProductIndexationTask(IProductIndexUpdater indexUpdater)
        {
            if (indexUpdater == null)
                throw new ArgumentNullException("indexUpdater");

            IndexUpdater = indexUpdater;
        }

        public bool IsIndexationEngineRunning
        {
            get
            {
                return IndexationEngineThread != null && IndexationEngineThread.IsAlive;
            }
        }

        public void Start()
        {
            if (IsIndexationEngineRunning)
                return;

            continueIndexation = true;
            ThreadStart starter = delegate { IndexationEngineJob(); };
            IndexationEngineThread = new Thread(starter);
            IndexationEngineThread.IsBackground = true;
            IndexationEngineThread.Start();
        }

        public void Stop()
        {
            if (!IsIndexationEngineRunning)
                return;

            continueIndexation = false;
            IndexationEngineThread.Join();
            IndexationEngineThread = null;
        }

        private void IndexationEngineJob()
        {
            var restClient = new RestClient("https://www.amazon.com/product-reviews/");

            while (continueIndexation)
            {
                if (!IndexUpdater.TryDequeueRequest(out var asin))
                {
                    Thread.Sleep(1000);
                    if (continueIndexation)
                        continue;
                    else
                        break;
                }

                try
                {
                    IndexUpdater.SetAsIndexing(asin);

                    var request = new RestRequest(asin, Method.GET);
                    request.AddParameter("sortBy", "recent", ParameterType.QueryString);
                    var response = restClient.Execute(request);

                    if (!continueIndexation)
                        break;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(response.Content);

                        var name = htmlDoc.DocumentNode.SelectSingleNode("//a[@data-hook = 'product-link']").InnerText;
                        var url = htmlDoc.DocumentNode.SelectSingleNode("//a[@data-hook = 'product-link']").GetAttributeValue("href", "#");
                        var by = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-hook = 'cr-product-byline']/span/a").InnerText;
                        var averageRating = htmlDoc.DocumentNode.SelectSingleNode("//span[@data-hook = 'rating-out-of-text']").InnerText;
                        var countersPhrase = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-hook = 'cr-filter-info-review-rating-count']/span").InnerText.Trim();
                        var countersPhrases = countersPhrase.Split('|');

                        var reviews = new List<Review>();
                        AppendReviews(htmlDoc, reviews, asin);

                        for (int i = 2; i <= 5; i++)
                        {
                            var pageRequest = new RestRequest(asin, Method.GET);
                            pageRequest.AddParameter("sortBy", "recent", ParameterType.QueryString);
                            pageRequest.AddParameter("pageNumber", i, ParameterType.QueryString);
                            var pageResponse = restClient.Execute(pageRequest);

                            if (pageResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var pageHtmlDoc = new HtmlDocument();
                                pageHtmlDoc.LoadHtml(pageResponse.Content);
                                if (AppendReviews(pageHtmlDoc, reviews, asin) < 10)
                                    break;
                            }
                        }

                        IndexUpdater.SetAsSucceded(asin, name, $"https://amazon.com{url}", by, averageRating, countersPhrases[0], countersPhrases[1], reviews);
                    }
                    else
                    {
                        IndexUpdater.SetAsErrored(asin, $"HTTP {response.StatusCode} : {response.StatusDescription}");
                    }
                }
                catch (Exception ex)
                {
                    IndexUpdater.SetAsErrored(asin, $"{ex.GetType().FullName} : {ex.Message}");
                }
            }
        }

        private int AppendReviews(HtmlDocument htmlDoc, List<Review> collection, String productAsin)
        {
            var reviewNodes = htmlDoc.DocumentNode.SelectNodes("//div[@data-hook = 'review']");
            foreach (var node in reviewNodes)
                collection.Add(ParseHtmlNode(node, culture, productAsin));
            
            return reviewNodes.Count;
        }

        private static Review ParseHtmlNode(HtmlNode node, CultureInfo culture, String productAsin)
        {
            var review = new Review();
            review.ProductAsin = productAsin;

            review.ID = node.GetAttributeValue("id", string.Empty);
            review.Title = node.SelectSingleNode(".//a[@data-hook='review-title']/span").InnerText;
            review.Content = node.SelectSingleNode(".//span[@data-hook='review-body']/span").InnerText;

            var date = node.SelectSingleNode(".//span[@data-hook='review-date']").InnerText;
            var match = Regex.Match(date, @".*\s(?<dateValue>[A-Z][a-z]+\s[0-9]+,\s[0-9]+)");
            if (match.Success)
                review.Date = DateTime.Parse(match.Groups["dateValue"].Value, culture);

            var rating = node.SelectSingleNode(".//i[@data-hook='review-star-rating']").InnerText;
            review.Rating = (int)float.Parse(rating.Split('.')[0]);

            return review;
        }
    }
}
