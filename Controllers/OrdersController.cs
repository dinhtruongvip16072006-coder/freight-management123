using FreightManagement.Data;
using FreightManagement.Filters;
using FreightManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreightManagement.Controllers
{
    [RequireLogin("KhachHang")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) { _db = db; }

        private static readonly List<string> Provinces = new()
        {
            "An Giang","Bà Rịa - Vũng Tàu","Bắc Giang","Bắc Kạn","Bạc Liêu","Bắc Ninh",
            "Bến Tre","Bình Định","Bình Dương","Bình Phước","Bình Thuận","Cà Mau","Cần Thơ",
            "Cao Bằng","Đà Nẵng","Đắk Lắk","Đắk Nông","Điện Biên","Đồng Nai","Đồng Tháp",
            "Gia Lai","Hà Giang","Hà Nam","Hà Nội","Hà Tĩnh","Hải Dương","Hải Phòng",
            "Hậu Giang","Hòa Bình","Hưng Yên","Khánh Hòa","Kiên Giang","Kon Tum","Lai Châu",
            "Lâm Đồng","Lạng Sơn","Lào Cai","Long An","Nam Định","Nghệ An","Ninh Bình",
            "Ninh Thuận","Phú Thọ","Phú Yên","Quảng Bình","Quảng Nam","Quảng Ngãi",
            "Quảng Ninh","Quảng Trị","Sóc Trăng","Sơn La","Tây Ninh","Thái Bình","Thái Nguyên",
            "Thanh Hóa","Thừa Thiên Huế","Tiền Giang","TP. Hồ Chí Minh","Trà Vinh","Tuyên Quang",
            "Vĩnh Long","Vĩnh Phúc","Yên Bái"
        };

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var orders = await _db.DonHangs
                .Include(d => d.TaiXe)
                .Where(d => d.MaKH == userId)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _db.Users.FindAsync(userId);

            ViewBag.HoTen = user?.HoTen;             
            ViewBag.SoDienThoai = user?.SoDienThoai;        
            ViewBag.DiaChi = user?.DiaChi;
            ViewBag.Provinces = Provinces;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create (string TenNguoiNhan, string SdtNguoiNhan,
            string TinhNhan, string DiaChiNhan,
            string? MoTaHang, int SoLuong, string LoaiHang)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _db.Users.FindAsync(userId);
            int donGia = 0;
            if (LoaiHang == "HangHoa")
                donGia = 25000;
            else if (LoaiHang == "ThuTu")
                donGia = 20000;
            else
                donGia = 30000;

            var order = new DonHang
            {
                    MaKH = userId,
                    DiaChiGui = user?.DiaChi ?? "Chưa cập nhật",
                    TenNguoiNhan = TenNguoiNhan,
                    SdtNguoiNhan = SdtNguoiNhan,
                    DiaChiNhan = $"{DiaChiNhan}, {TinhNhan}",
                    MoTaHang = MoTaHang,
                    SoLuong = SoLuong,
                    DonGia = donGia,
                    TrangThai = "Đang xử lý"
            };
            _db.DonHangs.Add(order);
            await _db.SaveChangesAsync();
            //Ghi lịch sử
            _db.LichSuTrangThais.Add(new LichSuTrangThai
            {
                MaDon = order.MaDon,
                TrangThai = "Đang xử lý",
                GhiChu = "Đơn hàng đã được đặt thành công",
                CapNhatBoi = userId
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đặt đơn hàng #DH{order.MaDon:D5} thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = await _db.DonHangs
                .Include(d => d.TaiXe)
                .Include(d => d.LichSuTrangThais)
                .Include(d => d.HangTrongKho).ThenInclude(h => h!.KhoHang)
                .FirstOrDefaultAsync(d => d.MaDon == id && d.MaKH == userId);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var order = await _db.DonHangs.FirstOrDefaultAsync(d => d.MaDon == id && d.MaKH == userId);
            if (order == null || order.TrangThai != "Đang xử lý")
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
                return RedirectToAction("Index");
            }
            order.TrangThai = "Đã hủy";
            _db.LichSuTrangThais.Add(new LichSuTrangThai
            {
                MaDon = id,
                TrangThai = "Đã hủy",
                GhiChu = "Khách hàng hủy đơn",
                CapNhatBoi = userId
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction();
        }
    }
}
