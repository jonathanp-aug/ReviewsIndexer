using Microsoft.AspNetCore.Mvc;
using ReviewsIndexer.Data;
using System.Collections.Generic;

namespace ReviewsIndexer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProductReviewController : ControllerBase
    {

        private IProductReviewsIndex index;

        public ProductReviewController()
        {
            index = LocalProductReviewsIndex.Instance;
        }

        [HttpGet]
        public IEnumerable<ReviewsIndexationState> ListAll()
        {
            return index.IndexedData;
        }

        [HttpPost]
        public void RequestIndexation([FromForm]string asins)
        {
            if (string.IsNullOrEmpty(asins))
                return;

            foreach (var asin in asins.Split(','))
                index.RequestIndexation(asin);
        }
    }
}
