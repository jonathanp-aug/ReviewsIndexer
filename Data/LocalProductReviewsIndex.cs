using System.Collections.Generic;
using System.Linq;

namespace ReviewsIndexer.Data
{
    /// <summary>
    /// A local thread safe product reviews index
    /// </summary>
    public class LocalProductReviewsIndex : IProductReviewsIndexUpdater
    {
        public static LocalProductReviewsIndex Instance { get; } = new LocalProductReviewsIndex();

        private object _IndexedDataSync = new object();
        private object _LocalQueueSync = new object();
        private Dictionary<string,ReviewsIndexationState> _IndexedData = new Dictionary<string, ReviewsIndexationState>();
        private Queue<string> _LocalQueue = new Queue<string>();

        private LocalProductReviewsIndex()
        {

        }

        public IEnumerable<ReviewsIndexationState> IndexedData
        {
            get
            {
                lock(_IndexedDataSync)
                {
                    return _IndexedData.Values.ToList();
                }
            }
        }

        public ReviewsIndexationState RequestIndexation(string asin)
        {
            if (string.IsNullOrWhiteSpace(asin))
            {
                return null;
            }

            var state = new ReviewsIndexationState(asin.Trim());

            try
            {
                lock (_IndexedDataSync)
                {
                    if (_IndexedData.ContainsKey(state.ProductAsin))
                    {
                        if (_IndexedData[state.ProductAsin].IndexationStatus != ReviewsIndexationState.Status.Error)
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
                state.IndexationStatus = ReviewsIndexationState.Status.Error;
                state.ErrorMessage = message;
            }
        }

        public void SetAsSucceded(string asin, IEnumerable<Review> reviews)
        {
            lock (_IndexedDataSync)
            {
                var state = _IndexedData[asin];
                state.IndexationStatus = ReviewsIndexationState.Status.Indexed;
                state.IndexedReviews = reviews;
            }
        }

        public void SetAsIndexing(string asin)
        {
            lock (_IndexedDataSync)
            {
                var state = _IndexedData[asin];
                state.IndexationStatus = ReviewsIndexationState.Status.Indexing;
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
