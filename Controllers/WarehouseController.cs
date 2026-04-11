using FreightManagement.Data;
using FreightManagement.Filters;
using FreightManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreightManagement.Controllers
{
    [RequireLogin("QuanLyKho")]
    public class WarehouseController:Controller
    {
        private readonly AppDbContext _db;
        public WarehouseController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            ViewBag.ChoNhanVaoKho = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đang xử lý");
            ViewBag.DangTrongKho = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đã vào kho");
            ViewBag.DangVanChuyen = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đang vận chuyển");
            ViewBag.DaGiao = await _db.DonHangs.CountAsync(d => d.TrangThai == "Đã giao" && d.NgayCapNhat.Date == DateTime.Today);
            return View();
        }

        //public async Task<IActionResult> Incoming()
        //{
        //    var uid = HttpContext.Session.GetInt32("UserId")!.Value;
        //    var orders = await _db.DonHangs.Include(d => d.KhachHang)
        //        .Where(d => d.TrangThai == "Đang xử lý")
        //        .OrderBy(d => d.NgayTao).ToListAsync();
        //    ViewBag.Khos = await _db.KhoHangs.Where(k => k.MaQLK == uid).ToListAsync();
        //    return View(orders);
        //}

        public async Task<IActionResult> Incoming()
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;

            var orders = await _db.DonHangs
                .Include(d => d.KhachHang)
                .Where(d => d.TrangThai == "Đang xử lý")
                .OrderBy(d => d.NgayTao)
                .ToListAsync();

            ViewBag.Khos = await _db.QLK_KhoHangs
                .Include(q => q.KhoHang)
                .Where(q => q.MaQLK == uid)
                .Select(q => q.KhoHang)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> NhanVaoKho(int maDon, int maKho)
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = await _db.DonHangs.FindAsync(maDon);
            var kho = await _db.KhoHangs.FindAsync(maKho);
            if (order == null || kho == null) return NotFound();

            order.TrangThai = "Đã vào kho";
            order.MaQLK = uid;

            _db.HangTrongKhos.Add(new HangTrongKho { MaKho = maKho, MaDon = maDon });
            _db.LichSuTrangThais.Add(new LichSuTrangThai
            {
                MaDon = maDon,
                TrangThai = "Đã vào kho",
                GhiChu = $"Nhập kho tại {kho.TenKho}",
                CapNhatBoi = uid
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã nhận đơn #DH{maDon:D5} vào {kho.TenKho}!";
            return RedirectToAction("Incoming");
        }

        public async Task<IActionResult> Assign()
        {
            var orders = await _db.DonHangs
                .Include(d => d.KhachHang).Include(d => d.TaiXe)
                .Include(d => d.HangTrongKho).ThenInclude(h => h!.KhoHang)
                .Where(d => d.TrangThai == "Đã vào kho")
                .ToListAsync();
            ViewBag.TaiXes = await _db.Users.Where(u => u.RoleId == 4 && u.TrangThai == "HoatDong").ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> GanTaiXe(int maDon, int maTX)
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = await _db.DonHangs.FindAsync(maDon);
            var tx = await _db.Users.FindAsync(maTX);
            if (order == null || tx == null) return NotFound();

            order.MaTX = maTX;
            _db.LichSuTrangThais.Add(new LichSuTrangThai
            {
                MaDon = maDon,
                TrangThai = "Đã vào kho",
                GhiChu = $"Đã gán tài xế: {tx.HoTen}",
                CapNhatBoi = uid
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã gán tài xế {tx.HoTen} cho đơn #DH{maDon:D5}!";
            return RedirectToAction("Assign");
        }

        public async Task<IActionResult> Stock()
        {
            var items = await _db.HangTrongKhos
                .Include(h => h.KhoHang)
                .Include(h => h.DonHang).ThenInclude(d => d.KhachHang)
                .Include(h => h.DonHang).ThenInclude(d => d.TaiXe)
                .Where(h => h.ThoiGianXuatKho == null)
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Exported()
        {
            var items = await _db.HangTrongKhos
                .Include(h => h.KhoHang)
                .Include(h => h.DonHang).ThenInclude(d => d.KhachHang)
                .Where(h => h.ThoiGianXuatKho != null)
                .OrderByDescending(h => h.ThoiGianXuatKho)
                .ToListAsync();
            return View(items);
        }
    }
}
