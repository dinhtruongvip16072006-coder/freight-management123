// =====================================================================
// Controllers/AdminController.cs — ĐẦY ĐỦ (thay thế file cũ)
// =====================================================================
using FreightManagement.Data;
using FreightManagement.Filters;
using FreightManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreightManagement.Controllers
{
    [RequireLogin("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) { _db = db; }

        // ── DASHBOARD ──────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // Thống kê đơn hàng
            ViewBag.TongDonHang = await _db.DonHangs.CountAsync();
            ViewBag.DangXuLy = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đang xử lý");
            ViewBag.DangVanChuyen = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đang vận chuyển");
            ViewBag.DaGiao = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đã giao");

            // Thống kê hệ thống
            ViewBag.TongKhachHang = await _db.Users.CountAsync(u => u.RoleId == 2);
            ViewBag.TongTaiXe = await _db.Users.CountAsync(u => u.RoleId == 4);
            ViewBag.TongKho = await _db.KhoHangs.CountAsync();
            ViewBag.DoanhThu = await _db.ThongKeDoanhThus
                                        .SumAsync(t => (int?)t.ChiPhiVanChuyen) ?? 0;

            // 10 đơn hàng gần nhất
            ViewBag.RecentOrders = await _db.DonHangs
                .Include(d => d.KhachHang)
                .OrderByDescending(d => d.NgayTao)
                .Take(10)
                .ToListAsync();

            return View();
        }

        // ── QUẢN LÝ USERS ──────────────────────────────────────────
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.RoleId)
                .ThenBy(u => u.HoTen)
                .ToListAsync();
            return View(users);
        }

        // Khóa / mở khóa tài khoản
        public async Task<IActionResult> ToggleUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null && user.RoleId != 1) // không khóa Admin
            {
                user.TrangThai = user.TrangThai == "HoatDong" ? "BiKhoa" : "HoatDong";
                await _db.SaveChangesAsync();
                TempData["Success"] = user.TrangThai == "HoatDong"
                    ? $"Đã mở khóa tài khoản {user.HoTen}."
                    : $"Đã khóa tài khoản {user.HoTen}.";
            }
            return RedirectToAction("Users");
        }

        // ── XEM TẤT CẢ ĐƠN HÀNG ──────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var orders = await _db.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.TaiXe)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();
            return View(orders);
        }

        // ── KHO HÀNG ──────────────────────────────────────────────
        //public async Task<IActionResult> Warehouses()
        //{
        //    //var khos = await _db.KhoHangs
        //    //    .Include(k => k.QuanLyKho)
        //    //    .ToListAsync();
        //    var khos = await _db.QLK_KhoHangs.Include(d => d.KhoHang).Include(d => d.QuanLyKho).ToListAsync();
        //    return View(khos);
        //}

        public async Task<IActionResult> Warehouses()
        {
            var khos = await _db.QLK_KhoHangs
                .Include(q => q.KhoHang)
                .Include(q => q.QuanLyKho)
                .Select(q => new KhoHang
                {
                    MaKho = q.KhoHang!.MaKho,
                    TenKho = q.KhoHang.TenKho,
                    DiaChiKho = q.KhoHang.DiaChiKho,
                    SucChua = q.KhoHang.SucChua,
                    SoLuongHienTai = q.KhoHang.SoLuongHienTai,

                    // gán 1 quản lý (mỗi kho 1 dòng vì UNIQUE)
                    QuanLyKho = q.QuanLyKho
                })
                .ToListAsync();

            return View(khos);
        }

        // ── THỐNG KÊ DOANH THU ───────────────────────────────────
        public async Task<IActionResult> Revenue()
        {
            var data = await _db.ThongKeDoanhThus
                .Include(t => t.DonHang)
                .OrderByDescending(t => t.Ngay)
                .ToListAsync();
            return View(data);
        }
    }
}
