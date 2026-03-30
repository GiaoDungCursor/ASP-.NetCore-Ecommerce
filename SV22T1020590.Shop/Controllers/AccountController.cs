using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020590.DataLayers;
using SV22T1020590.Models;
using SV22T1020590.Shop;

namespace SV22T1020590.Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static bool PasswordMatches(string? stored, string? plain)
        {
            if (string.IsNullOrEmpty(stored) || plain == null) return false;
            var hash = CryptHelper.HashMD5(plain);
            if (string.Equals(stored.Trim(), hash, StringComparison.OrdinalIgnoreCase))
                return true;
            return string.Equals(stored, plain, StringComparison.Ordinal);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(
            string customerName, string contactName, string email, string password, string confirmPassword,
            string province, string address, string phone, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(customerName))
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên hiển thị.");
            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError(string.Empty, "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
                ModelState.AddModelError(string.Empty, "Mật khẩu tối thiểu 4 ký tự.");
            if (password != confirmPassword)
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");

            if (CustomerDAL.GetByEmail(_configuration, email) != null)
                ModelState.AddModelError(string.Empty, "Email đã được đăng ký.");

            if (!ModelState.IsValid)
                return View();

            var c = new Customer
            {
                CustomerName = customerName.Trim(),
                ContactName = contactName?.Trim() ?? "",
                Province = province?.Trim() ?? "",
                Address = address?.Trim() ?? "",
                Phone = phone?.Trim() ?? "",
                Email = email.Trim(),
                Password = CryptHelper.HashMD5(password),
                IsLocked = false
            };

            var id = CustomerDAL.Add(_configuration, c);
            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var c = CustomerDAL.GetByEmail(_configuration, email ?? "");
            if (c == null || c.IsLocked || !PasswordMatches(c.Password, password))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, c.CustomerID.ToString()),
                new Claim(ClaimTypes.Email, c.Email),
                new Claim(ClaimTypes.Name, c.CustomerName)
            };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var id = User.GetCustomerId();
            if (id == null) return RedirectToAction(nameof(Login));
            var c = CustomerDAL.Get(_configuration, id.Value);
            if (c == null) return RedirectToAction(nameof(Login));
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(Customer model)
        {
            var id = User.GetCustomerId();
            if (id == null || id.Value != model.CustomerID)
                return RedirectToAction(nameof(Login));

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(model.CustomerName), "Bắt buộc.");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Bắt buộc.");

            var existing = CustomerDAL.GetByEmail(_configuration, model.Email);
            if (existing != null && existing.CustomerID != id.Value)
                ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");

            if (!ModelState.IsValid)
                return View(model);

            var db = CustomerDAL.Get(_configuration, id.Value);
            if (db == null) return RedirectToAction(nameof(Login));

            db.CustomerName = model.CustomerName;
            db.ContactName = model.ContactName ?? "";
            db.Province = model.Province ?? "";
            db.Address = model.Address ?? "";
            db.Phone = model.Phone ?? "";
            db.Email = model.Email.Trim();

            if (CustomerDAL.Update(_configuration, db))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, db.CustomerID.ToString()),
                    new Claim(ClaimTypes.Email, db.Email),
                    new Claim(ClaimTypes.Name, db.CustomerName)
                };
                var cid = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(cid));
                TempData["Success"] = "Đã cập nhật thông tin.";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var id = User.GetCustomerId();
            if (id == null) return RedirectToAction(nameof(Login));

            var c = CustomerDAL.Get(_configuration, id.Value);
            if (c == null) return RedirectToAction(nameof(Login));

            if (!PasswordMatches(c.Password, currentPassword))
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                ModelState.AddModelError(string.Empty, "Mật khẩu mới tối thiểu 4 ký tự.");
            if (newPassword != confirmPassword)
                ModelState.AddModelError(string.Empty, "Xác nhận mật khẩu không khớp.");

            if (!ModelState.IsValid)
                return View();

            CustomerDAL.ChangePassword(_configuration, id.Value, CryptHelper.HashMD5(newPassword!));
            TempData["Success"] = "Đã đổi mật khẩu. Vui lòng đăng nhập lại nếu cần.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
