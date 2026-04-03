using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;

namespace SV22T1020590.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string? searchValue = "", int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1)
        {
            ViewData["Title"] = "Mặt hàng";
            const int pageSize = 12;

            int rowCount = 0;
            var products = ProductDAL.List(_configuration, out rowCount, searchValue ?? "", null, categoryId, minPrice, maxPrice, page, pageSize)
                .Where(p => p.IsSelling)
                .ToList();

            int catRow = 0;
            var categories = CategoryDAL.List(_configuration, out catRow, "", 1, 500);

            ViewBag.SearchValue = searchValue ?? "";
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.RowCount = rowCount;
            ViewBag.Categories = categories;

            return View(products);
        }

        public IActionResult Detail(int id)
        {
            var p = ProductDAL.Get(_configuration, id);
            if (p == null || !p.IsSelling)
                return NotFound();

            var photos = ProductPhotoDAL.GetByProductID(_configuration, id)
                .Where(x => !x.IsHidden)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.PhotoID)
                .ToList();

            var attributes = ProductAttributeDAL.GetByProductID(_configuration, id)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.AttributeID)
                .ToList();

            ViewBag.ProductPhotos = photos;
            ViewBag.ProductAttributes = attributes;
            ViewData["Title"] = p.ProductName;
            return View(p);
        }
    }
}
