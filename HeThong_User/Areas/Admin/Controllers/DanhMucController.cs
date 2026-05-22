using HeThong_User.Models;
using HeThong_User.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeThong_User.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;

        public DanhMucController(HeThongChiaSeTaiLieu_V1 context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var khoas = await _context.Khoas.Select(k => new { k.MaKhoa, k.TenKhoa }).ToListAsync();
            var nganhs = await _context.Nganhs.Select(n => new { n.MaNganh, n.TenNganh, n.MaKhoa }).ToListAsync();
            var monHocs = await _context.MonHocs.Select(m => new { m.MaMonHoc, m.TenMonHoc, m.MaNganh }).ToListAsync();
            var taiLieus = await _context.TaiLieus.Select(t => new { t.MaTaiLieu, t.TieuDe, t.MaMonHoc, t.MaNguoiDang, t.LuotTai, t.TrangThaiDuyet }).ToListAsync();
            var sinhViens = await _context.SinhViens.Select(s => new { maSV = s.MaSv, hoTen = s.TenSv }).ToListAsync();
            var giangViens = await _context.GiangViens.Select(g => new { maGV = g.MaGv, hoTen = g.TenGv }).ToListAsync();

            var seedData = new
            {
                khoa = khoas,
                nganh = nganhs,
                monHoc = monHocs,
                taiLieu = taiLieus.Select(t => new { 
                    maTL = t.MaTaiLieu, 
                    tieuDe = t.TieuDe, 
                    maMonHoc = t.MaMonHoc, 
                    maNguoiDang = t.MaNguoiDang, 
                    luotTai = t.LuotTai, 
                    trangThaiDuyet = t.TrangThaiDuyet 
                }),
                sinhVien = sinhViens,
                giangVien = giangViens
            };

            var viewModel = new CategorySeedViewModel
            {
                SeedJson = JsonSerializer.Serialize(seedData, new JsonSerializerOptions { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                })
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ThemNganhAjax([FromBody] JsonElement? data)
        {
            if (data == null) return BadRequest();
            var jsonElement = data.Value;
            string tenNganh = jsonElement.TryGetProperty("tenNganh", out var t) ? t.GetString() ?? "" : "";
            string maKhoa = jsonElement.TryGetProperty("maKhoa", out var k) ? k.GetString() ?? "" : "";

            if (!string.IsNullOrEmpty(tenNganh) && !string.IsNullOrEmpty(maKhoa))
            {
                var count = await _context.Nganhs.CountAsync() + 1;
                var maNganh = "NG" + count.ToString("D3");
                _context.Nganhs.Add(new Nganh { MaNganh = maNganh, TenNganh = tenNganh, MaKhoa = maKhoa });
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] JsonElement? data)
        {
            if (data == null) return BadRequest();
            var jsonElement = data.Value;
            string maMonHoc = jsonElement.TryGetProperty("maMonHoc", out var m) ? m.GetString() ?? "" : "";
            string tenMonHoc = jsonElement.TryGetProperty("tenMonHoc", out var t) ? t.GetString() ?? "" : "";
            string maNganh = jsonElement.TryGetProperty("maNganh", out var n) ? n.GetString() ?? "" : "";

            if (string.IsNullOrEmpty(maMonHoc)) {
                var count = await _context.MonHocs.CountAsync() + 1;
                maMonHoc = "MH" + count.ToString("D3");
            }

            var monHoc = new MonHoc { MaMonHoc = maMonHoc, TenMonHoc = tenMonHoc, MaNganh = maNganh };
            _context.MonHocs.Add(monHoc);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse([FromBody] JsonElement? data)
        {
            if (data == null) return BadRequest();
            var jsonElement = data.Value;
            string maMonHoc = jsonElement.TryGetProperty("maMonHoc", out var m) ? m.GetString() ?? "" : "";
            string tenMonHoc = jsonElement.TryGetProperty("tenMonHoc", out var t) ? t.GetString() ?? "" : "";
            string maNganh = jsonElement.TryGetProperty("maNganh", out var n) ? n.GetString() ?? "" : "";

            var monHoc = await _context.MonHocs.FindAsync(maMonHoc);
            if (monHoc == null) return NotFound();

            monHoc.TenMonHoc = tenMonHoc;
            monHoc.MaNganh = maNganh;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse([FromBody] JsonElement? data)
        {
            if (data == null) return BadRequest();
            var jsonElement = data.Value;
            string maMonHoc = jsonElement.TryGetProperty("maMonHoc", out var m) ? m.GetString() ?? "" : "";

            var monHoc = await _context.MonHocs.FindAsync(maMonHoc);
            if (monHoc == null) return NotFound();

            // Kiểm tra ràng buộc tài liệu
            var hasDocs = await _context.TaiLieus.AnyAsync(t => t.MaMonHoc == maMonHoc);
            if (hasDocs) return BadRequest("Không thể xóa môn học đang có tài liệu.");

            _context.MonHocs.Remove(monHoc);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
