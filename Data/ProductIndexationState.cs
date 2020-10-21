using System.Collections.Generic;

namespace ReviewsIndexer.Data
{
    public class ProductIndexationState
    {
        public enum Status { Waiting, Indexing, Indexed, Error }

        public ProductIndexationState(string productAsin)
        {
            ProductAsin = productAsin;
            IndexationStatus = Status.Waiting;
        }

        public string ProductAsin { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string By { get; set; }

        public string RatingAverage { get; set; }
        public string RatingTotalCount { get; set; }
        public string ReviewTotalCount { get; set; }

        public Status IndexationStatus { get; set; }

        public IEnumerable<Review> IndexedReviews { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}
