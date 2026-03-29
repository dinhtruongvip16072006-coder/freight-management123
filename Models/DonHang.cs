using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreightManagement.Models
{
    public class DonHang
    {
        [Key]
        public int MaDon { get; set; }

        // FKs
        public int MaKH { get; set; }
        public int? MaTX { get; set; }
        public int? MaQLK { get; set; }

        [Required, MaxLength(255), Display(Name = "Địa chỉ gửi")]
        public string DiaChiGui { get; set; } = string.Empty;

        [Required, MaxLength(100), Display(Name = "Tên người nhận")]
        public string TenNguoiNhan { get; set; } = string.Empty;

        [Required, MaxLength(255), Display(Name = "Địa chỉ nhận")]
        public string DiaChiNhan { get; set; } = string.Empty;

        [Required, MaxLength(20), Display(Name = "SĐT người nhận")]
        public string SdtNguoiNhan { get; set; } = string.Empty;

        [MaxLength(255), Display(Name = "Mô tả hàng")]
        public string? MoTaHang { get; set; }

        [Range(1, int.MaxValue), Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        [Range(1000, int.MaxValue), Display(Name = "Đơn giá (VNĐ)")]
        public int DonGia { get; set; } = 10000;

        // Computed: ChiPhi = SoLuong * DonGia (tính ở DB, đọc-only ở C#)
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Display(Name = "Chi phí")]
        public int ChiPhi { get; set; }

        [MaxLength(50), Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đang xử lý";

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Navigation
        public User KhachHang { get; set; } = null!;
        public User? TaiXe { get; set; }
        public User? QuanLyKho { get; set; }
        public ICollection<LichSuTrangThai> LichSuTrangThais { get; set; } = new List<LichSuTrangThai>();
        public HangTrongKho? HangTrongKho { get; set; }
    }
}
