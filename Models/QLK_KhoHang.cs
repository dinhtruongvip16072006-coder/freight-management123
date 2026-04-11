using Microsoft.EntityFrameworkCore;

namespace FreightManagement.Models
{
    public class QLK_KhoHang
    {
        public int QLK_KhoHangID { get; set; }

        public int? MaKho { get; set; }
        public int? MaQLK { get; set; }

        public KhoHang? KhoHang { get; set; }
        public User? QuanLyKho { get; set; }
    }
}
