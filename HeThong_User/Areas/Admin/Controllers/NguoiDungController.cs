using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_User.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NguoiDungController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;

        public NguoiDungController(HeThongChiaSeTaiLieu_V1 context)
        {
            _context = context;
        }

        // [GET] Danh sách người dùng: Lấy toàn bộ tài khoản (Sinh viên, Giảng viên, CBK) kèm thông tin phân quyền và trạng thái.
        public async Task<IActionResult> Index(string? search, string? vaitro, string? trangthai)
        {
            var query = _context.TaiKhoans
                .Include(t => t.MaVaiTroNavigation)
                .Include(t => t.MaSvNavigation)
                .Include(t => t.MaGvNavigation)
                .AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.TenTk.Contains(search) ||
                    (t.MaSvNavigation != null && t.MaSvNavigation.TenSv.Contains(search)) ||
                    (t.MaGvNavigation != null && t.MaGvNavigation.TenGv.Contains(search)));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrEmpty(vaitro) && vaitro != "all")
            {
                query = query.Where(t => t.MaVaiTro == vaitro);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangthai) && trangthai != "all")
            {
                if (int.TryParse(trangthai, out int statusValue))
                {
                    query = query.Where(t => t.TrangThai == statusValue);
                }
            }

            var users = await query.OrderBy(t => t.MaTk).ToListAsync();
            var vaiTros = await _context.VaiTros.ToListAsync();
            
            ViewBag.Lops = await _context.Lops.ToListAsync();
            ViewBag.Khoas = await _context.Khoas.ToListAsync();

            ViewBag.VaiTros = vaiTros;
            ViewBag.Search = search;
            ViewBag.VaiTro = vaitro;
            ViewBag.TrangThai = trangthai;

            return View(users);
        }

        // Thêm sinh viên mới (Database sẽ tự động tạo tài khoản dựa trên mã sinh viên này)
        [HttpPost]
        public async Task<IActionResult> CreateStudent(SinhVien sv)
        {
            if (ModelState.IsValid)
            {
                try {
                    _context.SinhViens.Add(sv);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Thêm sinh viên thành công!" });
                } catch (Exception ex) {
                    return Json(new { success = false, message = "Lỗi: " + ex.InnerException?.Message ?? ex.Message });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        // [POST] Thêm giảng viên: Tiếp nhận thông tin giảng viên/CBK mới và lưu vào DB.
        [HttpPost]
        public async Task<IActionResult> CreateLecturer(GiangVien gv)
        {
            if (ModelState.IsValid)
            {
                try {
                    _context.GiangViens.Add(gv);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Thêm giảng viên thành công!" });
                } catch (Exception ex) {
                    return Json(new { success = false, message = "Lỗi: " + ex.InnerException?.Message ?? ex.Message });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        // [POST] Khóa tài khoản: Cập nhật trạng thái tài khoản thành 0 (Tạm khóa) để ngăn người dùng đăng nhập.
        [HttpPost]
        public async Task<IActionResult> KhoaTaiKhoan(string id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                // Không cho phép khóa tài khoản Admin hoặc chính mình
                var adminId = HttpContext.Session.GetString("AdminId");
                if (tk.MaTk.Trim() == "ADMIN" || tk.MaTk == adminId || tk.MaVaiTro?.Trim() == "VT001")
                {
                    return RedirectToAction("Index");
                }

                // Cập nhật trạng thái = 0 (Tạm khóa). Người dùng sẽ không thể đăng nhập được nữa.
                tk.TrangThai = 0; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MoKhoaTaiKhoan(string id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                tk.TrangThai = 1; // Hoạt động
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> XoaTaiKhoan(string id)
        {
            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk != null)
            {
                // Không cho phép xóa tài khoản Admin hoặc chính mình
                var adminId = HttpContext.Session.GetString("AdminId")?.Trim();
                var targetId = tk.MaTk?.Trim();
                var targetRole = tk.MaVaiTro?.Trim();

                if (targetId == "ADMIN" || targetId == adminId || targetRole == "VT001")
                {
                    return RedirectToAction("Index");
                }

                _context.TaiKhoans.Remove(tk);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // [GET] Chi tiết người dùng: Lấy thông tin cá nhân chi tiết của một tài khoản để hiển thị trên Modal (AJAX).
        [HttpGet]
        public async Task<IActionResult> GetDetails(string id)
        {
            var tk = await _context.TaiKhoans
                .Include(t => t.MaVaiTroNavigation)
                .Include(t => t.MaSvNavigation).ThenInclude(s => s!.MaLopNavigation).ThenInclude(l => l!.MaKhoaNavigation)
                .Include(t => t.MaGvNavigation).ThenInclude(g => g!.MaKhoaNavigation)
                .FirstOrDefaultAsync(t => t.MaTk == id);

            if (tk == null) return NotFound();

            object? detail = null;
            if (tk.MaSvNavigation != null)
            {
                var sv = tk.MaSvNavigation;
                detail = new
                {
                    type = "Sinh viên",
                    name = sv.TenSv,
                    code = sv.MaSv,
                    email = sv.Email,
                    dob = sv.NgaySinh?.ToString("dd/MM/yyyy"),
                    gender = sv.GioiTinh,
                    points = sv.DiemTichLuy,
                    className = sv.MaLopNavigation?.TenLop,
                    facultyName = sv.MaLopNavigation?.MaKhoaNavigation?.TenKhoa,
                    status = sv.TrangThaiSv
                };
            }
            else if (tk.MaGvNavigation != null)
            {
                var gv = tk.MaGvNavigation;
                detail = new
                {
                    type = "Giảng viên",
                    name = gv.TenGv,
                    code = gv.MaGv,
                    email = gv.Email,
                    dob = gv.NgaySinh?.ToString("dd/MM/yyyy"),
                    gender = gv.GioiTinh,
                    phone = gv.Sdt,
                    degree = gv.HocVi,
                    facultyName = gv.MaKhoaNavigation?.TenKhoa,
                    roleInFaculty = gv.LoaiGv == "CBK" ? "Cán bộ khoa" : "Giảng viên"
                };
            }
            else
            {
                detail = new
                {
                    type = "Quản trị viên",
                    name = tk.TenTk,
                    code = tk.MaTk,
                    role = tk.MaVaiTroNavigation?.TenVaiTro
                };
            }

            return Json(detail);
        }
    }
}
