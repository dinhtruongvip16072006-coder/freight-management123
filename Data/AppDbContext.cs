using FreightManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FreightManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<KhoHang> KhoHangs { get; set; }
        public DbSet<QLK_KhoHang> QLK_KhoHangs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<LichSuTrangThai> LichSuTrangThais { get; set; }
        public DbSet<HangTrongKho> HangTrongKhos { get; set; }
        public DbSet<ThongKeDoanhThu> ThongKeDoanhThus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<KhoHang>().ToTable("KhoHang");
            modelBuilder.Entity<QLK_KhoHang>().ToTable("QLK_KhoHang");
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<LichSuTrangThai>().ToTable("LichSuTrangThai");
            modelBuilder.Entity<HangTrongKho>().ToTable("HangTrongKho");
            modelBuilder.Entity<ThongKeDoanhThu>().ToTable("ThongKeDoanhThu");

            // Composite PK
            modelBuilder.Entity<HangTrongKho>()
                .HasKey(h => new { h.MaKho, h.MaDon });

            // DonHang relationships
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.KhachHang).WithMany(u => u.DonHangKhachHang)
                .HasForeignKey(d => d.MaKH).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.TaiXe).WithMany(u => u.DonHangTaiXe)
                .HasForeignKey(d => d.MaTX).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.QuanLyKho).WithMany(u => u.DonHangQuanLyKho)
                .HasForeignKey(d => d.MaQLK).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.ChiPhi).ValueGeneratedOnAddOrUpdate();

            // HangTrongKho relationships
            modelBuilder.Entity<HangTrongKho>()
                .HasOne(h => h.DonHang).WithOne(d => d.HangTrongKho)
                .HasForeignKey<HangTrongKho>(h => h.MaDon)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HangTrongKho>()
                .HasOne(h => h.KhoHang).WithMany(k => k.HangTrongKhos)
                .HasForeignKey(h => h.MaKho).OnDelete(DeleteBehavior.Restrict);

            // KhoHang relationships
            //modelBuilder.Entity<KhoHang>()
            //    .HasOne(k => k.QuanLyKho).WithMany()
            //    .HasForeignKey(k => k.MaQLK).OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<QLK_KhoHang>()
            //      .HasOne(q => q.KhoHang)
            //      .WithOne(k => k.QLK_KhoHang)
            //      .HasForeignKey<QLK_KhoHang>(q => q.MaKho)
            //      .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QLK_KhoHang>()
                    .HasOne(q => q.QuanLyKho)
                    .WithMany()
                    .HasForeignKey(q => q.MaQLK)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QLK_KhoHang>()
                .HasOne(q => q.QuanLyKho)
                .WithOne(u => u.QLK_KhoHang)
                .HasForeignKey<QLK_KhoHang>(q => q.MaQLK)
                .OnDelete(DeleteBehavior.Restrict);

            // LichSuTrangThai relationships
            modelBuilder.Entity<LichSuTrangThai>()
                .HasOne(l => l.DonHang).WithMany(d => d.LichSuTrangThais)
                .HasForeignKey(l => l.MaDon).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuTrangThai>()
                .HasOne(l => l.NguoiCapNhat).WithMany()
                .HasForeignKey(l => l.CapNhatBoi).OnDelete(DeleteBehavior.Restrict);

            // ThongKeDoanhThu relationships
            modelBuilder.Entity<ThongKeDoanhThu>()
                .HasOne(t => t.DonHang).WithMany()
                .HasForeignKey(t => t.MaDon).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThongKeDoanhThu>()
                .HasIndex(t => t.MaDon).IsUnique();

            // Khai báo triggers để EF Core không dùng OUTPUT clause
            modelBuilder.Entity<DonHang>()
                .ToTable("DonHang", tb => {
                    tb.HasTrigger("trg_LichSuTrangThai");
                    tb.HasTrigger("trg_ThongKeDoanhThu");
                    tb.HasTrigger("trg_HangXuatKho");
                    tb.HasTrigger("trg_TinhChiPhi");
                });

            modelBuilder.Entity<HangTrongKho>()
                .ToTable("HangTrongKho", tb => {
                    tb.HasTrigger("trg_HangVaoKho");
                });

        }   
    }
}
