using System.ComponentModel.DataAnnotations;

namespace FreightManagement.Models
{
    public class ThongKeDoanhThu
    {
        [Key]
        public int MaThongKe { get; set; }
        public DateOnly Ngay { get; set; }
        public int MaDon { get; set; }
        public int ChiPhiVanChuyen { get; set; }

        // Navigation
        public DonHang DonHang { get; set; } = null!;
    }
}
