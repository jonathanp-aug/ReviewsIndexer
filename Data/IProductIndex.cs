using System.Collections.Generic;

namespace ReviewsIndexer.Data
{
    public interface IProductIndex
    {
        public IEnumerable<ProductIndexationState> IndexedData { get; }
        public ProductIndexationState this[string asin] { get; }

        public ProductIndexationState RequestIndexation(string asin);
    }

    public interface IProductIndexUpdater : IProductIndex
    {
        public bool TryDequeueRequest(out string asin);

        public void SetAsIndexing(string asin);
        public void SetAsSucceded(string asin, string name, string url, string by, string ratingAverage, string ratingCount, string reviewsCount, IEnumerable<Review> reviews);
        public void SetAsErrored(string asin, string message);
    }
}
