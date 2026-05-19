using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HeThong_User.Models;

public partial class HeThongChiaSeTaiLieu_V1 : DbContext
{
    public HeThongChiaSeTaiLieu_V1()
    {
    }

    public HeThongChiaSeTaiLieu_V1(DbContextOptions<HeThongChiaSeTaiLieu_V1> options)
        : base(options)
    {
    }

    public virtual DbSet<BaoCaoViPham> BaoCaoViPhams { get; set; }

    public virtual DbSet<BinhLuan> BinhLuans { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DoQuy> DoQuies { get; set; }

    public virtual DbSet<GiangVien> GiangViens { get; set; }

    public virtual DbSet<HocKy> HocKies { get; set; }

    public virtual DbSet<Khoa> Khoas { get; set; }

    public virtual DbSet<LichSuDiem> LichSuDiems { get; set; }

    public virtual DbSet<LichSuTaiXuong> LichSuTaiXuongs { get; set; }

    public virtual DbSet<LoaiTaiLieu> LoaiTaiLieus { get; set; }

    public virtual DbSet<Lop> Lops { get; set; }

    public virtual DbSet<MonHoc> MonHocs { get; set; }

    public virtual DbSet<Nganh> Nganhs { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TaiLieu> TaiLieus { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TlyeuThich> TlyeuThiches { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
<<<<<<< Updated upstream
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\KIEUOANH;Database=HeThongChiaSeTaiLieu_V1;Trusted_Connection=True;TrustServerCertificate=True;");
=======
    {
        // Connection string được cấu hình trong Program.cs thông qua Dependency Injection
        // Không cần hardcode ở đây
        if (!optionsBuilder.IsConfigured)
        {
            // Chỉ sử dụng connection string mặc định nếu chưa được cấu hình
            // (trường hợp scaffold hoặc migration)
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-991F2DU\\KIEUOANH;Initial Catalog=SHAREDOCS;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30");
        }
    }
>>>>>>> Stashed changes

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaoCaoViPham>(entity =>
        {
            entity.HasKey(e => e.MaBaoCao).HasName("PK__BaoCaoVi__25A9188C3FB5B5B6");

            entity.ToTable("BaoCaoViPham");

            entity.Property(e => e.MaBaoCao)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LyDo).HasMaxLength(255);
            entity.Property(e => e.MaTaiLieu)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NgayBaoCao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayDuyet).HasColumnType("datetime");
            entity.Property(e => e.NguoiBaoCao)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TrangThaiXuLy).HasMaxLength(20);

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.BaoCaoViPhams)
                .HasForeignKey(d => d.MaTaiLieu)
                .HasConstraintName("FK__BaoCaoViP__MaTai__71D1E811");
        });

        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.HasKey(e => e.MaBl).HasName("PK__BinhLuan__272475AF949923DA");

            entity.ToTable("BinhLuan");

            entity.Property(e => e.MaBl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaBL");
            entity.Property(e => e.MaNd)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaND");
            entity.Property(e => e.MaTl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTL");
            entity.Property(e => e.NoiDung).HasMaxLength(200);
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaTlNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaTl)
                .HasConstraintName("FK__BinhLuan__MaTL__66603565");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.MaDg).HasName("PK__DanhGia__27258660FEBF9C4E");

            entity.Property(e => e.MaDg)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaDG");
            entity.Property(e => e.MaNd)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaND");
            entity.Property(e => e.MaTl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTL");
            entity.Property(e => e.SoSaoDg).HasColumnName("SoSaoDG");
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaTlNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaTl)
                .HasConstraintName("FK__DanhGia__MaTL__619B8048");
        });

        modelBuilder.Entity<DoQuy>(entity =>
        {
            entity.HasKey(e => e.MaDq).HasName("PK__DoQuy__2725866A5ECDA5CC");

            entity.ToTable("DoQuy");

            entity.Property(e => e.MaDq)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaDQ");
            entity.Property(e => e.DiemTl).HasColumnName("DiemTL");
        });

        modelBuilder.Entity<GiangVien>(entity =>
        {
            entity.HasKey(e => e.MaGv).HasName("PK__GiangVie__2725AEF3C2082FB4");

            entity.ToTable("GiangVien");

            entity.Property(e => e.MaGv)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaGV");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HocVi).HasMaxLength(100);
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaTk)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTK");
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.TenGv)
                .HasMaxLength(100)
                .HasColumnName("TenGV");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.GiangViens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName("FK__GiangVien__MaKho__46E78A0C");

            entity.HasOne(d => d.MaTkNavigation).WithMany(p => p.GiangViens)
                .HasForeignKey(d => d.MaTk)
                .HasConstraintName("FK__GiangVien__MaTK__47DBAE45");
        });

        modelBuilder.Entity<HocKy>(entity =>
        {
            entity.HasKey(e => e.MaHk).HasName("PK__HocKy__2725A6E7547A48F7");

            entity.ToTable("HocKy");

            entity.Property(e => e.MaHk)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaHK");
            entity.Property(e => e.NamHoc)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayBd).HasColumnName("NgayBD");
            entity.Property(e => e.NgayKt).HasColumnName("NgayKT");
            entity.Property(e => e.TenHk)
                .HasMaxLength(50)
                .HasColumnName("TenHK");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.MaKhoa).HasName("PK__Khoa__65390405BAEFDDC1");

            entity.ToTable("Khoa");

            entity.Property(e => e.MaKhoa)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenKhoa).HasMaxLength(100);
        });

        modelBuilder.Entity<LichSuDiem>(entity =>
        {
            entity.HasKey(e => e.MaLs).HasName("PK__LichSuDi__2725C772A761C606");

            entity.ToTable("LichSuDiem");

            entity.Property(e => e.MaLs)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaLS");
            entity.Property(e => e.LyDo).HasMaxLength(100);
            entity.Property(e => e.MaHk)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaHK");
            entity.Property(e => e.MaSv)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaSV");

            entity.HasOne(d => d.MaHkNavigation).WithMany(p => p.LichSuDiems)
                .HasForeignKey(d => d.MaHk)
                .HasConstraintName("FK__LichSuDiem__MaHK__52593CB8");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.LichSuDiems)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK__LichSuDiem__MaSV__5165187F");
        });

        modelBuilder.Entity<LichSuTaiXuong>(entity =>
        {
            entity.HasKey(e => e.MaDownTl).HasName("PK__LichSuTa__0F610348817C63F4");

            entity.ToTable("LichSuTaiXuong");

            entity.Property(e => e.MaDownTl).HasColumnName("MaDownTL");
            entity.Property(e => e.MaNd)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaND");
            entity.Property(e => e.MaTaiLieu)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NgayTai)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.LichSuTaiXuongs)
                .HasForeignKey(d => d.MaTaiLieu)
                .HasConstraintName("FK__LichSuTai__MaTai__6E01572D");
        });

        modelBuilder.Entity<LoaiTaiLieu>(entity =>
        {
            entity.HasKey(e => e.MaLtl).HasName("PK__LoaiTaiL__3B98371BEBB78431");

            entity.ToTable("LoaiTaiLieu");

            entity.Property(e => e.MaLtl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaLTL");
            entity.Property(e => e.TenLtl)
                .HasMaxLength(100)
                .HasColumnName("TenLTL");
        });

        modelBuilder.Entity<Lop>(entity =>
        {
            entity.HasKey(e => e.MaLop).HasName("PK__Lop__3B98D2732D55E1B9");

            entity.ToTable("Lop");

            entity.Property(e => e.MaLop)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenLop).HasMaxLength(100);

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Lops)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName("FK__Lop__MaKhoa__3C69FB99");
        });

        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.HasKey(e => e.MaMonHoc).HasName("PK__MonHoc__4127737F34BAFEBE");

            entity.ToTable("MonHoc");

            entity.Property(e => e.MaMonHoc)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaNganh)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenMonHoc).HasMaxLength(100);

            entity.HasOne(d => d.MaNganhNavigation).WithMany(p => p.MonHocs)
                .HasForeignKey(d => d.MaNganh)
                .HasConstraintName("FK__MonHoc__MaNganh__3F466844");
        });

        modelBuilder.Entity<Nganh>(entity =>
        {
            entity.HasKey(e => e.MaNganh).HasName("PK__Nganh__A2CEF50D39CBBB21");

            entity.ToTable("Nganh");

            entity.Property(e => e.MaNganh)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenNganh).HasMaxLength(100);

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Nganhs)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName("FK__Nganh__MaKhoa__398D8EEE");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.MaSv).HasName("PK__SinhVien__2725081AD83304FE");

            entity.ToTable("SinhVien");

            entity.Property(e => e.MaSv)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaSV");
            entity.Property(e => e.DiemTichLuy).HasDefaultValue(0);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.MaLop)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaTk)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTK");
            entity.Property(e => e.TenSv)
                .HasMaxLength(100)
                .HasColumnName("TenSV");
            entity.Property(e => e.TrangThaiSv)
                .HasMaxLength(100)
                .HasColumnName("TrangThaiSV");

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.MaLop)
                .HasConstraintName("FK__SinhVien__MaLop__4BAC3F29");

            entity.HasOne(d => d.MaTkNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.MaTk)
                .HasConstraintName("FK__SinhVien__MaTK__4CA06362");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk).HasName("PK__TaiKhoan__2725007034254627");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.MaTk)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTK");
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TenTk)
                .HasMaxLength(100)
                .HasColumnName("TenTK");
            entity.Property(e => e.TrangThai).HasMaxLength(100);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaVaiTro)
                .HasConstraintName("FK__TaiKhoan__MaVaiT__440B1D61");
        });

        modelBuilder.Entity<TaiLieu>(entity =>
        {
            entity.HasKey(e => e.MaTaiLieu).HasName("PK__TaiLieu__FD18A6578C647D3E");

            entity.ToTable("TaiLieu");

            entity.Property(e => e.MaTaiLieu)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DiemYeuCau).HasColumnName("diemYeuCau");
            entity.Property(e => e.DuongDanFile)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LanTaiBan)
                .HasDefaultValue(1)
                .HasColumnName("lanTaiBan");
            entity.Property(e => e.LoaiFile)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LuotTai).HasDefaultValue(0);
            entity.Property(e => e.LyDoTuChoi).HasMaxLength(255);
            entity.Property(e => e.MaBaoCao)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("maBaoCao");
            entity.Property(e => e.MaMonHoc)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("maMonHoc");
            entity.Property(e => e.MaNguoiDang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("maNguoiDang");
            entity.Property(e => e.MaNguoiDuyetKhoa)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("maNguoiDuyetKhoa");
            entity.Property(e => e.MaloaiTl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("maloaiTL");
            entity.Property(e => e.NamXb).HasColumnName("namXB");
            entity.Property(e => e.NgayDang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngayDang");
            entity.Property(e => e.Nxb)
                .HasMaxLength(100)
                .HasColumnName("NXB");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThaiDuyet)
                .HasMaxLength(20)
                .HasColumnName("trangThaiDuyet");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaMonHoc)
                .HasConstraintName("FK__TaiLieu__maMonHo__5812160E");

            entity.HasOne(d => d.MaloaiTlNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaloaiTl)
                .HasConstraintName("FK__TaiLieu__maloaiT__5AEE82B9");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaTb).HasName("PK__ThongBao__2725006F4923E849");

            entity.ToTable("ThongBao");

            entity.Property(e => e.MaTb)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTB");
            entity.Property(e => e.LoaiThongBao).HasMaxLength(100);
            entity.Property(e => e.MaNguoiNhan)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaTl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTL");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(500);
            entity.Property(e => e.TieuDe).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.MaTlNavigation).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.MaTl)
                .HasConstraintName("FK__ThongBao__MaTL__5EBF139D");
        });

        modelBuilder.Entity<TlyeuThich>(entity =>
        {
            entity.HasKey(e => e.MaTll).HasName("PK__TLYeuThi__3149CE19F4C31454");

            entity.ToTable("TLYeuThich");

            entity.Property(e => e.MaTll)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTLL");
            entity.Property(e => e.MaNd)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaND");
            entity.Property(e => e.MaTl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaTL");
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaTlNavigation).WithMany(p => p.TlyeuThiches)
                .HasForeignKey(d => d.MaTl)
                .HasConstraintName("FK__TLYeuThich__MaTL__6A30C649");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF7B50AC0D");

            entity.ToTable("VaiTro");

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
