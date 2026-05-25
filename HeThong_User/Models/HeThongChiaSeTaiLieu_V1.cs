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

    public virtual DbSet<DanhGia> DanhGia { get; set; }

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

    public virtual DbSet<TLYeuThich> TLYeuThiches { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Không cần hardcode connection string ở đây
        // Connection string sẽ được inject từ Program.cs qua constructor
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback nếu không có options được inject (chỉ dùng khi test)
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-991F2DU\\KIEUOANH;Initial Catalog=SHAREDOCS;Integrated Security=True;TrustServerCertificate=True");
        }
    }

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

            entity.HasOne(d => d.NguoiBaoCaoNavigation).WithMany(p => p.BaoCaoViPhams)
                .HasForeignKey(d => d.NguoiBaoCao)
                .HasConstraintName("FK__BaoCaoViP__Nguoi__72C60C4A");
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

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.BinhLuans)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("FK__BinhLuan__MaND__6754599E");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDg).HasName("PK__DanhGia__27258660FEBF9C4E");

            entity.ToTable("DanhGia");

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

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("FK__DanhGia__MaND__628FA481");
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

            entity.ToTable("GiangVien", tb => tb.HasTrigger("trg_CreateAccountForGV"));

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
            entity.Property(e => e.LoaiGv)
                .HasMaxLength(50)
                .HasDefaultValue("GV")
                .HasColumnName("loaiGV");
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
            entity.HasKey(e => e.MaLS).HasName("PK__LichSuDi__2725C772A761C606");

            entity.ToTable("LichSuDiem");

            entity.Property(e => e.MaLS)
                .HasColumnName("MaLS")
                .ValueGeneratedOnAdd();
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
            entity.HasKey(e => e.MaDownTL).HasName("PK__LichSuTa__0F610348817C63F4");

            entity.ToTable("LichSuTaiXuong");

            entity.Property(e => e.MaDownTL)
                .HasColumnName("MaDownTL")
                .ValueGeneratedOnAdd();
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

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.LichSuTaiXuongs)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("FK__LichSuTai__MaND__6EF57B66");
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
            entity.Property(e => e.MaDq)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaDQ");

            entity.HasOne(d => d.MaDqNavigation).WithMany(p => p.LoaiTaiLieus)
                .HasForeignKey(d => d.MaDq)
                .HasConstraintName("FK__LoaiTaiLi__MaDQ");
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

            entity.ToTable("SinhVien", tb => tb.HasTrigger("trg_CreateAccountForSV"));

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
            entity.Property(e => e.TenSv)
                .HasMaxLength(100)
                .HasColumnName("TenSV");
            entity.Property(e => e.TrangThaiSv)
                .HasMaxLength(100)
                .HasColumnName("TrangThaiSV");

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.MaLop)
                .HasConstraintName("FK__SinhVien__MaLop__4BAC3F29");
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
                .IsUnicode(false)
                .HasDefaultValue("123456");
            entity.Property(e => e.TenTk)
                .HasMaxLength(100)
                .HasColumnName("TenTK");
            entity.Property(e => e.TrangThai).HasDefaultValue(1);
            entity.Property(e => e.MaGv)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaGV");
            entity.Property(e => e.MaSv)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaSV");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName("FK__TaiKhoan__MaGV__5EBF139D");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK__TaiKhoan__MaSV__5FB337D6");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaVaiTro)
                .HasConstraintName("FK_TaiKhoan_VaiTro");
        });

        modelBuilder.Entity<TaiLieu>(entity =>
        {
            entity.HasKey(e => e.MaTaiLieu).HasName("PK__TaiLieu__FD18A6578C647D3E");

            entity.ToTable("TaiLieu");

            entity.Property(e => e.MaTaiLieu)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DiemYeuCau).HasColumnName("DiemYeuCau");
            entity.Property(e => e.DuongDanFile)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LanTaiBan)
                .HasDefaultValue(1)
                .HasColumnName("LanTaiBan");
            entity.Property(e => e.LoaiFile)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LuotTai).HasDefaultValue(0);
            entity.Property(e => e.LyDoTuChoi).HasMaxLength(255);
            entity.Property(e => e.MaMonHoc)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaMonHoc");
            entity.Property(e => e.MaNguoiDang)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaNguoiDang");
            entity.Property(e => e.MaNguoiDuyetKhoa)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaNguoiDuyetKhoa");
            entity.Property(e => e.MaLoaiTl)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MaLoaiTL");
            entity.Property(e => e.NamXb).HasColumnName("NamXB");
            entity.Property(e => e.NgayDang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("NgayDang");
            entity.Property(e => e.Nxb)
                .HasMaxLength(100)
                .HasColumnName("NXB");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
            entity.Property(e => e.TrangThaiDuyet)
                .HasMaxLength(20)
                .HasDefaultValueSql("(N'Chờ duyệt')")
                .HasColumnName("TrangThaiDuyet");

            entity.HasOne(d => d.MaMonHocNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaMonHoc)
                .HasConstraintName("FK__TaiLieu__MaMonHo__5812160E");

            entity.HasOne(d => d.MaLoaiTlNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaLoaiTl)
                .HasConstraintName("FK__TaiLieu__MaLoaiT__5AEE82B9");

            entity.HasOne(d => d.MaNguoiDangNavigation).WithMany(p => p.TaiLieuMaNguoiDangNavigations)
                .HasForeignKey(d => d.MaNguoiDang)
                .HasConstraintName("FK__TaiLieu__MaNguoi__59063A47");

            entity.HasOne(d => d.MaNguoiDuyetKhoaNavigation).WithMany(p => p.TaiLieuMaNguoiDuyetKhoaNavigations)
                .HasForeignKey(d => d.MaNguoiDuyetKhoa)
                .HasConstraintName("FK__TaiLieu__MaNguoi__59FA5E80");
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

            entity.HasOne(d => d.MaNguoiNhanNavigation).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.MaNguoiNhan)
                .HasConstraintName("FK__ThongBao__MaNguo__5DCAEF64");
        });

        modelBuilder.Entity<TLYeuThich>(entity =>
        {
            entity.HasKey(e => e.MaYeuThich).HasName("PK__TLYeuThi__3149CE19F4C31454");

            entity.ToTable("TLYeuThich");

            entity.Property(e => e.MaYeuThich)
                .HasColumnName("MaYeuThich")
                .ValueGeneratedOnAdd();
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
            
            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.TlyeuThiches)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("FK__TLYeuThich__MaND__6B24EA82");
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
