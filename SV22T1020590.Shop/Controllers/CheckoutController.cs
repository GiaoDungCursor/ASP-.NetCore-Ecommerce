using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;
using SV22T1020590.Shop;
using SV22T1020590.Shop.Services;

namespace SV22T1020590.Shop.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ShopCartService _cart;

        public CheckoutController(IConfiguration configuration, ShopCartService cart)
        {
            _configuration = configuration;
            _cart = cart;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Đặt hàng";
            var cart = _cart.GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var cid = User.GetCustomerId();
            if (cid == null) return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });

            var c = CustomerDAL.Get(_configuration, cid.Value);
            ViewBag.Customer = c;
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(string deliveryProvince, string deliveryAddress)
        {
            var cart = _cart.GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            var cid = User.GetCustomerId();
            if (cid == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập địa chỉ giao hàng.");
                ViewBag.Customer = CustomerDAL.Get(_configuration, cid.Value);
                return View("Index", cart);
            }

            var order = new Order
            {
                CustomerID = cid.Value,
                OrderTime = DateTime.Now,
                DeliveryProvince = deliveryProvince ?? "",
                DeliveryAddress = deliveryAddress.Trim(),
                Status = ((int)SV22T1020590.Models.Sales.OrderStatusEnum.New).ToString(),
                OrderDetails = cart
            };

            var newId = OrderDAL.Add(_configuration, order);
            _cart.SaveCart(new List<SV22T1020590.Models.Sales.OrderDetail>());
            TempData["Success"] = "Đặt hàng thành công.";
            return RedirectToAction("Detail", "Orders", new { id = newId });
        }
    }
}
