using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using UltraStrore.Data;
using UltraStrore.Data.Temp;

namespace UltraStrore.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BinhLuan> BinhLuans { get; set; } = null!;
        public virtual DbSet<ChiTietComBo> ChiTietComBos { get; set; } = null!;
        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; } = null!;
        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; } = null!;
        public virtual DbSet<ComBoSanPham> ComBoSanPhams { get; set; } = null!;
        public virtual DbSet<DanhSachDiaChi> DanhSachDiaChis { get; set; } = null!;
        public virtual DbSet<DonHang> DonHangs { get; set; } = null!;
        public virtual DbSet<GioHang> GioHangs { get; set; } = null!;
        public virtual DbSet<HinhAnh> HinhAnhs { get; set; } = null!;
        public virtual DbSet<LoaiSanPham> LoaiSanPhams { get; set; } = null!;
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
        public virtual DbSet<SanPham> SanPhams { get; set; } = null!;
        public virtual DbSet<ThuongHieu> ThuongHieus { get; set; } = null!;
        public virtual DbSet<Video> Videos { get; set; } = null!;
        public virtual DbSet<Voucher> Vouchers { get; set; } = null!;
        public virtual DbSet<Coupon> Coupons { get; set; } = null!;
        public virtual DbSet<YeuThich> YeuThichs { get; set; } = null!;
        public virtual DbSet<LienHe> LienHes { get; set; } = null!;
        public virtual DbSet<TinNhan> TinNhans { get; set; } = null!;
        public virtual DbSet<ChiTietGioHangSupport> GioHangSupports { get; set; }
        public virtual DbSet<GiaoDien> GiaoDiens { get; set; } = null!;
        public virtual DbSet<KichThuoc> KichThuocs { get; set; } = null!;
        public DbSet<PendingOder> PendingOrders { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PendingOder>(entity =>
            {
                modelBuilder.Entity<PendingOder>()
                    .HasKey(p => p.TempOrderId);

                modelBuilder.Entity<PendingOder>()
                    .HasIndex(p => p.TempOrderId)
                    .IsUnique();
            });
            modelBuilder.Entity<ChiTietGioHangSupport>(entity =>
            {
                entity.HasKey(e => e.ID).HasName("PK__SupportGioHang");
                entity.Property(e => e.ID).ValueGeneratedOnAdd();
                entity.Property(e => e.MaSanPham)
                      .IsRequired()
                      .HasMaxLength(20);
                entity.Property(e => e.ChiTietGioHang);
                entity.Property(e => e.MaChiTietCombo);

                entity.Property(e => e.SoLuong);
                entity.Property(e => e.Version);

            });
            modelBuilder.Entity<BinhLuan>(entity =>
            {
                entity.HasKey(e => e.MaBinhLuan).HasName("PK__BINH_LUA__300DD2D899CEB418");

                entity.ToTable("BINH_LUAN");

                entity.HasIndex(e => e.MaSanPham, "IX_BINH_LUAN_ma_san_pham");

                entity.Property(e => e.MaBinhLuan).HasColumnName("ma_binh_luan");
                entity.Property(e => e.DanhGia).HasColumnName("danh_gia");
                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");
                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");
                entity.Property(e => e.NoiDungBinhLuan)
                    .HasMaxLength(255)
                    .HasColumnName("noi_dung_binh_luan");
                entity.Property(e => e.SoTimBinhLuan).HasColumnName("so_binh_luan");


            });
            modelBuilder.Entity<ChiTietComBo>(entity =>
            {
                entity.HasKey(e => e.MaChiTietComBo).HasName("PK__CHI_TIET__1B885A91D07009B0");

                entity.ToTable("CHI_TIET_COM_BO");

                entity.HasIndex(e => e.MaComBo, "IX_CHI_TIET_COM_BO_ma_com_bo");

                entity.Property(e => e.MaChiTietComBo).HasColumnName("ma_chi_tiet_com_bo");
                entity.Property(e => e.MaComBo).HasColumnName("ma_com_bo");
                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");
                entity.Property(e => e.SoLuong).HasColumnName("so_luong");

                entity.HasOne(d => d.MaComBoNavigation).WithMany(p => p.ChiTietComBos)
                    .HasForeignKey(d => d.MaComBo)
                    .HasConstraintName("FK_CHI_TIET_COM_BO_COM_BO_SAN_PHAM");
            });

            modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.MaCtgh).HasName("PK__CHI_TIET__5AE495EDA6F2A925");

            entity.ToTable("CHI_TIET_GIO_HANG");

            entity.HasIndex(e => e.MaGioHang, "IX_CHI_TIET_GIO_HANG_ma_gio_hang");

            entity.Property(e => e.MaCtgh).HasColumnName("ma_ctgh");
            entity.Property(e => e.Gia).HasColumnName("gia");
            entity.Property(e => e.MaCombo).HasColumnName("ma_combo");
            entity.Property(e => e.MaGioHang).HasColumnName("ma_gio_hang");
            entity.Property(e => e.MaSanPham)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_san_pham");
            entity.Property(e => e.SoLuong).HasColumnName("so_luong");
            entity.Property(e => e.ThanhTien).HasColumnName("thanh_tien");
            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGioHang)
                .HasConstraintName("FK_CHI_TIET_GIO_HANG_GIO_HANG");
        });

            modelBuilder.Entity<ComBoSanPham>(entity =>
            {
                entity.HasKey(e => e.MaComBo).HasName("PK__COM_BO_S__AF6434A35D0F47EB");

                entity.ToTable("COM_BO_SAN_PHAM");

                entity.Property(e => e.MaComBo)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_com_bo");
                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");
                entity.Property(e => e.SoLuong).HasColumnName("so_luong");
                entity.Property(e => e.TenComBo)
                    .HasMaxLength(100)
                    .HasColumnName("ten_com_bo");
                entity.Property(e => e.TongGia).HasColumnName("tong_gia");
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai");
            });

            modelBuilder.Entity<DanhSachDiaChi>(entity =>
            {
                entity.HasKey(e => e.MaDiaChi).HasName("PK__DANH_SAC__80439859B4824340");

                entity.ToTable("DANH_SACH_DIA_CHI");

                entity.HasIndex(e => e.MaNguoiDung, "IX_DANH_SACH_DIA_CHI_ma_nguoi_dung");

                entity.Property(e => e.MaDiaChi).HasColumnName("ma_dia_chi");
                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .HasColumnName("dia_chi");
                entity.Property(e => e.HoTen)
                    .HasMaxLength(100)
                    .HasColumnName("ho_ten");
                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");
                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");
                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .HasColumnName("sdt");
                entity.Property(e => e.PhuongXa)
                    .HasMaxLength(255)
                    .HasColumnName("phuong_xa");
                entity.Property(e => e.QuanHuyen)
                    .HasMaxLength(255)
                    .HasColumnName("quan_huyen");
                entity.Property(e => e.Tinh)
                    .HasMaxLength(255)
                    .HasColumnName("tinh");

                entity.Property(e => e.TrangThai)
                    .HasColumnName("trang_thai");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhSachDiaChis)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK_DANH_SACH_DIA_CHI_NGUOI_DUNG");
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDonHang).HasName("PK__DON_HANG__0246C5EAB291F99F");

                entity.ToTable("DON_HANG");

                entity.HasIndex(e => e.MaNguoiDung, "IX_DON_HANG_ma_nguoi_dung");

                entity.HasIndex(e => e.MaNhanVien, "IX_DON_HANG_ma_nhan_vien");

                entity.Property(e => e.MaDonHang).HasColumnName("ma_don_hang");
                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .HasColumnName("dia_chi");
                entity.Property(e => e.LyDoHuy)
                    .HasMaxLength(255)
                    .HasColumnName("ly_do_huy");
                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");
                entity.Property(e => e.MaNhanVien)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nhan_vien");
                entity.Property(e => e.NgayDat).HasColumnName("ngay_dat");
                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .HasColumnName("sdt");
                entity.Property(e => e.TenNguoiNhan)
                    .HasMaxLength(100)
                    .HasColumnName("ten_nguoi_nhan");
                entity.Property(e => e.TrangThaiDonHang) // Thay đổi thành int
                    .HasColumnName("trang_thai_don_hang");
                entity.Property(e => e.TrangThaiHang) // Thay đổi thành int
                    .HasColumnName("trang_thai_hang");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonHangMaNguoiDungNavigations)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK_DON_HANG_NGUOI_DUNG");
            });

            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasKey(e => e.MaGioHang).HasName("PK__GIO_HANG__6C00DDA3D3ED86DE");

                entity.ToTable("GIO_HANG");

                entity.HasIndex(e => e.MaNguoiDung, "IX_GIO_HANG_ma_nguoi_dung");

                entity.Property(e => e.MaGioHang).HasColumnName("ma_gio_hang")
                .ValueGeneratedOnAdd();
                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");

                entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.GioHangs)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK_GIO_HANG_NGUOI_DUNG");
            });

            modelBuilder.Entity<HinhAnh>(entity =>
            {
                entity.HasKey(e => e.MaHinhAnh).HasName("PK__HINH_ANH__5AE49D8E2E189F36");

                entity.ToTable("HINH_ANH");

                entity.HasIndex(e => e.MaBinhLuan, "IX_HINH_ANH_ma_binh_luan");

                entity.HasIndex(e => e.MaSanPham, "IX_HINH_ANH_ma_san_pham");

                entity.Property(e => e.MaHinhAnh).HasColumnName("ma_hinh_anh");
                entity.Property(e => e.Data).HasColumnName("data");
                entity.Property(e => e.MaBinhLuan).HasColumnName("ma_binh_luan");
                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");
                entity.Property(e => e.TenHinhAnh)
                    .HasMaxLength(100)
                    .HasColumnName("ten_hinh_anh");


            });

            modelBuilder.Entity<LoaiSanPham>(entity =>
            {
                entity.HasKey(e => e.MaLoaiSanPham).HasName("PK__LOAI_SAN__7FEA3693092D4574");

                entity.ToTable("LOAI_SAN_PHAM");

                entity.Property(e => e.MaLoaiSanPham).HasColumnName("ma_loai_san_pham");
                entity.Property(e => e.TenLoaiSanPham)
                    .HasMaxLength(100)
                    .HasColumnName("ten_loai_san_pham");
                entity.Property(e => e.KiHieu)
                    .HasMaxLength(10)
                    .HasColumnName("ki_hieu");
                entity.Property(e => e.HinhAnh)
                    .HasColumnName("hinh_anh");
            });
            modelBuilder.Entity<KichThuoc>(entity =>
            {
                entity.HasKey(e => e.MaKichThuoc).HasName("DMCUOCDOI");
                entity.ToTable("KICH_THUOC");
                entity.Property(e => e.MaKichThuoc).ValueGeneratedOnAdd();
                entity.Property(e => e.MaLoai).HasColumnName("ma_loai");
                entity.Property(e => e.TenKichThuoc).HasMaxLength(5).HasColumnName("collect");
            });
            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.MaNguoiDung).HasName("PK__NGUOI_DU__19C32CF72B9CA41E");

                entity.ToTable("NGUOI_DUNG");

                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");
                entity.Property(e => e.Cccd)
                    .HasMaxLength(20)
                    .HasColumnName("cccd");
                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .HasColumnName("dia_chi");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");
                entity.Property(e => e.HinhAnh)
                    .HasColumnName("hinh_anh");
                entity.Property(e => e.HoTen)
                    .HasMaxLength(100)
                    .HasColumnName("ho_ten");
                entity.Property(e => e.MoTa)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("mo_ta");
                entity.Property(e => e.MatKhau)
                    .HasMaxLength(255)
                    .HasColumnName("mat_khau");
                entity.Property(e => e.TaiKhoan)
                .HasMaxLength(50).HasColumnName("tai_khoan");
                entity.Property(e => e.NgayTao).HasColumnName("ngay_dang_ky");
                entity.Property(e => e.NgaySinh).HasColumnName("ngay_sinh");
                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .HasColumnName("sdt");
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai");
                entity.Property(e => e.GioiTinh).HasColumnName("gioi_tinh");
            });

            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.HasKey(e => e.MaSanPham).HasName("PK__SAN_PHAM__9D25990C2713A687");

                entity.ToTable("SAN_PHAM");

                entity.HasIndex(e => e.MaLoaiSanPham, "IX_SAN_PHAM_ma_loai_san_pham");

                entity.HasIndex(e => e.MaThuongHieu, "IX_SAN_PHAM_ma_thuong_hieu");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");
                entity.Property(e => e.Example).HasColumnName("example");
                entity.Property(e => e.Gia).HasColumnName("gia");
                entity.Property(e => e.KichThuoc)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("kich_thuoc");
                entity.Property(e => e.MaLoaiSanPham).HasColumnName("ma_loai_san_pham");
                entity.Property(e => e.MaThuongHieu).HasColumnName("ma_thuong_hieu");
                entity.Property(e => e.MoTa)
                    .HasMaxLength(150)
                    .HasColumnName("mo_ta");
                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");
                entity.Property(e => e.SoLuong).HasColumnName("so_luong");
                entity.Property(e => e.TenSanPham)
                    .HasMaxLength(100)
                    .HasColumnName("ten_san_pham");

                entity.HasOne(d => d.MaLoaiSanPhamNavigation).WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaLoaiSanPham)
                    .HasConstraintName("FK_SAN_PHAM_LOAI_SAN_PHAM");

                entity.HasOne(d => d.MaThuongHieuNavigation).WithMany(p => p.SanPhams)
                    .HasForeignKey(d => d.MaThuongHieu)
                    .HasConstraintName("FK_SAN_PHAM_THUONG_HIEU");
            });

            modelBuilder.Entity<ThuongHieu>(entity =>
            {
                entity.HasKey(e => e.MaThuongHieu).HasName("PK__THUONG_H__C4FB3F32AE837BB2");

                entity.ToTable("THUONG_HIEU");

                entity.Property(e => e.MaThuongHieu).HasColumnName("ma_thuong_hieu");
                entity.Property(e => e.TenThuongHieu)
                    .HasMaxLength(100)
                    .HasColumnName("ten_thuong_hieu");

                entity.Property(e => e.HinhAnh)
                    .HasColumnName("hinh_anh");
            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.MaVideo).HasName("PK__VIDEO__946AD5B8DC15860F");

                entity.ToTable("VIDEO");

                entity.HasIndex(e => e.MaSanPham, "IX_VIDEO_ma_san_pham");

                entity.Property(e => e.MaVideo).HasColumnName("ma_video");
                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");
                entity.Property(e => e.TenVideo)
                    .HasMaxLength(100)
                    .HasColumnName("ten_video");

                entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.Videos)
                    .HasForeignKey(d => d.MaSanPham)
                    .HasConstraintName("FK_VIDEO_SAN_PHAM");
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.MaVoucher).HasName("PK__VOUCHER__466D17BE6217DC60");

                entity.ToTable("VOUCHER");

                entity.Property(e => e.MaVoucher)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_voucher");
                entity.Property(e => e.DieuKien)
                    .HasMaxLength(255)
                    .HasColumnName("dieu_kien");
                entity.Property(e => e.GiaTri).HasColumnName("giam_gia");
                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");
                entity.Property(e => e.NgayBatDau).HasColumnName("ngay_bat_dau");
                entity.Property(e => e.NgayKetThuc).HasColumnName("ngay_ket_thuc");
                entity.Property(e => e.SoLuong).HasColumnName("so_luong");
                entity.Property(e => e.TenVoucher)
                    .HasMaxLength(100)
                    .HasColumnName("ten_voucher");
                entity.Property(e => e.TrangThai).HasColumnName("trang_thai");
            });

            modelBuilder.Entity<YeuThich>(entity =>
            {
                entity.HasKey(e => e.MaYeuThich).HasName("PK__YEU_THIC__6427C4C6C61640D4");

                entity.ToTable("YEU_THICH");

                entity.HasIndex(e => e.MaSanPham, "IX_YEU_THICH_ma_san_pham");

                entity.Property(e => e.MaYeuThich).HasColumnName("ma_yeu_thich");
                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");
                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");
                entity.Property(e => e.NgayYeuThich).HasColumnName("ngay_yeu_thich");


            });

            modelBuilder.Entity<BinhLuan>(entity =>
            {
                entity.HasKey(e => e.MaBinhLuan)
                    .HasName("PK__BINH_LUA__300DD2D899CEB418");

                entity.ToTable("BINH_LUAN");

                entity.HasIndex(e => e.MaSanPham, "IX_BINH_LUAN_ma_san_pham");

                entity.Property(e => e.MaBinhLuan)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_binh_luan");

                entity.Property(e => e.DanhGia).HasColumnName("danh_gia");

                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_nguoi_dung");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");

                entity.Property(e => e.NoiDungBinhLuan)
                    .HasMaxLength(255)
                    .HasColumnName("noi_dung_binh_luan");

                entity.Property(e => e.SoTimBinhLuan).HasColumnName("so_tim_binh_luan");

            });

            modelBuilder.Entity<ChiTietComBo>(entity =>
            {
                entity.HasKey(e => e.MaChiTietComBo)
                    .HasName("PK__CHI_TIET__1B885A91D07009B0");

                entity.ToTable("CHI_TIET_COM_BO");

                entity.HasIndex(e => e.MaComBo, "IX_CHI_TIET_COM_BO_ma_com_bo");

                entity.HasIndex(e => e.MaSanPham, "IX_CHI_TIET_COM_BO_ma_san_pham");

                entity.Property(e => e.MaChiTietComBo)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_chi_tiet_com_bo");

                entity.Property(e => e.MaComBo).HasColumnName("ma_com_bo");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");

                entity.Property(e => e.SoLuong).HasColumnName("so_luong");

                entity.HasOne(d => d.MaComBoNavigation)
                    .WithMany(p => p.ChiTietComBos)
                    .HasForeignKey(d => d.MaComBo)
                    .HasConstraintName("FK_CHI_TIET_COM_BO_COM_BO_SAN_PHAM");
            });

            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.HasKey(e => e.MaCtdh).HasName("PK__CHI_TIET__5AE49D8E95395895");

                entity.ToTable("CHI_TIET_DON_HANG");

                entity.Property(e => e.MaCtdh).HasColumnName("ma_ctdh");
                entity.Property(e => e.MaDonHang).HasColumnName("ma_don_hang").HasColumnType("int"); // Đảm bảo là int
                entity.Property(e => e.MaSanPham).HasColumnName("ma_san_pham");
                entity.Property(e => e.SoLuong).HasColumnName("so_luong");
                entity.Property(e => e.Gia).HasColumnName("gia");
                entity.Property(e => e.ThanhTien).HasColumnName("thanh_tien");
                entity.Property(e => e.MaCombo).HasColumnName("ma_combo");
                entity.Property(e => e.SanPhamMaSanPham).HasColumnName("SanPhamMaSanPham");

                // Cấu hình mối quan hệ với DonHang
                entity.HasOne(d => d.MaDonHangNavigation)
                      .WithMany(p => p.ChiTietDonHangs)
                      .HasForeignKey(d => d.MaDonHang)
                      .HasConstraintName("FK_CHI_TIET_DON_HANG_DON_HANG");

                // Cấu hình mối quan hệ với SanPham
                entity.HasOne(d => d.MaSanPhamNavigation)
                      .WithMany()
                      .HasForeignKey(d => d.MaSanPham)
                      .HasConstraintName("FK_CHI_TIET_DON_HANG_SAN_PHAM");

                // Cấu hình mối quan hệ với ComBoSanPham
                entity.HasOne(d => d.MaComboNavigation)
                      .WithMany()
                      .HasForeignKey(d => d.MaCombo)
                      .HasConstraintName("FK_CHI_TIET_DON_HANG_COM_BO_SAN_PHAM");
            });

            modelBuilder.Entity<ChiTietGioHang>(entity =>
            {
                entity.HasKey(e => e.MaCtgh)
                    .HasName("PK__CHI_TIET__5AE495EDA6F2A925");

                entity.ToTable("CHI_TIET_GIO_HANG");

                entity.HasIndex(e => e.MaCombo, "IX_CHI_TIET_GIO_HANG_ma_combo");

                entity.HasIndex(e => e.MaGioHang, "IX_CHI_TIET_GIO_HANG_ma_gio_hang");

                entity.HasIndex(e => e.MaSanPham, "IX_CHI_TIET_GIO_HANG_ma_san_pham");

                entity.Property(e => e.MaCtgh)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_ctgh");

                entity.Property(e => e.Gia).HasColumnName("gia");

                entity.Property(e => e.MaCombo).HasColumnName("ma_combo");

                entity.Property(e => e.MaGioHang).HasColumnName("ma_gio_hang");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");

                entity.Property(e => e.SoLuong).HasColumnName("so_luong");

                entity.Property(e => e.ThanhTien).HasColumnName("thanh_tien");

                entity.HasOne(d => d.MaComboNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaCombo)
                    .HasConstraintName("FK_CHI_TIET_GIO_HANG_COM_BO_SAN_PHAM");

                entity.HasOne(d => d.MaGioHangNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaGioHang)
                    .HasConstraintName("FK_CHI_TIET_GIO_HANG_GIO_HANG");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaSanPham)
                    .HasConstraintName("FK_CHI_TIET_GIO_HANG_SAN_PHAM");
            });

            modelBuilder.Entity<ComBoSanPham>(entity =>
            {
                entity.HasKey(e => e.MaComBo)
                    .HasName("PK__COM_BO_S__AF6434A35D0F47EB");

                entity.ToTable("COM_BO_SAN_PHAM");

                entity.Property(e => e.MaComBo)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_com_bo");

                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");

                entity.Property(e => e.SoLuong).HasColumnName("so_luong");

                entity.Property(e => e.TenComBo)
                    .HasMaxLength(100)
                    .HasColumnName("ten_com_bo");
                entity.Property(e => e.TongGia).HasColumnName("tong_gia");

                entity.Property(e => e.TrangThai).HasColumnName("trang_thai");
            });

            modelBuilder.Entity<DanhSachDiaChi>(entity =>
            {
                entity.HasKey(e => e.MaDiaChi)
                    .HasName("PK__DANH_SAC__80439859B4824340");

                entity.ToTable("DANH_SACH_DIA_CHI");

                entity.HasIndex(e => e.MaNguoiDung, "IX_DANH_SACH_DIA_CHI_ma_nguoi_dung");

                entity.Property(e => e.MaDiaChi)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_dia_chi");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .HasColumnName("dia_chi");

                entity.Property(e => e.HoTen)
                    .HasMaxLength(100)
                    .HasColumnName("ho_ten");

                entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");

                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .HasColumnName("sdt");

                entity.Property(e => e.TrangThai)
                      .HasColumnName("trang_thai");

                entity.HasOne(d => d.MaNguoiDungNavigation)
                    .WithMany(p => p.DanhSachDiaChis)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK_DANH_SACH_DIA_CHI_NGUOI_DUNG");
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDonHang)
                    .HasName("PK__DON_HANG__0246C5EAB291F99F");

                entity.ToTable("DON_HANG");

                entity.HasIndex(e => e.MaNguoiDung, "IX_DON_HANG_ma_nguoi_dung");

                entity.HasIndex(e => e.MaNhanVien, "IX_DON_HANG_ma_nhan_vien");

                entity.Property(e => e.MaDonHang)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_don_hang");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .HasColumnName("dia_chi");

                entity.Property(e => e.LyDoHuy)
                    .HasMaxLength(255)
                    .HasColumnName("ly_do_huy");

                entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");

                entity.Property(e => e.MaNhanVien).HasColumnName("ma_nhan_vien");

                entity.Property(e => e.NgayDat)
                    .HasColumnType("date")
                    .HasColumnName("ngay_dat");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .HasColumnName("sdt");

                entity.Property(e => e.TenNguoiNhan)
                    .HasMaxLength(100)
                    .HasColumnName("ten_nguoi_nhan");

                entity.Property(e => e.TrangThaiDonHang)
                    .HasMaxLength(50)
                    .HasColumnName("trang_thai_don_hang");

                entity.Property(e => e.TrangThaiHang)
                    .HasMaxLength(50)
                    .HasColumnName("trang_thai_hang");

                entity.HasOne(d => d.MaNguoiDungNavigation)
                    .WithMany(p => p.DonHangMaNguoiDungNavigations)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK_DON_HANG_NGUOI_DUNG1");
            });

            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasKey(e => e.MaGioHang)
                    .HasName("PK__GIO_HANG__6C00DDA3D3ED86DE");

                entity.ToTable("GIO_HANG");

                entity.HasIndex(e => e.MaNguoiDung, "IX_GIO_HANG_ma_nguoi_dung");

                entity.Property(e => e.MaGioHang)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_gio_hang");

                entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");

                entity.HasOne(d => d.MaNguoiDungNavigation)
                    .WithMany(p => p.GioHangs)
                    .HasForeignKey(d => d.MaNguoiDung)
                    .HasConstraintName("FK_GIO_HANG_NGUOI_DUNG");
            });

            modelBuilder.Entity<HinhAnh>(entity =>
            {
                entity.HasKey(e => e.MaHinhAnh)
                    .HasName("PK__HINH_ANH__5AE49D8E2E189F36");

                entity.ToTable("HINH_ANH");

                entity.HasIndex(e => e.MaBinhLuan, "IX_HINH_ANH_ma_binh_luan");

                entity.HasIndex(e => e.MaSanPham, "IX_HINH_ANH_ma_san_pham");

                entity.Property(e => e.MaHinhAnh)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_hinh_anh");

                entity.Property(e => e.MaBinhLuan).HasColumnName("ma_binh_luan");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");

                entity.Property(e => e.TenHinhAnh)
                    .HasMaxLength(100)
                    .HasColumnName("ten_hinh_anh");



            });

            modelBuilder.Entity<LoaiSanPham>(entity =>
            {
                entity.HasKey(e => e.MaLoaiSanPham)
                    .HasName("PK__LOAI_SAN__7FEA3693092D4574");

                entity.ToTable("LOAI_SAN_PHAM");

                entity.Property(e => e.MaLoaiSanPham)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_loai_san_pham");

                entity.Property(e => e.TenLoaiSanPham)
                    .HasMaxLength(100)
                    .HasColumnName("ten_loai_san_pham");

                entity.Property(e => e.HinhAnh)
                    .HasColumnName("hinh_anh");
            });

            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.MaNguoiDung)
                    .HasName("PK__NGUOI_DU__19C32CF72B9CA41E");

                entity.ToTable("NGUOI_DUNG");

                entity.Property(e => e.MaNguoiDung)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_nguoi_dung");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(20)
                    .HasColumnName("cccd");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .HasColumnName("dia_chi");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");


                entity.Property(e => e.HoTen)
                    .HasMaxLength(100)
                    .HasColumnName("ho_ten");

                entity.Property(e => e.MatKhau)
                      .HasColumnType("nvarchar(max)")
                      .HasColumnName("mat_khau");


                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");

                entity.Property(e => e.NgaySinh)
                    .HasColumnType("date")
                    .HasColumnName("ngay_sinh");

                entity.Property(e => e.NgayTao)
                    .HasColumnType("date")
                    .HasColumnName("ngay_tao");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .HasColumnName("sdt");

                entity.Property(e => e.TrangThai).HasColumnName("trang_thai");
            });

            modelBuilder.Entity<ThuongHieu>(entity =>
            {
                entity.HasKey(e => e.MaThuongHieu)
                    .HasName("PK__THUONG_H__C4FB3F32AE837BB2");

                entity.ToTable("THUONG_HIEU");

                entity.Property(e => e.MaThuongHieu)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_thuong_hieu");

                entity.Property(e => e.TenThuongHieu)
                    .HasMaxLength(100)
                    .HasColumnName("ten_thuong_hieu");

                entity.Property(e => e.HinhAnh)
                    .HasColumnName("hinh_anh");
            });

            modelBuilder.Entity<KichThuoc>(entity =>
            {
                entity.HasKey(e => e.MaKichThuoc)
                    .HasName("PK_KICH_THUOC");

                entity.ToTable("KICH_THUOC");

                entity.Property(e => e.MaKichThuoc)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_kich_thuoc");

                entity.Property(e => e.TenKichThuoc)
                    .HasMaxLength(100)
                    .HasColumnName("ten_kich_thuoc");

            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.MaVideo)
                    .HasName("PK__VIDEO__946AD5B8DC15860F");

                entity.ToTable("VIDEO");

                entity.HasIndex(e => e.MaSanPham, "IX_VIDEO_ma_san_pham");

                entity.Property(e => e.MaVideo)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_video");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");

                entity.Property(e => e.TenVideo)
                    .HasMaxLength(100)
                    .HasColumnName("ten_video");

                entity.HasOne(d => d.MaSanPhamNavigation)
                    .WithMany(p => p.Videos)
                    .HasForeignKey(d => d.MaSanPham)
                    .HasConstraintName("FK_VIDEO_SAN_PHAM");
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.MaVoucher)
                    .HasName("PK__VOUCHER__466D17BE6217DC60");

                entity.ToTable("VOUCHER");

                entity.Property(e => e.MaVoucher)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ma_voucher");

                entity.Property(e => e.DieuKien)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("dieu_kien");

                entity.Property(e => e.GiaTri).HasColumnName("giam_gia");

                entity.Property(e => e.MoTa)
                    .HasMaxLength(255)
                    .HasColumnName("mo_ta");

                entity.Property(e => e.NgayBatDau)
                    .HasColumnType("date")
                    .HasColumnName("ngay_bat_dau");

                entity.Property(e => e.NgayKetThuc)
                    .HasColumnType("date")
                    .HasColumnName("ngay_ket_thuc");

                entity.Property(e => e.SoLuong).HasColumnName("so_luong");

                entity.Property(e => e.TenVoucher)
                    .HasMaxLength(100)
                    .HasColumnName("ten_voucher");

                entity.Property(e => e.TrangThai).HasColumnName("trang_thai");
            });

            modelBuilder.Entity<YeuThich>(entity =>
            {
                entity.HasKey(e => e.MaYeuThich)
                    .HasName("PK__YEU_THIC__6427C4C6C61640D4");

                entity.ToTable("YEU_THICH");

                entity.HasIndex(e => e.MaSanPham, "IX_YEU_THICH_ma_san_pham");

                entity.Property(e => e.MaYeuThich)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_yeu_thich");

                entity.Property(e => e.MaNguoiDung).HasColumnName("ma_nguoi_dung");

                entity.Property(e => e.MaSanPham)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ma_san_pham");

                entity.Property(e => e.NgayYeuThich)
                    .HasColumnType("date")
                    .HasColumnName("ngay_yeu_thich");

            });

            modelBuilder.Entity<TinNhan>(entity =>
            {
                entity.HasKey(e => e.MaTinNhan).HasName("PK_TIN_NHAN");

                entity.ToTable("TIN_NHAN");

                entity.Property(e => e.MaTinNhan)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ma_tin_nhan");

                // Đồng bộ kiểu và độ dài với MaNguoiDung (varchar(10))
                entity.Property(e => e.NguoiGuiId)
                    .IsRequired()
                    .HasMaxLength(10)           // Trùng với MaNguoiDung
                    .IsUnicode(false)
                    .HasColumnName("nguoi_gui_id");

                entity.Property(e => e.NguoiNhanId)
                    .IsRequired()
                    .HasMaxLength(10)           // Trùng với MaNguoiDung
                    .IsUnicode(false)
                    .HasColumnName("nguoi_nhan_id");

                entity.Property(e => e.NoiDung)
                    .IsRequired()
                    .HasColumnName("noi_dung");

                entity.Property(e => e.NgayTao)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()")
                    .HasColumnName("ngay_tao");

                entity.Property(e => e.TrangThai)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("trang_thai");
                // Cấu hình quan hệ với NguoiGui
                entity.HasOne(d => d.NguoiGui)
                    .WithMany(p => p.TinNhanGui)
                    .HasForeignKey(d => d.NguoiGuiId)
                    .HasConstraintName("FK_TINNHAN_NGUOIGUI")
                    .OnDelete(DeleteBehavior.NoAction);

                // Cấu hình quan hệ với NguoiNhan
                entity.HasOne(d => d.NguoiNhan)
                    .WithMany(p => p.TinNhanNhan)
                    .HasForeignKey(d => d.NguoiNhanId)
                    .HasConstraintName("FK_TINNHAN_NGUOINHAN")
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<LienHe>(entity =>
            {
                entity.HasKey(e => e.MaLienHe)
                      .HasName("PK_LIEN_HE");

                entity.ToTable("LIEN_HE");

                entity.Property(e => e.MaLienHe)
                      .HasColumnName("ma_lien_he");

                entity.Property(e => e.HoTen)
                      .HasMaxLength(100)
                      .HasColumnName("ho_ten");

                entity.Property(e => e.Sdt)
                      .HasMaxLength(15)
                      .HasColumnName("sdt");

                entity.Property(e => e.Email)
                      .HasMaxLength(100)
                      .HasColumnName("email");

                entity.Property(e => e.NoiDung)
                      .HasColumnName("noi_dung");

                entity.Property(e => e.TrangThai)
                      .HasColumnName("trang_thai");

                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");
            });

            modelBuilder.Entity<GiaoDien>(entity =>
            {
                entity.HasKey(e => e.MaGiaoDien)
                      .HasName("PK_GIAO_DIEN");

                entity.ToTable("GIAO_DIEN");

                entity.Property(e => e.MaGiaoDien)
                      .HasColumnName("ma_giao_dien");

                entity.Property(e => e.TenGiaoDien)
                      .HasMaxLength(100)
                      .HasColumnName("ho_ten");

                entity.Property(e => e.Logo)
                      .HasColumnName("logo");

                entity.Property(e => e.Slider1)
                      .HasColumnName("slider1");

                entity.Property(e => e.Slider2)
                      .HasColumnName("slider2");

                entity.Property(e => e.Slider3)
                      .HasColumnName("slider3");

                entity.Property(e => e.Slider4)
                      .HasColumnName("slider4");

                entity.Property(e => e.Avt)
                      .HasColumnName("avt");

                entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");

                entity.Property(e => e.TrangThai)
                      .HasColumnName("trang_thai");
            });     
            modelBuilder.Entity<Voucher>()
        .ToTable("VOUCHER");

            modelBuilder.Entity<Coupon>()
        .ToTable("Coupons");

            // Thiết lập quan hệ
            modelBuilder.Entity<Coupon>()
                .HasOne(c => c.MaVoucherNavigation)
                .WithMany(v => v.Coupons)
                .HasForeignKey(c => c.MaVoucher);


            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
