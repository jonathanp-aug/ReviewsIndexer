using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ReviewsIndexer.Data
{
    public class ProductReviewsIndexationTask
    {
        private Thread IndexationEngineThread = null;
		private volatile bool continueIndexation = false;
		private IProductReviewsIndexUpdater IndexUpdater;


		public ProductReviewsIndexationTask(IProductReviewsIndexUpdater indexUpdater)
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
			var culture = CultureInfo.CreateSpecificCulture("en-US");

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

						var reviewNodes = htmlDoc.DocumentNode.SelectNodes("//div[@data-hook = 'review']");
						var reviews = new List<Review>();

						foreach (var node in reviewNodes)
							reviews.Add(Review.ParseHtmlNode(node, culture));

						IndexUpdater.SetAsSucceded(asin, reviews);
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
    }
}
