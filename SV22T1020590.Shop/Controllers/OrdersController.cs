using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;
using SV22T1020590.Shop;

namespace SV22T1020590.Shop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IConfiguration _configuration;

        public OrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(int page = 1)
        {
            ViewData["Title"] = "Đơn hàng của tôi";
            var cid = User.GetCustomerId();
            if (cid == null) return RedirectToAction("Login", "Account");

            const int pageSize = 10;
            int rowCount = 0;
            var list = OrderDAL.ListByCustomer(_configuration, cid.Value, out rowCount, page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.RowCount = rowCount;
            return View(list);
        }

        public IActionResult Detail(int id)
        {
            var cid = User.GetCustomerId();
            if (cid == null) return RedirectToAction("Login", "Account");

            var order = OrderDAL.Get(_configuration, id);
            if (order == null || order.CustomerID != cid.Value)
                return NotFound();

            var map = new Dictionary<int, Product>();
            if (order.OrderDetails != null)
            {
                foreach (var d in order.OrderDetails)
                {
                    if (map.ContainsKey(d.ProductID)) continue;
                    var p = ProductDAL.Get(_configuration, d.ProductID);
                    if (p != null) map[d.ProductID] = p;
                }
            }
            ViewBag.LineProducts = map;

            ViewData["Title"] = $"Đơn hàng #{id}";
            return View(order);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var cid = User.GetCustomerId();
            if (cid == null) return RedirectToAction("Login", "Account");

            var order = OrderDAL.Get(_configuration, id);
            if (order == null || order.CustomerID != cid.Value)
                return NotFound();

            if (order.Status == "1" || order.Status == "2")
            {
                OrderDAL.SetStatus(_configuration, id, "-1");
            }

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
