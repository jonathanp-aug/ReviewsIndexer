using System;

namespace ReviewsIndexer.Data
{
    public class Review
    {
        public string ID { get; set; }

        public DateTime Date { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int Rating { get; set; }

        public string ProductAsin { get; set; }
    }
}
