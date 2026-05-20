using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_Admin.Controllers
{
    public class WorkflowDocDto
    {
        public string maTaiLieu { get; set; } = "";
        public string tieuDe { get; set; } = "";
        public string loaiFile { get; set; } = "";
        public string trangThaiDuyet { get; set; } = "";
        public string? lyDoTuChoi { get; set; }
        public DateTime? ngayDang { get; set; }
        public string urlXemTruoc { get; set; } = "";
    }

    public class CanBoKhoaController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;
        private readonly HeThong_Admin.Services.AzureBlobService _azureBlobService;

        public CanBoKhoaController(HeThongChiaSeTaiLieu_V1 context, HeThong_Admin.Services.AzureBlobService azureBlobService)
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
            if (string.IsNullOrEmpty(maKhoa)) return RedirectToAction("Login", "Auth");

            var data = await _context.TaiLieus
                .Include(t => t.MaMonHocNavigation)
                    .ThenInclude(m => m!.MaNganhNavigation)
                .Where(t => t.MaMonHocNavigation != null 
                    && t.MaMonHocNavigation.MaNganhNavigation != null
                    && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoa
                    && t.TrangThaiDuyet == "Chờ duyệt")
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
            if (string.IsNullOrEmpty(maKhoa)) return RedirectToAction("Login", "Auth");

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
        public async Task<IActionResult> CBKApprove([FromBody] dynamic data)
        {
            string id = data.GetProperty("maTaiLieu").GetString();
            var tl = await _context.TaiLieus.FindAsync(id);
            if (tl != null)
            {
                tl.TrangThaiDuyet = "Chờ Admin duyệt";
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] dynamic data)
        {
            string id = data.GetProperty("maTaiLieu").GetString();
            string reason = data.GetProperty("lyDoTuChoi").GetString();
            var tl = await _context.TaiLieus.FindAsync(id);
            if (tl != null)
            {
                tl.TrangThaiDuyet = "Từ chối";
                tl.LyDoTuChoi = reason;
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
