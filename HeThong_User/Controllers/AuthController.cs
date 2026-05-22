using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_User.Controllers
{
    public class AuthController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(HeThongChiaSeTaiLieu_V1 context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // [GET] Trang đăng nhập: Hiển thị giao diện cho người dùng nhập tài khoản/mật khẩu.
        public IActionResult Login()
        {
            // Nếu đã đăng nhập, chuyển về trang tương ứng
            var maVaiTro = HttpContext.Session.GetString("MaVaiTro");
            if (!string.IsNullOrEmpty(maVaiTro))
            {
                if (maVaiTro.Trim() == "VT001" || maVaiTro.Trim() == "VT004" || maVaiTro.Trim() == "VT002")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string? username, string? password)
        {
            _logger.LogInformation($"=== BẮT ĐẦU ĐĂNG NHẬP ===");
            _logger.LogInformation($"Username nhận được: '{username}'");
            _logger.LogInformation($"Password nhận được: '{password}'");
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Username hoặc password bị null/empty");
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                return View();
            }

            try
            {
                // Trim khoảng trắng thừa
                var trimmedUsername = username?.Trim();
                
                // Tìm tài khoản
                _logger.LogInformation($"Đang tìm tài khoản với username: '{trimmedUsername}'");
                
                var taiKhoan = await _context.TaiKhoans
                    .Include(tk => tk.MaVaiTroNavigation)
                    .Include(tk => tk.MaSvNavigation)
                    .Include(tk => tk.MaGvNavigation)
                    .FirstOrDefaultAsync(tk => trimmedUsername != null && tk.MaTk.Trim() == trimmedUsername);

                if (taiKhoan == null)
                {
                    _logger.LogWarning($"KHÔNG TÌM THẤY tài khoản với username: '{username}'");
                    var errorMsg = "Tên đăng nhập không tồn tại!";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMsg });
                    }
                    
                    TempData["ErrorMessage"] = errorMsg;
                    return View();
                }

                _logger.LogInformation($"TÌM THẤY tài khoản: MaTK={taiKhoan.MaTk}, TenTK={taiKhoan.TenTk}");
                _logger.LogInformation($"Password trong DB: '{taiKhoan.MatKhau}'");
                _logger.LogInformation($"Password nhập vào: '{password}'");
                
                // Trim password trong DB trước khi so sánh
                var dbPassword = taiKhoan.MatKhau?.Trim();
                _logger.LogInformation($"So sánh: DB='{dbPassword}' vs Input='{password}' => {dbPassword == password}");

                // Kiểm tra mật khẩu (TODO: Implement password hashing)
                if (dbPassword != password)
                {
                    _logger.LogWarning($"MẬT KHẨU SAI! DB='{dbPassword}' vs Input='{password}'");
                    var errorMsg = "Mật khẩu không đúng!";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMsg });
                    }
                    
                    TempData["ErrorMessage"] = errorMsg;
                    return View();
                }

                _logger.LogInformation("Mật khẩu ĐÚNG!");

                // Kiểm tra trạng thái tài khoản
                _logger.LogInformation($"Trạng thái tài khoản: '{taiKhoan.TrangThai}'");
                
                // Kiểm tra nếu tài khoản bị khóa (0 = Khóa, 1 = Hoạt động)
                if (taiKhoan.TrangThai == 0)
                {
                    _logger.LogWarning($"Tài khoản bị khóa: {taiKhoan.MaTk}");
                    var lockedMsg = "Tài khoản bị tạm khóa. Vui lòng liên hệ Admin để biết thêm thông tin!";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = lockedMsg });
                    }
                    
                    TempData["ErrorMessage"] = lockedMsg;
                    return View();
                }

                // BƯỚC 4: Thiết lập các thông tin cơ bản vào Session để sử dụng toàn hệ thống
                _logger.LogInformation("Đang lưu thông tin vào Session...");
                HttpContext.Session.SetString("MaTaiKhoan", taiKhoan.MaTk ?? "");
                HttpContext.Session.SetString("TenTaiKhoan", taiKhoan.TenTk ?? "");
                HttpContext.Session.SetString("MaVaiTro", taiKhoan.MaVaiTro ?? "");
                HttpContext.Session.SetString("TenVaiTro", taiKhoan.MaVaiTroNavigation?.TenVaiTro ?? "");

                // Tạo object để trả về cho client (lưu vào localStorage)
                dynamic userInfo = new
                {
                    MaTaiKhoan = taiKhoan.MaTk ?? "",
                    TenTaiKhoan = taiKhoan.TenTk ?? "",
                    MaVaiTro = taiKhoan.MaVaiTro ?? "",
                    TenVaiTro = taiKhoan.MaVaiTroNavigation?.TenVaiTro ?? "Người dùng",
                    LoaiNguoiDung = "",
                    MaNguoiDung = "",
                    TenNguoiDung = taiKhoan.TenTk ?? "Người dùng",
                    Email = "",
                    DiemTichLuy = 0
                };

                // Kiểm tra vai trò và lưu thông tin tương ứng
                if (taiKhoan.MaSvNavigation != null)
                {
                    var sinhVien = taiKhoan.MaSvNavigation;
                    _logger.LogInformation($"Đăng nhập với vai trò SINH VIÊN: {sinhVien.MaSv} - {sinhVien.TenSv}");
                    HttpContext.Session.SetString("MaSinhVien", sinhVien.MaSv ?? "");
                    HttpContext.Session.SetString("TenNguoiDung", sinhVien.TenSv ?? "");
                    HttpContext.Session.SetString("LoaiNguoiDung", "SinhVien");
                    HttpContext.Session.SetString("DiemTichLuy", (sinhVien.DiemTichLuy ?? 0).ToString());
                    
                    userInfo = new
                    {
                        MaTaiKhoan = taiKhoan.MaTk ?? "",
                        TenTaiKhoan = taiKhoan.TenTk ?? "",
                        MaVaiTro = taiKhoan.MaVaiTro ?? "",
                        TenVaiTro = taiKhoan.MaVaiTroNavigation?.TenVaiTro ?? "Sinh viên",
                        LoaiNguoiDung = "SinhVien",
                        MaNguoiDung = sinhVien.MaSv ?? "",
                        TenNguoiDung = sinhVien.TenSv ?? "Sinh viên",
                        Email = sinhVien.Email ?? "",
                        NgaySinh = sinhVien.NgaySinh?.ToString("dd/MM/yyyy") ?? "",
                        GioiTinh = sinhVien.GioiTinh ?? "",
                        DiemTichLuy = sinhVien.DiemTichLuy ?? 0,
                        MaLop = sinhVien.MaLop ?? "",
                        TrangThaiSv = sinhVien.TrangThaiSv ?? ""
                    };
                }
                else if (taiKhoan.MaGvNavigation != null || taiKhoan.MaVaiTro?.Trim() == "VT001" || taiKhoan.MaVaiTro?.Trim() == "VT004")
                {
                    var giangVien = taiKhoan.MaGvNavigation;
                    _logger.LogInformation($"Đăng nhập với vai trò QUẢN TRỊ/GIẢNG VIÊN: {taiKhoan.MaTk} - {taiKhoan.TenTk}");
                    
                    // Set common session variables
                    HttpContext.Session.SetString("TenNguoiDung", taiKhoan.TenTk ?? "");
                    HttpContext.Session.SetString("LoaiNguoiDung", "GiangVien");
                    HttpContext.Session.SetString("DiemTichLuy", "0");

                    // Set Admin-specific session variables for Admin Area
                    if (taiKhoan.MaVaiTro?.Trim() == "VT001" || taiKhoan.MaVaiTro?.Trim() == "VT004" || taiKhoan.MaVaiTro?.Trim() == "VT002")
                    {
                        HttpContext.Session.SetString("AdminId", taiKhoan.MaTk ?? "");
                        HttpContext.Session.SetString("AdminName", taiKhoan.TenTk ?? "Admin");
                        HttpContext.Session.SetString("AdminRole", taiKhoan.MaVaiTro?.Trim() ?? "");
                        HttpContext.Session.SetString("RoleName", taiKhoan.MaVaiTroNavigation?.TenVaiTro ?? "Quản trị");

                        if (giangVien != null)
                        {
                            var gvDetailed = await _context.GiangViens
                                .Include(g => g.MaKhoaNavigation)
                                .FirstOrDefaultAsync(g => g.MaGv == taiKhoan.MaTk);
                            
                            if (gvDetailed != null)
                            {
                                HttpContext.Session.SetString("AdminEmail", gvDetailed.Email ?? "");
                                HttpContext.Session.SetString("AdminFaculty", gvDetailed.MaKhoaNavigation?.TenKhoa ?? "");
                            }
                        }
                    }

                    if (giangVien != null)
                    {
                        HttpContext.Session.SetString("MaGiangVien", giangVien.MaGv ?? "");
                        userInfo = new
                        {
                            MaTaiKhoan = taiKhoan.MaTk ?? "",
                            TenTaiKhoan = taiKhoan.TenTk ?? "",
                            MaVaiTro = taiKhoan.MaVaiTro ?? "",
                            TenVaiTro = taiKhoan.MaVaiTroNavigation?.TenVaiTro ?? "Giảng viên",
                            LoaiNguoiDung = "GiangVien",
                            MaNguoiDung = giangVien.MaGv ?? "",
                            TenNguoiDung = giangVien.TenGv ?? "Giảng viên",
                            Email = giangVien.Email ?? "",
                            Sdt = giangVien.Sdt ?? "",
                            DiemTichLuy = 0,
                            MaKhoa = giangVien.MaKhoa ?? ""
                        };
                    }
                    else
                    {
                        userInfo = new
                        {
                            MaTaiKhoan = taiKhoan.MaTk ?? "",
                            TenTaiKhoan = taiKhoan.TenTk ?? "",
                            MaVaiTro = taiKhoan.MaVaiTro ?? "",
                            TenVaiTro = taiKhoan.MaVaiTroNavigation?.TenVaiTro ?? "Quản trị",
                            LoaiNguoiDung = "Admin",
                            MaNguoiDung = taiKhoan.MaTk ?? "",
                            TenNguoiDung = taiKhoan.TenTk ?? "Quản trị",
                            DiemTichLuy = 0
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("Tài khoản không có SinhVien hoặc GiangVien liên kết!");
                }

                _logger.LogInformation("=== ĐĂNG NHẬP THÀNH CÔNG ===");
                
                string redirectUrl = Url.Action("Index", "Home") ?? "/";
                
                // Nếu là Admin hoặc Cán bộ khoa hoặc Giảng viên, ưu tiên chuyển về trang quản trị
                if (taiKhoan.MaVaiTro?.Trim() == "VT001" || taiKhoan.MaVaiTro?.Trim() == "VT004" || taiKhoan.MaVaiTro?.Trim() == "VT002")
                {
                    redirectUrl = Url.Action("Index", "Home", new { area = "Admin" }) ?? "/Admin/Home/Index";
                }
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đăng nhập thành công!", userInfo, redirectUrl });
                }
                
                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LỖI KHI ĐĂNG NHẬP");
                var errorMessage = $"Lỗi hệ thống: {ex.Message}";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                
                TempData["ErrorMessage"] = errorMessage;
                return View();
            }
        }

        // [GET] Đăng xuất: Xóa toàn bộ Session và chuyển hướng người dùng về trang đăng nhập.
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            TempData["ClearLocalStorage"] = "true"; // Signal để xóa localStorage
            return RedirectToAction("Login");
        }

        // GET: Auth/TestAccount?username=sv_mai
        public async Task<IActionResult> TestAccount(string? username)
        {
            _logger.LogInformation($"=== TEST TÀI KHOẢN: {username} ===");
            
            // Lấy tất cả tài khoản
            var allAccounts = await _context.TaiKhoans
                .Include(tk => tk.MaVaiTroNavigation)
                .Include(tk => tk.MaSvNavigation)
                .Include(tk => tk.MaGvNavigation)
                .ToListAsync();
            
            _logger.LogInformation($"Tổng số tài khoản: {allAccounts.Count}");
            
            foreach (var acc in allAccounts)
            {
                _logger.LogInformation($"- MaTK: {acc.MaTk}, TenTK: '{acc.TenTk}', MatKhau: '{acc.MatKhau}', TrangThai: '{acc.TrangThai}'");
            }
            
            // Tìm tài khoản cụ thể
            var taiKhoan = allAccounts.FirstOrDefault(tk => tk.MaTk.Trim() == username?.Trim());
            
            if (taiKhoan == null)
            {
                _logger.LogWarning($"KHÔNG TÌM THẤY tài khoản: {username}");
                return Content($"KHÔNG TÌM THẤY tài khoản: {username}");
            }
            
            _logger.LogInformation($"TÌM THẤY tài khoản:");
            _logger.LogInformation($"- MaTK: {taiKhoan.MaTk}");
            _logger.LogInformation($"- TenTK: '{taiKhoan.TenTk}'");
            _logger.LogInformation($"- MatKhau: '{taiKhoan.MatKhau}'");
            _logger.LogInformation($"- TrangThai: '{taiKhoan.TrangThai}'");
            _logger.LogInformation($"- MaVaiTro: '{taiKhoan.MaVaiTro}'");
            _logger.LogInformation($"- SinhVien liên kết: {taiKhoan.MaSv ?? "None"}");
            _logger.LogInformation($"- GiangVien liên kết: {taiKhoan.MaGv ?? "None"}");
            
            if (taiKhoan.MaSvNavigation != null)
            {
                var sv = taiKhoan.MaSvNavigation;
                _logger.LogInformation($"- SinhVien: MaSV={sv.MaSv}, TenSV={sv.TenSv}");
            }
            
            return Content($"Tìm thấy tài khoản: {taiKhoan.TenTk} - Xem log để biết chi tiết");
        }

        // GET: Auth/TestLogin?username=sv_mai&password=123456
        public async Task<IActionResult> TestLogin(string? username, string? password)
        {
            _logger.LogInformation($"=== TEST ĐĂNG NHẬP: {username} / {password} ===");
            
            try
            {
                // Trim khoảng trắng thừa
                username = username?.Trim();
                password = password?.Trim();
                
                // Tìm tài khoản
                var taiKhoan = await _context.TaiKhoans
                    .Include(tk => tk.MaVaiTroNavigation)
                    .Include(tk => tk.MaSvNavigation)
                    .Include(tk => tk.MaGvNavigation)
                    .FirstOrDefaultAsync(tk => tk.MaTk.Trim() == username);

                if (taiKhoan == null)
                {
                    _logger.LogWarning($"KHÔNG TÌM THẤY tài khoản");
                    return Content("KHÔNG TÌM THẤY tài khoản");
                }

                _logger.LogInformation($"TÌM THẤY tài khoản: {taiKhoan.TenTk}");
                
                var dbPassword = taiKhoan.MatKhau?.Trim();
                _logger.LogInformation($"So sánh password: DB='{dbPassword}' vs Input='{password}'");

                if (dbPassword != password)
                {
                    _logger.LogWarning($"MẬT KHẨU SAI!");
                    return Content("MẬT KHẨU SAI!");
                }

                _logger.LogInformation("Mật khẩu ĐÚNG!");
                _logger.LogInformation($"Trạng thái: '{taiKhoan.TrangThai}'");
                
                // Kiểm tra trạng thái
                if (taiKhoan.TrangThai == 0)
                {
                    _logger.LogWarning($"Tài khoản bị khóa");
                    return Content("Tài khoản bị khóa");
                }

                _logger.LogInformation("Trạng thái OK!");
                
                if (taiKhoan.MaSvNavigation != null)
                {
                    var sv = taiKhoan.MaSvNavigation;
                    _logger.LogInformation($"SinhVien: {sv.MaSv} - {sv.TenSv}");
                    return Content($"ĐĂNG NHẬP THÀNH CÔNG! Sinh viên: {sv.TenSv}");
                }
                
                return Content("ĐĂNG NHẬP THÀNH CÔNG!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LỖI");
                return Content($"LỖI: {ex.Message}");
            }
        }
    }
}
