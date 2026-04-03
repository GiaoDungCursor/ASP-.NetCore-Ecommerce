using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;

namespace SV22T1020590.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string searchValue = "", int? supplierID = null, int? categoryID = null,
            decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 0)
        {
            pageSize = pageSize > 0 ? pageSize : ApplicationContext.PageSize;
            ViewData["Title"] = "Quản lý Mặt Hàng";
            ViewBag.SearchValue = searchValue;
            ViewBag.SupplierID = supplierID;
            ViewBag.CategoryID = categoryID;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // Get suppliers and categories for filter dropdowns
            var suppliers = SupplierDAL.GetAll(_configuration);
            var categories = CategoryDAL.GetAll(_configuration);
            ViewBag.Suppliers = suppliers;
            ViewBag.Categories = categories;

            // Get products from database
            int rowCount = 0;
            var products = ProductDAL.List(_configuration, out rowCount, searchValue, supplierID, categoryID, minPrice, maxPrice, page, pageSize);

            // Get supplier and category names for each product
            var suppliersDict = suppliers.ToDictionary(s => s.SupplierID, s => s.SupplierName);
            var categoriesDict = categories.ToDictionary(c => c.CategoryID, c => c.CategoryName);

            var totalRecords = rowCount;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < totalPages;
            ViewBag.SupplierNames = suppliersDict;
            ViewBag.CategoryNames = categoriesDict;

            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết Mặt Hàng";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var supplier = product.SupplierID is int sid ? SupplierDAL.Get(_configuration, sid) : null;
            var category = product.CategoryID is int cid ? CategoryDAL.Get(_configuration, cid) : null;
            var attributes = ProductAttributeDAL.GetByProductID(_configuration, id);

            ViewBag.SupplierName = supplier?.SupplierName ?? "";
            ViewBag.CategoryName = category?.CategoryName ?? "";
            ViewBag.Attributes = attributes;

            return View(product);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            ViewData["Title"] = id.HasValue ? "Chỉnh sửa Mặt Hàng" : "Thêm Mặt Hàng";

            var suppliers = SupplierDAL.GetAll(_configuration);
            var categories = CategoryDAL.GetAll(_configuration);
            ViewBag.Suppliers = suppliers;
            ViewBag.Categories = categories;

            if (id.HasValue)
            {
                var product = ProductDAL.Get(_configuration, id.Value);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                return View(product);
            }

            return View(new Product { IsSelling = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Product product, IFormFile? uploadedMainPhoto)
        {
            if (uploadedMainPhoto != null && uploadedMainPhoto.Length > 0)
            {
                product.Photo = await SaveProductImageAsync(uploadedMainPhoto);
            }
            else if (!string.IsNullOrWhiteSpace(product.Photo))
            {
                product.Photo = product.Photo.Trim();
            }

            if (string.IsNullOrWhiteSpace(product.ProductName))
                ModelState.AddModelError(nameof(product.ProductName), "Vui lòng nhập tên mặt hàng.");

            if (product.CategoryID <= 0)
                ModelState.AddModelError(nameof(product.CategoryID), "Vui lòng chọn loại hàng.");

            if (product.SupplierID <= 0)
                ModelState.AddModelError(nameof(product.SupplierID), "Vui lòng chọn nhà cung cấp.");

            if (string.IsNullOrWhiteSpace(product.Unit))
                ModelState.AddModelError(nameof(product.Unit), "Vui lòng nhập đơn vị tính.");

            if (product.Price <= 0)
                ModelState.AddModelError(nameof(product.Price), "Vui lòng nhập giá bán.");

            if (string.IsNullOrWhiteSpace(product.ProductDescription))
                ModelState.AddModelError(nameof(product.ProductDescription), "Vui lòng nhập mô tả.");

            if (!ModelState.IsValid)
            {
                var suppliers = SupplierDAL.GetAll(_configuration);
                var categories = CategoryDAL.GetAll(_configuration);
                ViewBag.Suppliers = suppliers;
                ViewBag.Categories = categories;

                ViewData["Title"] = product.ProductID == 0 ? "Thêm Mặt Hàng" : "Chỉnh sửa Mặt Hàng";
                return View("Edit", product);
            }

            bool success;
            if (product.ProductID == 0)
            {
                success = ProductDAL.Add(_configuration, product);
                TempData["SuccessMessage"] = success ? "Thêm mặt hàng thành công!" : "Thêm mặt hàng thất bại!";
            }
            else
            {
                // Preserve existing photo when user does not pick/upload a new one.
                if (string.IsNullOrWhiteSpace(product.Photo))
                {
                    var existing = ProductDAL.Get(_configuration, product.ProductID);
                    if (existing != null)
                    {
                        product.Photo = existing.Photo;
                    }
                }
                success = ProductDAL.Update(_configuration, product);
                TempData["SuccessMessage"] = success ? "Cập nhật mặt hàng thành công!" : "Cập nhật mặt hàng thất bại!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewData["Title"] = "Xóa mặt hàng";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var supplier = product.SupplierID is int sid2 ? SupplierDAL.Get(_configuration, sid2) : null;
            var category = product.CategoryID is int cid2 ? CategoryDAL.Get(_configuration, cid2) : null;
            ViewBag.SupplierName = supplier?.SupplierName ?? "";
            ViewBag.CategoryName = category?.CategoryName ?? "";

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (ProductDAL.Delete(_configuration, id))
                TempData["SuccessMessage"] = "Xóa mặt hàng thành công!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddAttribute(int id)
        {
            ViewData["Title"] = "Thêm Thuộc Tính";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ProductID = id;
            ViewBag.ProductName = product.ProductName;

            return View(new ProductAttribute { ProductID = id, DisplayOrder = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAttribute(ProductAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.AttributeName))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên thuộc tính.");
                var product = ProductDAL.Get(_configuration, attribute.ProductID);
                ViewBag.ProductID = attribute.ProductID;
                ViewBag.ProductName = product?.ProductName ?? "";
                return View("AddAttribute", attribute);
            }

            if (ProductAttributeDAL.Add(_configuration, attribute))
            {
                TempData["SuccessMessage"] = "Thêm thuộc tính thành công!";
            }

            return RedirectToAction("Details", new { id = attribute.ProductID });
        }

        [HttpGet]
        public IActionResult EditAttribute(int id, int attributeId)
        {
            ViewData["Title"] = "Chỉnh sửa Thuộc Tính";
            var attribute = ProductAttributeDAL.Get(_configuration, attributeId);
            if (attribute == null || attribute.ProductID != id)
            {
                return RedirectToAction("Details", new { id });
            }

            var product = ProductDAL.Get(_configuration, id);
            ViewBag.ProductID = id;
            ViewBag.ProductName = product?.ProductName ?? "";

            return View(attribute);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAttribute(ProductAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.AttributeName))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên thuộc tính.");
                var product = ProductDAL.Get(_configuration, attribute.ProductID);
                ViewBag.ProductID = attribute.ProductID;
                ViewBag.ProductName = product?.ProductName ?? "";
                return View("EditAttribute", attribute);
            }

            if (ProductAttributeDAL.Update(_configuration, attribute))
            {
                TempData["SuccessMessage"] = "Cập nhật thuộc tính thành công!";
            }

            return RedirectToAction("Details", new { id = attribute.ProductID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            if (ProductAttributeDAL.Delete(_configuration, attributeId))
            {
                TempData["SuccessMessage"] = "Xóa thuộc tính thành công!";
            }

            return RedirectToAction("Details", new { id });
        }

        // ProductPhotos Actions
        [HttpGet]
        public IActionResult ListPhotos(int id)
        {
            ViewData["Title"] = "Quản lý Hình Ảnh";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ProductID = id;
            ViewBag.ProductName = product.ProductName;
            ViewBag.MainPhoto = product.Photo;

            var photos = ProductPhotoDAL.GetByProductID(_configuration, id);
            return View(photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetMainPhoto(int id, int photoId)
        {
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            var photo = ProductPhotoDAL.Get(_configuration, photoId);
            if (photo == null || photo.ProductID != id || string.IsNullOrWhiteSpace(photo.Photo))
            {
                TempData["SuccessMessage"] = "Không thể đặt ảnh đại diện (hình ảnh không hợp lệ).";
                return RedirectToAction("ListPhotos", new { id });
            }

            if (ProductDAL.UpdateMainPhoto(_configuration, id, photo.Photo.Trim()))
            {
                TempData["SuccessMessage"] = "Đã đặt ảnh đại diện thành công!";
            }

            return RedirectToAction("ListPhotos", new { id });
        }

        [HttpGet]
        public IActionResult AddPhoto(int id)
        {
            ViewData["Title"] = "Thêm Hình Ảnh";
            var product = ProductDAL.Get(_configuration, id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ProductID = id;
            ViewBag.ProductName = product.ProductName;

            return View(new ProductPhoto { ProductID = id, DisplayOrder = 0, IsHidden = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePhoto(int productId, ProductPhoto photo, IFormFile? uploadedFile)
        {
            // Nếu ProductID từ form không có, lấy từ parameter
            if (photo.ProductID == 0 && productId > 0)
            {
                photo.ProductID = productId;
            }

            // DEBUG: Log ProductID
            System.Diagnostics.Debug.WriteLine($"=== DEBUG: productId param = {productId}, photo.ProductID = {photo.ProductID}, Photo = '{photo.Photo}' ===");

            // Handle file upload first
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                photo.Photo = await SaveProductImageAsync(uploadedFile);
            }
            else
            {
                // URL mode: in some cases model binding may not populate photo.Photo (prefix mismatch, cached view, etc.)
                // So we read it directly from the posted form as a fallback.
                if (string.IsNullOrWhiteSpace(photo.Photo))
                {
                    string? postedUrl = Request.Form["Photo"].FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(postedUrl))
                    {
                        postedUrl = Request.Form["photo.Photo"].FirstOrDefault();
                    }
                    if (string.IsNullOrWhiteSpace(postedUrl))
                    {
                        postedUrl = Request.Form["PhotoUrl"].FirstOrDefault();
                    }

                    if (!string.IsNullOrWhiteSpace(postedUrl))
                    {
                        photo.Photo = postedUrl.Trim();
                    }
                }

                // Trim whitespace from photo URL if no file uploaded
                if (!string.IsNullOrEmpty(photo.Photo))
                {
                    photo.Photo = photo.Photo.Trim();
                }
            }

            // Validate ProductID
            if (photo.ProductID <= 0)
            {
                ModelState.AddModelError(string.Empty, $"ProductID không hợp lệ: {photo.ProductID}");
                ViewBag.ProductID = photo.ProductID;
                ViewBag.ProductName = "";
                return View("AddPhoto", photo);
            }

            // Validate Product exists
            var existingProduct = ProductDAL.Get(_configuration, photo.ProductID);
            if (existingProduct == null)
            {
                ModelState.AddModelError(string.Empty, $"Không tìm thấy sản phẩm với ID: {photo.ProductID}");
                ViewBag.ProductID = photo.ProductID;
                ViewBag.ProductName = "";
                return View("AddPhoto", photo);
            }

            if (string.IsNullOrWhiteSpace(photo.Photo))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn file ảnh hoặc nhập URL hình ảnh.");
                ViewBag.ProductID = photo.ProductID;
                ViewBag.ProductName = existingProduct?.ProductName ?? "";
                return View("AddPhoto", photo);
            }

            if (ProductPhotoDAL.Add(_configuration, photo))
            {
                TempData["SuccessMessage"] = "Thêm hình ảnh thành công!";
            }

            return RedirectToAction("ListPhotos", new { id = photo.ProductID });
        }

        [HttpGet]
        public IActionResult EditPhoto(int id, int photoId)
        {
            ViewData["Title"] = "Chỉnh sửa Hình Ảnh";
            var photo = ProductPhotoDAL.Get(_configuration, photoId);
            if (photo == null || photo.ProductID != id)
            {
                return RedirectToAction("ListPhotos", new { id });
            }

            var product = ProductDAL.Get(_configuration, id);
            ViewBag.ProductID = id;
            ViewBag.ProductName = product?.ProductName ?? "";

            return View(photo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhoto(ProductPhoto photo, IFormFile? uploadedFile)
        {
            // Trim whitespace from photo URL
            if (!string.IsNullOrEmpty(photo.Photo))
            {
                photo.Photo = photo.Photo.Trim();
            }

            // Handle file upload
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                photo.Photo = await SaveProductImageAsync(uploadedFile);
            }

            if (string.IsNullOrWhiteSpace(photo.Photo))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn file ảnh hoặc nhập URL hình ảnh.");
                var product = ProductDAL.Get(_configuration, photo.ProductID);
                ViewBag.ProductID = photo.ProductID;
                ViewBag.ProductName = product?.ProductName ?? "";
                return View("EditPhoto", photo);
            }

            if (ProductPhotoDAL.Update(_configuration, photo))
            {
                TempData["SuccessMessage"] = "Cập nhật hình ảnh thành công!";
            }

            return RedirectToAction("ListPhotos", new { id = photo.ProductID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePhoto(int id, int photoId)
        {
            if (ProductPhotoDAL.Delete(_configuration, photoId))
            {
                TempData["SuccessMessage"] = "Xóa hình ảnh thành công!";
            }

            return RedirectToAction("ListPhotos", new { id });
        }

        private async Task<string> SaveProductImageAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var relativePath = $"/images/products/{uniqueFileName}";

            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();

            foreach (var folder in GetProductImageFolders())
            {
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, uniqueFileName);
                await System.IO.File.WriteAllBytesAsync(filePath, content);
            }

            return relativePath;
        }

        private IEnumerable<string> GetProductImageFolders()
        {
            var adminRoot = Directory.GetCurrentDirectory();
            var adminFolder = Path.Combine(adminRoot, "wwwroot", "images", "products");
            var shopFolder = Path.GetFullPath(Path.Combine(adminRoot, "..", "SV22T1020590.Shop", "wwwroot", "images", "products"));

            return new[] { adminFolder, shopFolder }.Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
