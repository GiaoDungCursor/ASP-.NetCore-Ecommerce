using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Shop.Models;
using SV22T1020590.Shop.Services;
using System.Diagnostics;

namespace SV22T1020590.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ShopCartService _cart;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ShopCartService cart)
        {
            _logger = logger;
            _configuration = configuration;
            _cart = cart;
        }

        public IActionResult Index()
        {
            int rowCount = 0;
            var products = ProductDAL.List(_configuration, out rowCount, "", null, null, null, null, 1, 8)
                .Where(p => p.IsSelling)
                .ToList();
            ViewBag.CartCount = _cart.Count;
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
