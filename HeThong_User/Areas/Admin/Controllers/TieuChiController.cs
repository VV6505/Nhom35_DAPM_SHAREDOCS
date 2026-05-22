using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HeThong_User.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TieuChiController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;

        public TieuChiController(HeThongChiaSeTaiLieu_V1 context)
        {
            _context = context;
        }

        // Thiết lập Loại tài liệu và Điểm tích lũy tương ứng
        public async Task<IActionResult> Index()
        {
            var loaiTaiLieus = await _context.LoaiTaiLieus
                .Include(l => l.MaDqNavigation)
                .OrderBy(l => l.MaLtl)
                .ToListAsync();

            var doQuies = await _context.DoQuies
                .OrderBy(d => d.MucDoQuy)
                .ToListAsync();

            ViewBag.DoQuies = doQuies;
            return View(loaiTaiLieus);
        }

        // Đổi tên tài liệu hoặc gắn mức độ quý (điểm) mới cho loại tài liệu đó
        [HttpPost]
        public async Task<IActionResult> UpdateLoaiTaiLieu([FromBody] JsonElement data)
        {
            try
            {
                string maLtl = data.GetProperty("maLtl").GetString() ?? "";
                string tenLtl = data.GetProperty("tenLtl").GetString() ?? "";
                string maDq = data.GetProperty("maDq").GetString() ?? "";

                var item = await _context.LoaiTaiLieus.FindAsync(maLtl);
                if (item == null) return NotFound();

                item.TenLtl = tenLtl;
                item.MaDq = maDq;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // [POST] Cập nhật mức độ quý: Thay đổi số điểm tích lũy cần thiết để tải tài liệu thuộc mức độ này (AJAX).
        [HttpPost]
        public async Task<IActionResult> UpdateDoQuy([FromBody] JsonElement data)
        {
            try
            {
                string maDq = data.GetProperty("maDq").GetString() ?? "";
                int mucDoQuy = data.GetProperty("mucDoQuy").GetInt32();
                int diemTl = data.GetProperty("diemTl").GetInt32();

                var item = await _context.DoQuies.FindAsync(maDq);
                if (item == null) return NotFound();

                item.MucDoQuy = mucDoQuy;
                item.DiemTl = diemTl;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoaiTaiLieu([FromBody] JsonElement data)
        {
            try
            {
                string tenLtl = data.GetProperty("tenLtl").GetString() ?? "";
                string maDq = data.GetProperty("maDq").GetString() ?? "";

                var count = await _context.LoaiTaiLieus.CountAsync() + 1;
                var maLtl = "L" + count.ToString("D4");

                var newItem = new LoaiTaiLieu
                {
                    MaLtl = maLtl,
                    TenLtl = tenLtl,
                    MaDq = maDq
                };

                _context.LoaiTaiLieus.Add(newItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
