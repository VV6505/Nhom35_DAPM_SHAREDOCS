using System.Diagnostics;
using HeThong_Admin.Models;
using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HeThongChiaSeTaiLieu_V1 _context;

        public HomeController(ILogger<HomeController> logger, HeThongChiaSeTaiLieu_V1 context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            var adminRole = HttpContext.Session.GetString("AdminRole");
            var colors = new[] { "#3b82f6", "#06b6d4", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899", "#14b8a6" };

            string? maKhoaCBK = null;
            if (adminRole == "VT004" || adminRole == "VT002")
            {
                var tk = await _context.TaiKhoans
                    .Include(x => x.MaGvNavigation)
                    .FirstOrDefaultAsync(x => x.MaTk == adminId);
                maKhoaCBK = tk?.MaGvNavigation?.MaKhoa;
            }

            // 1. Tổng tài liệu
            var tlQuery = _context.TaiLieus.AsQueryable();
            if (maKhoaCBK != null)
            {
                tlQuery = tlQuery.Where(t => t.MaMonHocNavigation != null && t.MaMonHocNavigation.MaNganhNavigation != null && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoaCBK);
            }
            var tongTaiLieu = await tlQuery.CountAsync();
            var dangChoDuyet = await tlQuery.CountAsync(t => t.TrangThaiDuyet == "Chờ duyệt");

            // 2. Tổng sinh viên
            var svQuery = _context.SinhViens.AsQueryable();
            if (maKhoaCBK != null)
            {
                svQuery = svQuery.Where(s => s.MaLopNavigation != null && s.MaLopNavigation.MaKhoa == maKhoaCBK);
            }
            var tongSinhVien = await svQuery.CountAsync();
            var tongGiangVien = await _context.GiangViens.CountAsync(g => maKhoaCBK == null || g.MaKhoa == maKhoaCBK);
            var tongBaoCao = await _context.BaoCaoViPhams.CountAsync(b => maKhoaCBK == null || (b.MaTaiLieuNavigation != null && b.MaTaiLieuNavigation.MaMonHocNavigation != null && b.MaTaiLieuNavigation.MaMonHocNavigation.MaNganhNavigation != null && b.MaTaiLieuNavigation.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoaCBK));

            // 3. Biểu đồ tài liệu theo khoa (Nếu là CBK thì hiển thị theo Ngành trong khoa đó)
            List<TaiLieuTheoKhoa> taiLieuStats;
            if (maKhoaCBK != null)
            {
                taiLieuStats = await _context.TaiLieus
                    .Include(t => t.MaMonHocNavigation)
                        .ThenInclude(m => m!.MaNganhNavigation)
                    .Where(t => t.MaMonHocNavigation != null && t.MaMonHocNavigation.MaNganhNavigation != null && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoaCBK)
                    .GroupBy(t => t.MaMonHocNavigation!.MaNganhNavigation!.TenNganh)
                    .Select(g => new TaiLieuTheoKhoa
                    {
                        TenKhoa = g.Key, // Gán tạm tên ngành vào TenKhoa để dùng chung Model
                        SoTaiLieu = g.Count()
                    })
                    .OrderByDescending(x => x.SoTaiLieu)
                    .ToListAsync();
            }
            else
            {
                taiLieuStats = await _context.TaiLieus
                    .Include(t => t.MaMonHocNavigation)
                        .ThenInclude(m => m!.MaNganhNavigation)
                            .ThenInclude(n => n!.MaKhoaNavigation)
                    .Where(t => t.MaMonHocNavigation != null && t.MaMonHocNavigation.MaNganhNavigation != null && t.MaMonHocNavigation.MaNganhNavigation.MaKhoaNavigation != null)
                    .GroupBy(t => t.MaMonHocNavigation!.MaNganhNavigation!.MaKhoaNavigation!.TenKhoa)
                    .Select(g => new TaiLieuTheoKhoa
                    {
                        TenKhoa = g.Key,
                        SoTaiLieu = g.Count()
                    })
                    .OrderByDescending(x => x.SoTaiLieu)
                    .ToListAsync();
            }

            for (int i = 0; i < taiLieuStats.Count; i++)
                taiLieuStats[i].MauSac = colors[i % colors.Length];

            // 4. Top sinh viên
            var topSinhVien = await svQuery
                .Include(s => s.MaLopNavigation)
                .OrderByDescending(s => s.DiemTichLuy)
                .Take(5)
                .Select(s => new TopSinhVien
                {
                    TenSV = s.TenSv,
                    DiemTichLuy = s.DiemTichLuy ?? 0,
                    Lop = s.MaLopNavigation != null ? s.MaLopNavigation.TenLop : "—"
                })
                .ToListAsync();

            // 5. Báo cáo vi phạm
            var baoCaoQuery = _context.BaoCaoViPhams.AsQueryable();
            if (maKhoaCBK != null)
            {
                baoCaoQuery = baoCaoQuery.Where(b => b.MaTaiLieuNavigation != null && b.MaTaiLieuNavigation.MaMonHocNavigation != null && b.MaTaiLieuNavigation.MaMonHocNavigation.MaNganhNavigation != null && b.MaTaiLieuNavigation.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoaCBK);
            }
            var allBaoCao = await baoCaoQuery.ToListAsync();
            var baoCaoTheoLyDo = allBaoCao
                .GroupBy(b => string.IsNullOrEmpty(b.LyDo) ? "Khác" : b.LyDo)
                .Select(g => new BaoCaoTheoLyDo
                {
                    LyDo = g.Key ?? "Khác",
                    SoBaoCao = g.Count(),
                    ChoXuLy = g.Count(x => x != null && (x.TrangThaiXuLy == null || x.TrangThaiXuLy == "Chờ xử lý" || x.TrangThaiXuLy == "ChờXửLý")),
                    DaXuLy = g.Count(x => x != null && (x.TrangThaiXuLy == "Đã xử lý" || x.TrangThaiXuLy == "Bỏ qua"))
                })
                .OrderByDescending(x => x.SoBaoCao)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TongTaiLieu = tongTaiLieu,
                DangChoDuyet = dangChoDuyet,
                TongSinhVien = tongSinhVien,
                TongGiangVien = tongGiangVien,
                TongBaoCaoViPham = tongBaoCao,
                TaiLieuTheoKhoas = taiLieuStats,
                TopSinhViens = topSinhVien,
                BaoCaoTheoLyDos = baoCaoTheoLyDo
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> XuatBaoCao(string LoaiBaoCao, string DinhDang, DateTime TuNgay, DateTime DenNgay)
        {
            var endOfDay = DenNgay.AddDays(1).AddTicks(-1);
            var sb = new System.Text.StringBuilder();

            if (LoaiBaoCao == "TaiLieu")
            {
                var data = await _context.TaiLieus
                    .Where(t => t.NgayDang >= TuNgay && t.NgayDang <= endOfDay)
                    .Select(t => new { t.MaTaiLieu, t.TieuDe, t.TrangThaiDuyet, NgayDang = t.NgayDang.HasValue ? t.NgayDang.Value.ToString("yyyy-MM-dd") : "" })
                    .ToListAsync();
                sb.AppendLine("MaTaiLieu,TieuDe,TrangThai,NgayDang");
                foreach (var item in data)
                    sb.AppendLine($"{item.MaTaiLieu},{item.TieuDe?.Replace(",", " ")},{item.TrangThaiDuyet},{item.NgayDang}");
            }
            else if (LoaiBaoCao == "NguoiDung")
            {
                var data = await _context.TaiKhoans
                    .Select(t => new { t.MaTk, t.TenTk, t.TrangThai })
                    .ToListAsync();
                sb.AppendLine("MaTaiKhoan,TenTaiKhoan,TrangThai");
                foreach (var item in data)
                    sb.AppendLine($"{item.MaTk},{item.TenTk?.Replace(",", " ")},{item.TrangThai}");
            }
            else if (LoaiBaoCao == "ViPham")
            {
                var data = await _context.BaoCaoViPhams
                    .Where(b => b.NgayBaoCao >= TuNgay && b.NgayBaoCao <= endOfDay)
                    .Select(b => new { b.MaBaoCao, b.LyDo, b.TrangThaiXuLy, NgayBC = b.NgayBaoCao.HasValue ? b.NgayBaoCao.Value.ToString("yyyy-MM-dd") : "" })
                    .ToListAsync();
                sb.AppendLine("MaBaoCao,LyDo,TrangThai,NgayBaoCao");
                foreach (var item in data)
                    sb.AppendLine($"{item.MaBaoCao},{item.LyDo?.Replace(",", " ")},{item.TrangThaiXuLy},{item.NgayBC}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var result = new byte[bom.Length + bytes.Length];
            System.Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            System.Buffer.BlockCopy(bytes, 0, result, bom.Length, bytes.Length);

            var fileName = $"BaoCao_{LoaiBaoCao}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(result, "text/csv", fileName);
        }

        public async Task<IActionResult> Rank()
        {
            var topSinhVien = await _context.SinhViens
                .Include(s => s.MaLopNavigation)
                .OrderByDescending(s => s.DiemTichLuy)
                .Take(20)
                .ToListAsync();
            return View(topSinhVien);
        }

        [HttpPost]
        public async Task<IActionResult> TraoThuong(string[] selectedSv, int points)
        {
            if (selectedSv == null || selectedSv.Length == 0 || points <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var students = await _context.SinhViens
                    .Where(s => selectedSv.Contains(s.MaSv))
                    .ToListAsync();

                foreach (var student in students)
                {
                    student.DiemTichLuy = (student.DiemTichLuy ?? 0) + points;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Đã cộng {points} điểm cho {students.Count} sinh viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public IActionResult Backup()
        {
            // Trả về dữ liệu mẫu cho lịch sử sao lưu
            var backups = new List<dynamic>
            {
                new { ThoiDiem = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy HH:mm"), KichThuoc = "245 MB", Loai = "Tự động" },
                new { ThoiDiem = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy HH:mm"), KichThuoc = "243 MB", Loai = "Tự động" },
                new { ThoiDiem = DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy HH:mm"), KichThuoc = "241 MB", Loai = "Thủ công" }
            };
            ViewBag.Backups = backups;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId)) return Json(new { success = false });

            var notifications = await _context.ThongBaos
                .Where(t => t.MaNguoiNhan == adminId)
                .OrderByDescending(t => t.NgayTao)
                .Take(10)
                .Select(t => new {
                    t.MaTb,
                    t.TieuDe,
                    t.NoiDung,
                    t.TrangThai,
                    NgayTao = t.NgayTao.HasValue ? t.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : ""
                })
                .ToListAsync();

            return Json(new { success = true, data = notifications });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new HeThong_Admin.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
