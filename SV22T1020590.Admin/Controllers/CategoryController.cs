using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;

namespace SV22T1020590.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IConfiguration _configuration;

        public CategoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string searchValue = "", int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Loại Hàng";
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            int rowCount = 0;
            var categories = CategoryDAL.List(_configuration, out rowCount, searchValue, page, pageSize);

            var totalRecords = rowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm Loại Hàng";
            return View("Edit", new SV22T1020590.Models.Catalog.Category());
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["Title"] = "Sửa Loại Hàng";
            var category = CategoryDAL.Get(_configuration, id);
            if (category == null)
            {
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(SV22T1020590.Models.Catalog.Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                ModelState.AddModelError(nameof(category.CategoryName), "Vui lòng nhập tên loại hàng.");

            if (string.IsNullOrWhiteSpace(category.Description))
                ModelState.AddModelError(nameof(category.Description), "Vui lòng nhập mô tả loại hàng.");

            if (ModelState.IsValid)
            {
                if (category.CategoryID == 0)
                {
                    // Add new
                    CategoryDAL.Add(_configuration, category);
                    TempData["SuccessMessage"] = "Thêm loại hàng thành công!";
                }
                else
                {
                    // Update existing
                    if (CategoryDAL.Update(_configuration, category))
                    {
                        TempData["SuccessMessage"] = "Cập nhật loại hàng thành công!";
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["Title"] = category.CategoryID == 0 ? "Thêm Loại Hàng" : "Sửa Loại Hàng";
            return View("Edit", category);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Xóa loại hàng";
            var category = CategoryDAL.Get(_configuration, id);
            if (category == null)
            {
                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (CategoryDAL.Delete(_configuration, id))
                TempData["SuccessMessage"] = "Xóa loại hàng thành công!";

            return RedirectToAction("Index");
        }
    }
}
