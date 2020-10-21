using System.Collections.Generic;
using System.Linq;

namespace ReviewsIndexer.Data
{
    /// <summary>
    /// A local thread safe product reviews index
    /// </summary>
    public class LocalProductIndex : IProductIndexUpdater
    {
        public static LocalProductIndex Instance { get; } = new LocalProductIndex();

        private object _IndexedDataSync = new object();
        private object _LocalQueueSync = new object();
        private Dictionary<string, ProductIndexationState> _IndexedData = new Dictionary<string, ProductIndexationState>();
        private Queue<string> _LocalQueue = new Queue<string>();

        private LocalProductIndex()
        {

        }

        public ProductIndexationState this[string asin] {
            get
            {
                lock (_IndexedDataSync)
                    return _IndexedData[asin];
            }
        }

        public IEnumerable<ProductIndexationState> IndexedData
        {
            get
            {
                lock(_IndexedDataSync)
                    return _IndexedData.Values.ToList();
            }
        }

        public ProductIndexationState RequestIndexation(string asin)
        {
            if (string.IsNullOrWhiteSpace(asin))
                return null;

            var state = new ProductIndexationState(asin);

            try
            {
                lock (_IndexedDataSync)
                {
                    if (_IndexedData.ContainsKey(state.ProductAsin))
                    {
                        if (_IndexedData[state.ProductAsin].IndexationStatus != ProductIndexationState.Status.Error)
                            return _IndexedData[state.ProductAsin];
                        else
                            _IndexedData.Remove(state.ProductAsin);
                    }

                    _IndexedData[state.ProductAsin] = state;
                }
                lock (_LocalQueueSync)
                {
                    _LocalQueue.Enqueue(state.ProductAsin);
                }
            }
            catch
            {
                _IndexedData.Remove(state.ProductAsin);
                throw;
            }

            return state;
        }

        public void SetAsErrored(string asin, string message)
        {
            lock (_IndexedDataSync)
            {
                var state = _IndexedData[asin];
                state.IndexationStatus = ProductIndexationState.Status.Error;
                state.ErrorMessage = message;
            }
        }

        public void SetAsSucceded(string asin, string name, string url, string by, string ratingAverage, string ratingTotalCount, string reviewsTotalCount, IEnumerable<Review> reviews)
        {
            lock (_IndexedDataSync)
            {
                var state = _IndexedData[asin];
                state.IndexationStatus = ProductIndexationState.Status.Indexed;
                state.Name = name;
                state.Url = url;
                state.By = by;
                state.RatingAverage = ratingAverage;
                state.RatingTotalCount = ratingTotalCount;
                state.ReviewTotalCount = reviewsTotalCount;
                state.IndexedReviews = reviews;
            }
        }

        public void SetAsIndexing(string asin)
        {
            lock (_IndexedDataSync)
            {
                var state = _IndexedData[asin];
                state.IndexationStatus = ProductIndexationState.Status.Indexing;
            }
        }

        public bool TryDequeueRequest(out string asin)
        {
            lock (_LocalQueueSync)
            {
                return _LocalQueue.TryDequeue(out asin);
            }
        }
    }
}
