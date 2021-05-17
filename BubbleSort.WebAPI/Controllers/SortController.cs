using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BubbleSort.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SortController : ControllerBase
    {
        private readonly BubbleSortService service = new BubbleSortService();

        [HttpPost]
        public List<string> Post(List<string> list)
        {
            return service.Sort(list);
        }
    }
}