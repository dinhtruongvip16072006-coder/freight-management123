namespace FreightManagement.Models
{
    public class HangTrongKho
    {
        public int MaKho { get; set; }
        public int MaDon { get; set; }
        public DateTime ThoiGianVaoKho { get; set; } = DateTime.Now;
        public DateTime? ThoiGianXuatKho { get; set; }

        // Navigation
        public KhoHang KhoHang { get; set; } = null!;
        public DonHang DonHang { get; set; } = null!;
    }
}
