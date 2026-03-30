using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;
using SV22T1020590.Models.Sales;
using SV22T1020590.Shop.Services;

namespace SV22T1020590.Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ShopCartService _cart;

        public CartController(IConfiguration configuration, ShopCartService cart)
        {
            _configuration = configuration;
            _cart = cart;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Giỏ hàng";
            var cart = _cart.GetCart();
            var map = new Dictionary<int, Product>();
            foreach (var line in cart)
            {
                if (map.ContainsKey(line.ProductID)) continue;
                var p = ProductDAL.Get(_configuration, line.ProductID);
                if (p != null) map[line.ProductID] = p;
            }
            ViewBag.ProductMap = map;
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1, string? returnUrl = null)
        {
            var p = ProductDAL.Get(_configuration, productId);
            if (p == null || !p.IsSelling)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Không tìm thấy mặt hàng." });
                }
                TempData["Error"] = "Không tìm thấy mặt hàng.";
                return RedirectLocalOr(returnUrl, "Index", "Product");
            }

            quantity = Math.Max(1, quantity);
            var cart = _cart.GetCart();
            var line = cart.FirstOrDefault(x => x.ProductID == productId);
            if (line == null)
                cart.Add(new OrderDetail { ProductID = productId, Quantity = quantity, SalePrice = p.Price });
            else
            {
                line.Quantity += quantity;
                line.SalePrice = p.Price;
            }
            _cart.SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng.", cartCount = _cart.Count });
            }

            TempData["Success"] = "Đã thêm vào giỏ hàng.";
            return RedirectLocalOr(returnUrl, "Index", "Home");
        }

        /// <summary>Quay lại trang gọi thêm giỏ (path + query) nếu hợp lệ; không thì fallback.</summary>
        private IActionResult RedirectLocalOr(string? returnUrl, string fallbackAction, string fallbackController)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction(fallbackAction, fallbackController);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = _cart.GetCart();
            var line = cart.FirstOrDefault(x => x.ProductID == productId);
            if (line == null) return RedirectToAction(nameof(Index));
            if (quantity <= 0)
                cart.RemoveAll(x => x.ProductID == productId);
            else
            {
                line.Quantity = quantity;
            }
            _cart.SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = _cart.GetCart().Where(x => x.ProductID != productId).ToList();
            _cart.SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.SaveCart(new List<OrderDetail>());
            return RedirectToAction(nameof(Index));
        }
    }
}
