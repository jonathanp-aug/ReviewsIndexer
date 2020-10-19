using System.Collections.Generic;

namespace ReviewsIndexer.Data
{
    public class ReviewsIndexationState
    {
        public enum Status { Waiting, Indexing, Indexed, Error }

        public ReviewsIndexationState(string productAsin)
        {
            ProductAsin = productAsin;
            IndexationStatus = Status.Waiting;
        }

        public string ProductAsin { get; set; }

        public Status IndexationStatus { get; set; }

        public IEnumerable<Review> IndexedReviews { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}
