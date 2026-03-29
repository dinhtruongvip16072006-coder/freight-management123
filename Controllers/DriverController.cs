using FreightManagement.Data;
using FreightManagement.Filters;
using FreightManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreightManagement.Controllers
{
    [RequireLogin("TaiXe")]
    public class DriverController : Controller
    {
        private readonly AppDbContext _db;
        public DriverController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            ViewBag.ChoXuLy = await _db.DonHangs.CountAsync(d => d.MaTX == uid && d.TrangThai == "Đã vào kho");
            ViewBag.DangGiao = await _db.DonHangs.CountAsync(d => d.MaTX == uid && d.TrangThai == "Đang vận chuyển");
            ViewBag.HoanThanh = await _db.DonHangs.CountAsync(d => d.MaTX == uid && d.TrangThai == "Đã giao");
            return View();
        }

        public async Task<IActionResult> MyOrders()
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var orders = await _db.DonHangs
                .Where(d => d.MaTX == uid && d.TrangThai == "Đã vào kho")
                .OrderBy(d => d.NgayCapNhat).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ChapNhan(int maDon)
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTX == uid);
            if (order == null) return NotFound();

            order.TrangThai = "Đang vận chuyển";
            _db.LichSuTrangThais.Add(new LichSuTrangThai
            {
                MaDon = maDon,
                TrangThai = "Đang vận chuyển",
                GhiChu = "Tài xế đã nhận hàng và bắt đầu vận chuyển",
                CapNhatBoi = uid
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xác nhận nhận đơn #DH{maDon:D5}, bắt đầu vận chuyển!";
            return RedirectToAction("MyOrders");
        }

        public async Task<IActionResult> Delivering()
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var orders = await _db.DonHangs
                .Where(d => d.MaTX == uid && d.TrangThai == "Đang vận chuyển")
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> GiaoThanhCong(int maDon)
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDon == maDon && d.MaTX == uid);
            if (order == null) return NotFound();

            order.TrangThai = "Đã giao";
            _db.LichSuTrangThais.Add(new LichSuTrangThai
            {
                MaDon = maDon,
                TrangThai = "Đã giao",
                GhiChu = "Giao hàng thành công đến người nhận",
                CapNhatBoi = uid
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã giao thành công đơn #DH{maDon:D5}!";
            return RedirectToAction("Delivering");
        }

        public async Task<IActionResult> Completed()
        {
            var uid = HttpContext.Session.GetInt32("UserId")!.Value;
            var orders = await _db.DonHangs
                .Where(d => d.MaTX == uid && d.TrangThai == "Đã giao")
                .OrderByDescending(d => d.NgayCapNhat).ToListAsync();
            return View(orders);
        }
    }
}
