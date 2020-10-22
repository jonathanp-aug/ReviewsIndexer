using Microsoft.AspNetCore.Mvc;
using ReviewsIndexer.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ReviewsIndexer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProductController : ControllerBase
    {

        private IProductIndex ProductIndex;

        public ProductController(IProductIndex productIndex)
        {
            ProductIndex = productIndex;
        }

        [HttpGet]
        public IEnumerable<ProductIndexationState> ListAll()
        {
            return ProductIndex.IndexedData;
        }

        [HttpGet]
        public IEnumerable<ProductIndexationState> List(string asins)
        {
            if (string.IsNullOrEmpty(asins))
                return new ProductIndexationState[0];

            return asins.Split(',').Select(asin => ProductIndex[asin.Trim()]).ToArray();
        }

        [HttpPost]
        public IEnumerable<ProductIndexationState> RequestIndexation([FromForm]string asins)
        {
            var results = new List<ProductIndexationState>();

            if (string.IsNullOrEmpty(asins))
                return results;
            
            foreach (var asin in asins.Split(','))
            {
                var state = ProductIndex.RequestIndexation(asin.Trim());
                if (state != null)
                    results.Add(state);
            }

            return results;
        }
    }
}
