using FreightManagement.Data;
using FreightManagement.Filters;
using FreightManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FreightManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // --------------------------------------------------
        // ĐĂNG NHẬP
        // --------------------------------------------------
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var hash = HashPassword(password);

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email
                                       && u.PasswordHash == hash
                                       && u.TrangThai == "HoatDong");

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.";
                return View();
            }

            // Lưu thông tin vào Session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("HoTen", user.HoTen);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("RoleName", user.Role.RoleName);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            // Điều hướng theo role
            return user.Role.RoleName switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "KhachHang" => RedirectToAction("Index", "Orders"),
                "QuanLyKho" => RedirectToAction("Index", "Warehouse"),
                "TaiXe" => RedirectToAction("Index", "Driver"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // --------------------------------------------------
        // ĐĂNG KÝ (chỉ cho KhachHang tự đăng ký)
        // --------------------------------------------------
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string hoTen, string email,
            string password, string confirmPassword,
            string? soDienThoai, string? diaChi)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            if (await _db.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email này đã được đăng ký.";
                return View();
            }

            var user = new User
            {
                HoTen = hoTen,
                Email = email,
                PasswordHash = HashPassword(password),
                SoDienThoai = soDienThoai,
                DiaChi = diaChi,
                RoleId = 2  // KhachHang
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // --------------------------------------------------
        // ĐĂNG XUẤT
        // --------------------------------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // --------------------------------------------------
        // HELPER: Hash mật khẩu bằng SHA256
        // Ghi chú: Thực tế nên dùng BCrypt.Net-Next (an toàn hơn)
        //   Install-Package BCrypt.Net-Next
        //   BCrypt.Net.BCrypt.HashPassword(password)
        //   BCrypt.Net.BCrypt.Verify(password, hash)
        // --------------------------------------------------
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        // ── XEM THÔNG TIN CÁ NHÂN ──────────────────────────────────────────
        [RequireLogin]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        // ── CẬP NHẬT THÔNG TIN CÁ NHÂN ─────────────────────────────────────
        [HttpPost]
        [RequireLogin]
        public async Task<IActionResult> UpdateProfile(
            string hoTen, string? soDienThoai,
            string? diaChi, string? cccd)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _db.Users.FindAsync(userId);

            if (user == null) return RedirectToAction("Login");

            // Validate họ tên không được rỗng
            if (string.IsNullOrWhiteSpace(hoTen))
            {
                var u = await _db.Users.Include(x => x.Role)
                            .FirstAsync(x => x.UserId == userId);
                ViewBag.Error = "Họ tên không được để trống.";
                return View("Profile", u);
            }

            // Cập nhật thông tin
            user.HoTen = hoTen.Trim();
            user.SoDienThoai = soDienThoai?.Trim();
            user.DiaChi = diaChi?.Trim();

            // CCCD chỉ cập nhật cho TaiXe
            var roleName = HttpContext.Session.GetString("RoleName");
            if (roleName == "TaiXe" && !string.IsNullOrWhiteSpace(cccd))
                user.CCCD = cccd.Trim();

            await _db.SaveChangesAsync();

            // Cập nhật tên trong Session
            HttpContext.Session.SetString("HoTen", user.HoTen);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        // ── XEM ĐỔI MẬT KHẨU ──────────────────────────────────────────────
        [RequireLogin]
        public IActionResult ChangePassword() => View();

        // ── ĐỔI MẬT KHẨU ───────────────────────────────────────────────────
        [HttpPost]
        [RequireLogin]
        public async Task<IActionResult> ChangePassword(
            string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _db.Users.FindAsync(userId);

            if (user == null || user.PasswordHash != HashPassword(currentPassword))
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            if (HashPassword(newPassword) == user.PasswordHash)
            {
                ViewBag.Error = "Mật khẩu mới không được trùng mật khẩu cũ.";
                return View();
            }

            user.PasswordHash = HashPassword(newPassword);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
