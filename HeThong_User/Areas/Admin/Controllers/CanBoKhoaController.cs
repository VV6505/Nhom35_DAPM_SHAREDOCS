using System.Text.Json;
using HeThong_User.Models;
using HeThong_User.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_User.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CanBoKhoaController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;
        private readonly HeThong_User.Services.AzureBlobService _azureBlobService;

        private const int TimeDelay = 2; // Hằng số thời gian delay (giờ)

        public CanBoKhoaController(HeThongChiaSeTaiLieu_V1 context, HeThong_User.Services.AzureBlobService azureBlobService)
        {
            _context = context;
            _azureBlobService = azureBlobService;
        }

        private async Task<string?> GetMaKhoaCBK()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId)) return null;

            var tk = await _context.TaiKhoans
                .Include(x => x.MaGvNavigation)
                .FirstOrDefaultAsync(x => x.MaTk == adminId);

            return tk?.MaGvNavigation?.MaKhoa;
        }

        public async Task<IActionResult> ReviewCBK()
        {
            var maKhoa = await GetMaKhoaCBK();
            if (string.IsNullOrEmpty(maKhoa)) return RedirectToAction("Login", "Auth", new { area = "" });

            var cutOffTime = DateTime.Now.AddHours(-TimeDelay);
            var data = await _context.TaiLieus
                .Include(t => t.MaMonHocNavigation)
                    .ThenInclude(m => m!.MaNganhNavigation)
                .Where(t => t.MaMonHocNavigation != null 
                    && t.MaMonHocNavigation.MaNganhNavigation != null
                    && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoa
                    && t.TrangThaiDuyet == "Chờ duyệt"
                    && (t.NgayDang == null || t.NgayDang < cutOffTime))
                .OrderByDescending(t => t.NgayDang)
                .Select(t => new
                {
                    maTaiLieu = t.MaTaiLieu,
                    tieuDe = t.TieuDe,
                    loaiFile = t.LoaiFile,
                    trangThaiDuyet = t.TrangThaiDuyet,
                    ngayDang = t.NgayDang,
                    duongDanFile = t.DuongDanFile
                })
                .ToListAsync();

            var result = data.Select(t => new WorkflowDocDto
            {
                maTaiLieu = t.maTaiLieu,
                tieuDe = t.tieuDe,
                loaiFile = t.loaiFile,
                trangThaiDuyet = t.trangThaiDuyet,
                ngayDang = t.ngayDang,
                urlXemTruoc = _azureBlobService.GenerateSasLink(t.duongDanFile, 60)
            }).ToList();

            return View(result);
        }

        public async Task<IActionResult> ReviewedCBK()
        {
            var maKhoa = await GetMaKhoaCBK();
            if (string.IsNullOrEmpty(maKhoa)) return RedirectToAction("Login", "Auth", new { area = "" });

            var data = await _context.TaiLieus
                .Include(t => t.MaMonHocNavigation)
                    .ThenInclude(m => m!.MaNganhNavigation)
                .Where(t => t.MaMonHocNavigation != null 
                    && t.MaMonHocNavigation.MaNganhNavigation != null
                    && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoa
                    && t.TrangThaiDuyet != "Chờ duyệt")
                .OrderByDescending(t => t.NgayDang)
                .Select(t => new
                {
                    maTaiLieu = t.MaTaiLieu,
                    tieuDe = t.TieuDe,
                    loaiFile = t.LoaiFile,
                    trangThaiDuyet = t.TrangThaiDuyet,
                    lyDoTuChoi = t.LyDoTuChoi,
                    ngayDang = t.NgayDang,
                    duongDanFile = t.DuongDanFile
                })
                .ToListAsync();

            var result = data.Select(t => new WorkflowDocDto
            {
                maTaiLieu = t.maTaiLieu,
                tieuDe = t.tieuDe,
                loaiFile = t.loaiFile,
                trangThaiDuyet = t.trangThaiDuyet,
                lyDoTuChoi = t.lyDoTuChoi,
                ngayDang = t.ngayDang,
                urlXemTruoc = _azureBlobService.GenerateSasLink(t.duongDanFile, 60)
            }).ToList();

            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> CBKApprove([FromBody] JsonElement data)
        {
            if (data.TryGetProperty("maTaiLieu", out var prop))
            {
                string id = prop.GetString() ?? "";
                var tl = await _context.TaiLieus.FindAsync(id);
                if (tl != null)
                {
                    tl.TrangThaiDuyet = "Chờ Admin duyệt";

                    // GỬI THÔNG BÁO CHO NGƯỜI ĐĂNG
                    var lastTB_AP = _context.ThongBaos.OrderByDescending(t => t.MaTb).FirstOrDefault();
                    var nextTB_AP = lastTB_AP != null 
                        ? "TB" + (int.Parse(lastTB_AP.MaTb.Substring(2)) + 1).ToString("D3") 
                        : "TB001";

                    var thongBao = new ThongBao
                    {
                        MaTb = nextTB_AP,
                        TieuDe = "Kết quả phê duyệt",
                        NoiDung = $"Tài liệu '{tl.TieuDe}' đã được Khoa duyệt, chờ Admin xác nhận.",
                        NgayTao = DateTime.Now,
                        TrangThai = "Chưa đọc",
                        MaNguoiNhan = tl.MaNguoiDang
                    };
                    _context.ThongBaos.Add(thongBao);

                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] JsonElement data)
        {
            if (data.TryGetProperty("maTaiLieu", out var idProp) && data.TryGetProperty("lyDoTuChoi", out var reasonProp))
            {
                string id = idProp.GetString() ?? "";
                string reason = reasonProp.GetString() ?? "";
                var tl = await _context.TaiLieus.FindAsync(id);
                if (tl != null)
                {
                    tl.TrangThaiDuyet = "Từ chối";
                    tl.LyDoTuChoi = reason;

                    // GỬI THÔNG BÁO CHO NGƯỜI ĐĂNG
                    var lastTB_AP = _context.ThongBaos.OrderByDescending(t => t.MaTb).FirstOrDefault();
                    var nextTB_AP = lastTB_AP != null 
                        ? "TB" + (int.Parse(lastTB_AP.MaTb.Substring(2)) + 1).ToString("D3") 
                        : "TB001";

                    var thongBao = new ThongBao
                    {
                        MaTb = nextTB_AP,
                        TieuDe = "Kết quả phê duyệt",
                        NoiDung = $"Rất tiếc, tài liệu '{tl.TieuDe}' bị từ chối. Lý do: {reason}",
                        NgayTao = DateTime.Now,
                        TrangThai = "Chưa đọc",
                        MaNguoiNhan = tl.MaNguoiDang
                    };
                    _context.ThongBaos.Add(thongBao);

                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
            return NotFound();
        }
    }
}
