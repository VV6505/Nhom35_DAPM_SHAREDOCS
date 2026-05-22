namespace HeThong_User.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public int TongTaiLieu { get; set; }
        public int DangChoDuyet { get; set; }
        public int TongSinhVien { get; set; }
        public int TongGiangVien { get; set; }
        public int TongBaoCaoViPham { get; set; }

        // Tài liệu theo khoa
        public List<TaiLieuTheoKhoa> TaiLieuTheoKhoas { get; set; } = new();

        // Top sinh viên tích cực
        public List<TopSinhVien> TopSinhViens { get; set; } = new();

        // Báo cáo vi phạm theo lý do
        public List<BaoCaoTheoLyDo> BaoCaoTheoLyDos { get; set; } = new();
    }

    public class TaiLieuTheoKhoa
    {
        public string TenKhoa { get; set; } = "";
        public int SoTaiLieu { get; set; }
        public string MauSac { get; set; } = "";
    }

    public class TopSinhVien
    {
        public string TenSV { get; set; } = "";
        public int DiemTichLuy { get; set; }
        public string Lop { get; set; } = "";
    }

    public class BaoCaoTheoLyDo
    {
        public string LyDo { get; set; } = "";
        public int SoBaoCao { get; set; }
        public int ChoXuLy { get; set; }
        public int DaXuLy { get; set; }
    }
}
