using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_User.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class BaoCaoController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;

        public BaoCaoController(HeThongChiaSeTaiLieu_V1 context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? trangthai, string? search)
        {
            var query = _context.BaoCaoViPhams
                .Include(b => b.MaTaiLieuNavigation)
                .AsQueryable();

            // Tìm kiếm theo tên tài liệu
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.MaTaiLieuNavigation != null && b.MaTaiLieuNavigation.TieuDe.Contains(search));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangthai) && trangthai != "all")
            {
                if (trangthai == "Chờ xử lý")
                {
                    query = query.Where(b => b.TrangThaiXuLy == null || b.TrangThaiXuLy == "Chờ xử lý" || b.TrangThaiXuLy == "ChờXửLý");
                }
                else
                {
                    query = query.Where(b => b.TrangThaiXuLy == trangthai);
                }
            }

            var baoCaos = await query.OrderByDescending(b => b.NgayBaoCao).ToListAsync();

            // Lấy tên người báo cáo
            var nguoiBCMap = new Dictionary<string, string>();
            foreach (var bc in baoCaos)
            {
                if (!string.IsNullOrEmpty(bc.NguoiBaoCao) && !nguoiBCMap.ContainsKey(bc.NguoiBaoCao))
                {
                    var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.MaSv == bc.NguoiBaoCao);
                    var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.MaGv == bc.NguoiBaoCao);
                    nguoiBCMap[bc.NguoiBaoCao] = sv?.TenSv ?? gv?.TenGv ?? bc.NguoiBaoCao;
                }
            }

            ViewBag.NguoiBCMap = nguoiBCMap;
            ViewBag.TrangThai = trangthai;
            ViewBag.Search = search;

            return View(baoCaos);
        }

        [HttpPost("XuLyViPham")]
        public async Task<IActionResult> XuLyViPham(string id)
        {
            var bc = await _context.BaoCaoViPhams.FindAsync(id);
            if (bc != null)
            {
                bc.TrangThaiXuLy = "Đã xử lý";
                bc.NgayDuyet = DateTime.Now;

                // Cập nhật trạng thái và ẩn tài liệu gốc
                var taiLieu = await _context.TaiLieus.FindAsync(bc.MaTaiLieu);
                if (taiLieu != null)
                {
                    taiLieu.CheDoHienThi = false; // Ẩn tài liệu
                    taiLieu.TrangThaiDuyet = "Từ chối"; // Từ chối duyệt bài
                    _context.Update(taiLieu);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost("BoQua")]
        public async Task<IActionResult> BoQua(string id)
        {
            var bc = await _context.BaoCaoViPhams.FindAsync(id);
            if (bc != null)
            {
                bc.TrangThaiXuLy = "Bỏ qua";
                bc.NgayDuyet = DateTime.Now;

                // Khôi phục hiển thị cho tài liệu gốc (Trường hợp bác bỏ báo cáo sai)
                var taiLieu = await _context.TaiLieus.FindAsync(bc.MaTaiLieu);
                if (taiLieu != null)
                {
                    // Nếu KHÔNG PHẢI là tác giả tự yêu cầu gỡ khẩn cấp, khôi phục hiển thị (bác bỏ báo cáo sai của người khác)
                    bool isSelfRequest = taiLieu.MaNguoiDang == bc.NguoiBaoCao || bc.LyDo == "Tác giả yêu cầu gỡ bài khẩn cấp";
                    if (!isSelfRequest)
                    {
                        taiLieu.CheDoHienThi = true; // Hiển thị lại tài liệu
                        taiLieu.TrangThaiDuyet = "Đã duyệt"; // Đặt lại trạng thái duyệt
                        _context.Update(taiLieu);
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
