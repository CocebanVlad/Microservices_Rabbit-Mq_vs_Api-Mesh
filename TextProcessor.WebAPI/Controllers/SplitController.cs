using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace TextProcessor.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SplitController : ControllerBase
    {
        private readonly TextProcessorService service = new TextProcessorService();

        [HttpPost]
        public List<string> Post([FromBody] string text)
        {
            return service.Split(text);
        }
    }
}