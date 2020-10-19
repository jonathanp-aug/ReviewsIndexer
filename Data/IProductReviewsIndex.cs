using System.Collections.Generic;

namespace ReviewsIndexer.Data
{
    public interface IProductReviewsIndex
    {
        public IEnumerable<ReviewsIndexationState> IndexedData { get; }

        public ReviewsIndexationState RequestIndexation(string asin);
    }

    public interface IProductReviewsIndexUpdater : IProductReviewsIndex
    {
        public bool TryDequeueRequest(out string asin);

        public void SetAsIndexing(string asin);
        public void SetAsSucceded(string asin, IEnumerable<Review> reviews);
        public void SetAsErrored(string asin, string message);
    }
}
