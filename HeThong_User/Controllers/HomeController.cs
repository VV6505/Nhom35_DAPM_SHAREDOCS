using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThong_User.Models;

namespace HeThong_User.Controllers
{
    public class HomeController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, HeThongChiaSeTaiLieu_V1 context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false });

            var notifications = await _context.ThongBaos
                .Where(t => t.MaNguoiNhan == userId)
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

        public IActionResult Index()
        {
            // Nếu là Admin hoặc Cán bộ khoa hoặc Giảng viên, tự động chuyển về trang quản trị
            var maVaiTro = HttpContext.Session.GetString("MaVaiTro");
            if (maVaiTro?.Trim() == "VT001" || maVaiTro?.Trim() == "VT004" || maVaiTro?.Trim() == "VT002")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // 1. Giữ nguyên các con số thống kê cho Header/Sidebar
            ViewBag.TotalDocs = _context.TaiLieus.Count(t => t.TrangThaiDuyet == "Đã duyệt" && t.CheDoHienThi != false);
            ViewBag.TotalUsers = _context.SinhViens.Count();
            ViewBag.TotalDownloads = _context.TaiLieus.Sum(t => t.LuotTai) ?? 0;
            ViewBag.TotalReports = _context.BaoCaoViPhams.Count(); // Thêm đếm báo cáo cho ô màu đỏ
 
            // 2. Lấy danh sách tài liệu cho Bảng tin
            // Lấy 10 tài liệu mới nhất kèm thông tin người đăng
            var newsfeedDocs = _context.TaiLieus
                .Include(t => t.MaMonHocNavigation)
                .Include(t => t.MaLoaiTlNavigation)
                .Include(t => t.MaNguoiDangNavigation)
                    .ThenInclude(tk => tk.MaSvNavigation)
                .Include(t => t.BinhLuans)
                    .ThenInclude(bl => bl.MaNdNavigation)
                        .ThenInclude(tk => tk.MaSvNavigation)
                .Include(t => t.DanhGia)
                .Where(t => t.TrangThaiDuyet == "Đã duyệt" && t.CheDoHienThi != false) // Chuẩn hóa theo DB: Đã duyệt và không bị ẩn
                .OrderByDescending(t => t.NgayDang)
                .Take(10)
                .ToList();  

            return View(newsfeedDocs);
        }

        public IActionResult Students()
        {
            var dsSinhVien = _context.SinhViens.Include(s => s.MaLopNavigation).ToList();
            return View(dsSinhVien);
        }

        public IActionResult History()
        {
            var lichSu = _context.LichSuTaiXuongs
                .Include(l => l.MaTaiLieuNavigation)
                .OrderByDescending(l => l.NgayTai)
                .ToList();
            return View(lichSu);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public async Task<IActionResult> GetLatestPoints()
        {
            int points = 0;
            var maSV = HttpContext.Session.GetString("MaSinhVien");

            if (!string.IsNullOrEmpty(maSV))
            {
                var sinhVien = await _context.SinhViens.FindAsync(maSV);
                if (sinhVien != null)
                {
                    points = sinhVien.DiemTichLuy ?? 0;
                }
            }
            return Json(new { success = true, points = points });
        }
    }
}
