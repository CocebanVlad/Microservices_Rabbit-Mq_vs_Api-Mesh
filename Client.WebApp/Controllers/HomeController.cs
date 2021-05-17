using Client.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClientService service = new ClientService();

        [HttpGet]
        public IActionResult Index()
        {
            return View(new IndexModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexModel model)
        {
            var s1 = Stopwatch.StartNew();
            model.APIOutput = string.Join(" ", await service.SplitAndSortWordsUsingWebAPIsAsync(model.Input));
            s1.Stop();
            model.APIElapsedTime = s1.Elapsed;

            var s2 = Stopwatch.StartNew();
            model.MQOutput = string.Join(" ", await service.SplitAndSortWordsUsingRabbitMQAsync(model.Input));
            s2.Stop();
            model.MQElapsedTime = s2.Elapsed;

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}