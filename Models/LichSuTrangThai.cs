using System.ComponentModel.DataAnnotations;

namespace FreightManagement.Models
{
    public class LichSuTrangThai
    {
        [Key]
        public int MaLS { get; set; }

        public int MaDon { get; set; }

        [Required, MaxLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        public int? CapNhatBoi { get; set; }

        // Navigation
        public DonHang DonHang { get; set; } = null!;
        public User? NguoiCapNhat { get; set; }
    }
}
