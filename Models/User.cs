using System.ComponentModel.DataAnnotations;

namespace FreightManagement.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100), Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required, MaxLength(100), EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20), Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [MaxLength(255), Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [MaxLength(20), Display(Name = "CCCD")]
        public string? CCCD { get; set; }

        [MaxLength(50), Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "HoatDong";

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // FK
        public int RoleId { get; set; }

        // Navigation

        public QLK_KhoHang? QLK_KhoHang { get; set; }
        public Role Role { get; set; } = null!;
        public ICollection<DonHang> DonHangKhachHang { get; set; } = new List<DonHang>();
        public ICollection<DonHang> DonHangTaiXe { get; set; } = new List<DonHang>();
        public ICollection<DonHang> DonHangQuanLyKho { get; set; } = new List<DonHang>();
    }
}
