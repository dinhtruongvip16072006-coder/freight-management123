using System.ComponentModel.DataAnnotations;

namespace FreightManagement.Models
{
    public class KhoHang
    {
        [Key]
        public int MaKho { get; set; }

        [Required, MaxLength(100), Display(Name = "Tên kho")]
        public string TenKho { get; set; } = string.Empty;

        [Required, MaxLength(255), Display(Name = "Địa chỉ kho")]
        public string DiaChiKho { get; set; } = string.Empty;

        [Display(Name = "Sức chứa tối đa")]
        public int SucChua { get; set; } = 1000;

        [Display(Name = "Số lượng hiện tại")]
        public int SoLuongHienTai { get; set; } = 0;

        // FK - Quản lý kho phụ trách
        //public int? MaQLK { get; set; }

        // Navigation
        public User? QuanLyKho { get; set; }
        public QLK_KhoHang? QLK_KhoHang { get; set; }
        public ICollection<HangTrongKho> HangTrongKhos { get; set; } = new List<HangTrongKho>();
    }
}
