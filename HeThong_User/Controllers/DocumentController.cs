using HeThong_User.Models;
using HeThong_User.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace HeThong_User.Controllers
{
    public class DocumentController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentController> _logger;
        private readonly AzureBlobService _azureBlobService;
        private readonly NLPService _nlpService;

        public DocumentController(HeThongChiaSeTaiLieu_V1 context, IWebHostEnvironment env, ILogger<DocumentController> logger,
            AzureBlobService azureBlobService, NLPService nlpService)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _azureBlobService = azureBlobService;
            _nlpService = nlpService;
        }

        // =====================================================================
        // HÀM TIỆN ÍCH: Chuẩn hóa chuỗi thành dạng slug (không dấu, không khoảng trắng)
        // =====================================================================
        private static string Slugify(string text)
        {
            if (string.IsNullOrEmpty(text)) return "TaiLieu";
            var vietnameseChars = new Dictionary<string, string>
            {
                {"à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ", "a"},
                {"è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ", "e"},
                {"ì|í|ị|ỉ|ĩ", "i"},
                {"ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ", "o"},
                {"ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ", "u"},
                {"ỳ|ý|ỵ|ỷ|ỹ", "y"},
                {"đ", "d"},
                {"À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ", "A"},
                {"È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ", "E"},
                {"Ì|Í|Ị|Ỉ|Ĩ", "I"},
                {"Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ", "O"},
                {"Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ", "U"},
                {"Ỳ|Ý|Ỵ|Ỷ|Ỹ", "Y"},
                {"Đ", "D"}
            };
            foreach (var pair in vietnameseChars)
                foreach (var ch in pair.Key.Split('|'))
                    text = text.Replace(ch, pair.Value);
            var words = Regex.Replace(text, @"[^a-zA-Z0-9\s]", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
        }

        // =====================================================================
        // HÀM TIỆN ÍCH: Sinh tên file chuẩn hóa [MaMonHoc]_[MaLoaiTL]_[Slug]_[MaND]_[Nam].[ext]
        // =====================================================================
        private string GenerateStandardFileName(string maMonHoc, string maLoaiTl, string tieuDe, string maNguoiDang, string extension)
        {
            var mon = (maMonHoc ?? "MON").Trim();
            var loai = (maLoaiTl ?? "LTL").Trim();
            var slug = Slugify(tieuDe);
            var nd = (maNguoiDang ?? "ND").Trim();
            var nam = DateTime.Now.Year.ToString();
            var ext = extension.TrimStart('.').ToUpper();
            if (slug.Length > 30) slug = slug.Substring(0, 30);
            return $"{mon}_{loai}_{slug}_{nd}_{nam}.{ext}";
        }

        // =====================================================================
        // [GET] Trang danh mục: Lấy cây thư mục và danh sách tài liệu đã duyệt.
        // =====================================================================
        public IActionResult Index(string idMon, string searchString)
        {
            ViewBag.DanhMuc = _context.Khoas
                .Include(k => k.Nganhs)
                    .ThenInclude(n => n.MonHocs)
                        .ThenInclude(m => m.TaiLieus)
                .ToList();

            var query = from t in _context.TaiLieus
                        join tk in _context.TaiKhoans on t.MaNguoiDang equals tk.MaTk into tkGroup
                        from tk in tkGroup.DefaultIfEmpty()
                        join sv in _context.SinhViens on tk.MaSv equals sv.MaSv into svGroup
                        from sv in svGroup.DefaultIfEmpty()
                        where t.TrangThaiDuyet == "Đã duyệt" && t.CheDoHienThi != false
                        select new
                        {
                            Data = t,
                            TenNguoiDang = sv != null ? sv.TenSv : (tk != null ? tk.TenTk : t.MaNguoiDang),
                            TenMon = t.MaMonHocNavigation != null ? t.MaMonHocNavigation.TenMonHoc : ""
                        };

            if (!string.IsNullOrEmpty(idMon))
            {
                query = query.Where(x => x.Data.MaMonHoc == idMon);
                ViewBag.CurrentMon = idMon;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => (x.Data.TieuDe != null && x.Data.TieuDe.Contains(searchString))
                                      || (x.Data.MoTa != null && x.Data.MoTa.Contains(searchString)));
                ViewBag.SearchString = searchString;
            }

            // ĐOẠN ĐÃ ĐƯỢC CHỈNH SỬA: Sắp xếp tài liệu đăng mới nhất lên đầu bảng
            var results = query.OrderByDescending(x => x.Data.NgayDang).ToList();
            return View(results);
        }

        // [POST] Thích / Bỏ thích tài liệu (AJAX)
        [HttpPost]
        public IActionResult ToggleLike(string maTL)
        {
            string maND = HttpContext.Session.GetString("MaTaiKhoan") ?? "SV001";
            var existingLike = _context.DanhGia.FirstOrDefault(d => d.MaTl == maTL && d.MaNd == maND);

            if (existingLike != null)
            {
                _context.DanhGia.Remove(existingLike);
                _context.SaveChanges();
                return Json(new { success = true, liked = false, count = _context.DanhGia.Count(d => d.MaTl == maTL) });
            }
            else
            {
                var ticks = DateTime.Now.Ticks.ToString();
                var newLike = new DanhGia
                {
                    MaDg = "D" + (ticks.Length > 4 ? ticks.Substring(ticks.Length - 4) : ticks.PadLeft(4, '0')),
                    MaTl = maTL,
                    MaNd = maND,
                    SoSaoDg = 5,
                    ThoiGian = DateTime.Now
                };
                _context.DanhGia.Add(newLike);
                _context.SaveChanges();
                return Json(new { success = true, liked = true, count = _context.DanhGia.Count(d => d.MaTl == maTL) });
            }
        }

        // [POST] Lưu / Bỏ lưu tài liệu yêu thích (AJAX)
        [HttpPost]
        public IActionResult ToggleSave(string maTL)
        {
            var loaiNguoiDung = HttpContext.Session.GetString("LoaiNguoiDung");
            string maND = loaiNguoiDung == "SinhVien"
                ? HttpContext.Session.GetString("MaSinhVien")
                : HttpContext.Session.GetString("MaGiangVien");

            if (string.IsNullOrEmpty(maND)) return Json(new { success = false, message = "Chưa đăng nhập" });

            var existingSave = _context.TLYeuThiches.FirstOrDefault(d => d.MaTl == maTL && d.MaNd == maND);

            if (existingSave != null)
            {
                _context.TLYeuThiches.Remove(existingSave);
                _context.SaveChanges();
                return Json(new { success = true, saved = false });
            }
            else
            {
                var newSave = new TLYeuThich
                {
                    MaTl = maTL,
                    MaNd = maND,
                    ThoiGian = DateTime.Now
                };
                _context.TLYeuThiches.Add(newSave);
                _context.SaveChanges();
                return Json(new { success = true, saved = true });
            }
        }

        [HttpGet]
        public IActionResult GetNganhsByKhoa(string maKhoa)
        {
            var nganhs = _context.Nganhs.Where(n => n.MaKhoa == maKhoa)
                .Select(n => new { n.MaNganh, n.TenNganh }).ToList();
            return Json(nganhs);
        }

        [HttpGet]
        public IActionResult GetMonHocsByNganh(string maNganh)
        {
            var mons = _context.MonHocs.Where(m => m.MaNganh == maNganh)
                .Select(m => new { m.MaMonHoc, m.TenMonHoc }).ToList();
            return Json(mons);
        }

        [HttpGet]
        public IActionResult GetDiemByLoaiTL(string maLtl)
        {
            var loai = _context.LoaiTaiLieus.Include(l => l.MaDqNavigation).FirstOrDefault(l => l.MaLtl == maLtl);
            var diem = loai?.MaDqNavigation?.DiemTl ?? 0;
            return Json(diem);
        }

        // [POST] Thêm bình luận (AJAX)
        [HttpPost]
        public IActionResult AddComment(string maTL, string noiDung)
        {
            if (string.IsNullOrEmpty(noiDung)) return Json(new { success = false });
            var ticksBl = DateTime.Now.Ticks.ToString();
            var comment = new BinhLuan
            {
                MaBl = "B" + (ticksBl.Length > 4 ? ticksBl.Substring(ticksBl.Length - 4) : ticksBl.PadLeft(4, '0')),
                MaTl = maTL,
                MaNd = HttpContext.Session.GetString("MaTaiKhoan") ?? "SV001",
                NoiDung = noiDung,
                ThoiGian = DateTime.Now
            };
            _context.BinhLuans.Add(comment);
            _context.SaveChanges();
            var tenND = HttpContext.Session.GetString("TenNguoiDung") ?? "Người dùng";
            return Json(new { success = true, userName = tenND, content = noiDung, time = DateTime.Now.ToString("HH:mm") });
        }

        // GET: Documents/MyDocuments
        public IActionResult MyDocuments()
        {
            var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(maTaiKhoan))
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var maTaiKhoan = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(maTaiKhoan))
            {
                return RedirectToAction("Login", "Auth");
            }

            var taiLieu = await _context.TaiLieus.FirstOrDefaultAsync(t => t.MaTaiLieu == id && t.MaNguoiDang == maTaiKhoan);
            if (taiLieu == null)
            {
                return NotFound();
            }

            if (taiLieu.TrangThaiDuyet == "Chờ duyệt" && taiLieu.NgayDang.HasValue)
            {
                var timeDiff = (DateTime.Now - taiLieu.NgayDang.Value).TotalHours;
                if (timeDiff <= 2.0)
                {
                    _context.TaiLieus.Remove(taiLieu);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("MyDocuments");
        }

        // GET: Documents/GetMyUploads
        [HttpGet]
        public async Task<IActionResult> GetMyUploads()
        {
            try
            {
                var loaiNguoiDung = HttpContext.Session.GetString("LoaiNguoiDung");
                var maNguoiDung = loaiNguoiDung == "SinhVien"
                    ? HttpContext.Session.GetString("MaSinhVien")
                    : HttpContext.Session.GetString("MaGiangVien");

                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var taiLieuDaDang = await _context.TaiLieus
                    .Include(tl => tl.MaMonHocNavigation)
                    .Include(tl => tl.MaLoaiTlNavigation)
                    .Where(tl => tl.MaNguoiDang == maNguoiDung)
                    .OrderByDescending(tl => tl.NgayDang)
                    .Select(tl => new
                    {
                        maTaiLieu = tl.MaTaiLieu,
                        tieuDe = tl.TieuDe,
                        moTa = tl.MoTa,
                        loaiFile = tl.LoaiFile,
                        kichThuoc = tl.KichThuoc,
                        luotTai = tl.LuotTai,
                        ngayDang = tl.NgayDang,
                        trangThaiDuyet = tl.TrangThaiDuyet,
                        tenMonHoc = tl.MaMonHocNavigation != null ? tl.MaMonHocNavigation.TenMonHoc : null,
                        loaiTaiLieu = tl.MaLoaiTlNavigation != null ? tl.MaLoaiTlNavigation.TenLtl : null,
                        diemYeuCau = tl.DiemYeuCau
                    })
                    .ToListAsync();

                return Json(new { success = true, taiLieu = taiLieuDaDang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tài liệu đã đăng");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // GET: Documents/GetMySaved
        [HttpGet]
        public async Task<IActionResult> GetMySaved()
        {
            try
            {
                var loaiNguoiDung = HttpContext.Session.GetString("LoaiNguoiDung");
                var maNguoiDung = loaiNguoiDung == "SinhVien"
                    ? HttpContext.Session.GetString("MaSinhVien")
                    : HttpContext.Session.GetString("MaGiangVien");

                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var taiLieuDaLuu = await _context.TLYeuThiches
                    .Include(tlt => tlt.MaTlNavigation)
                        .ThenInclude(tl => tl.MaMonHocNavigation)
                    .Include(tlt => tlt.MaTlNavigation)
                        .ThenInclude(tl => tl.MaLoaiTlNavigation)
                    .Where(tlt => tlt.MaNd == maNguoiDung)
                    .OrderByDescending(tlt => tlt.ThoiGian)
                    .Select(tlt => new
                    {
                        maYeuThich = tlt.MaYeuThich,
                        thoiGianLuu = tlt.ThoiGian,
                        taiLieu = tlt.MaTlNavigation != null ? new
                        {
                            maTaiLieu = tlt.MaTlNavigation.MaTaiLieu,
                            tieuDe = tlt.MaTlNavigation.TieuDe,
                            moTa = tlt.MaTlNavigation.MoTa,
                            loaiFile = tlt.MaTlNavigation.LoaiFile,
                            kichThuoc = tlt.MaTlNavigation.KichThuoc,
                            luotTai = tlt.MaTlNavigation.LuotTai,
                            ngayDang = tlt.MaTlNavigation.NgayDang,
                            tenMonHoc = tlt.MaTlNavigation.MaMonHocNavigation != null ? tlt.MaTlNavigation.MaMonHocNavigation.TenMonHoc : null,
                            loaiTaiLieu = tlt.MaTlNavigation.MaLoaiTlNavigation != null ? tlt.MaTlNavigation.MaLoaiTlNavigation.TenLtl : null,
                            diemYeuCau = tlt.MaTlNavigation.DiemYeuCau
                        } : null
                    })
                    .ToListAsync();

                return Json(new { success = true, taiLieu = taiLieuDaLuu });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tài liệu đã lưu");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // GET: Documents/GetMyDownloads
        [HttpGet]
        public async Task<IActionResult> GetMyDownloads()
        {
            try
            {
                var loaiNguoiDung = HttpContext.Session.GetString("LoaiNguoiDung");
                var maNguoiDung = loaiNguoiDung == "SinhVien"
                    ? HttpContext.Session.GetString("MaSinhVien")
                    : HttpContext.Session.GetString("MaGiangVien");

                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                var lichSuTaiXuong = await _context.LichSuTaiXuongs
                    .Include(ls => ls.MaTaiLieuNavigation)
                        .ThenInclude(tl => tl.MaMonHocNavigation)
                    .Include(ls => ls.MaTaiLieuNavigation)
                        .ThenInclude(tl => tl.MaLoaiTlNavigation)
                    .Where(ls => ls.MaNd == maNguoiDung)
                    .OrderByDescending(ls => ls.NgayTai)
                    .Select(ls => new
                    {
                        maDownTl = ls.MaDownTL,
                        ngayTai = ls.NgayTai,
                        taiLieu = ls.MaTaiLieuNavigation != null ? new
                        {
                            maTaiLieu = ls.MaTaiLieuNavigation.MaTaiLieu,
                            tieuDe = ls.MaTaiLieuNavigation.TieuDe,
                            moTa = ls.MaTaiLieuNavigation.MoTa,
                            loaiFile = ls.MaTaiLieuNavigation.LoaiFile,
                            kichThuoc = ls.MaTaiLieuNavigation.KichThuoc,
                            luotTai = ls.MaTaiLieuNavigation.LuotTai,
                            ngayDang = ls.MaTaiLieuNavigation.NgayDang,
                            tenMonHoc = ls.MaTaiLieuNavigation.MaMonHocNavigation != null ? ls.MaTaiLieuNavigation.MaMonHocNavigation.TenMonHoc : null,
                            loaiTaiLieu = ls.MaTaiLieuNavigation.MaLoaiTlNavigation != null ? ls.MaTaiLieuNavigation.MaLoaiTlNavigation.TenLtl : null,
                            diemYeuCau = ls.MaTaiLieuNavigation.DiemYeuCau
                        } : null
                    })
                    .ToListAsync();

                return Json(new { success = true, lichSu = lichSuTaiXuong });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử tải xuống");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // [GET] Chi tiết tài liệu
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var taiLieu = _context.TaiLieus
                .Include(t => t.MaMonHocNavigation)
                    .ThenInclude(m => m.MaNganhNavigation)
                        .ThenInclude(n => n.MaKhoaNavigation)
                .Include(t => t.MaLoaiTlNavigation)
                .Include(t => t.BinhLuans)
                .Include(t => t.DanhGia)
                .FirstOrDefault(m => m.MaTaiLieu == id);

            if (taiLieu == null) return NotFound();

            var viewerRole = HttpContext.Session.GetString("LoaiNguoiDung");
            var viewerMaND = viewerRole == "SinhVien" ? HttpContext.Session.GetString("MaSinhVien") : HttpContext.Session.GetString("MaGiangVien");

            if (taiLieu.CheDoHienThi == false)
            {
                var maVaiTro = HttpContext.Session.GetString("MaVaiTro")?.Trim();
                bool isAdminOrCbk = maVaiTro == "VT001" || maVaiTro == "VT004" || maVaiTro == "VT002";

                if (taiLieu.MaNguoiDang != viewerMaND && !isAdminOrCbk)
                {
                    return NotFound();
                }
            }

            var tkDang = _context.TaiKhoans.Include(tk => tk.MaSvNavigation).FirstOrDefault(tk => tk.MaTk == taiLieu.MaNguoiDang);
            ViewBag.NguoiDang = tkDang?.MaSvNavigation;
            ViewBag.TenKhoa = taiLieu.MaMonHocNavigation?.MaNganhNavigation?.MaKhoaNavigation?.TenKhoa;
            ViewBag.TenNganh = taiLieu.MaMonHocNavigation?.MaNganhNavigation?.TenNganh;

            var comments = (from b in _context.BinhLuans
                            join tk in _context.TaiKhoans on b.MaNd equals tk.MaTk
                            join sv in _context.SinhViens on tk.MaSv equals sv.MaSv
                            where b.MaTl == id
                            select new { TenSv = sv.TenSv, NoiDung = b.NoiDung, ThoiGian = b.ThoiGian })
                            .OrderByDescending(x => x.ThoiGian).ToList();
            ViewBag.Comments = comments;

            ViewBag.IsSaved = !string.IsNullOrEmpty(viewerMaND) && _context.TLYeuThiches.Any(t => t.MaTl == id && t.MaNd == viewerMaND);

            bool isFullyVisible = false;

            if (!string.IsNullOrEmpty(viewerMaND))
            {
                if (taiLieu.MaNguoiDang == viewerMaND)
                {
                    isFullyVisible = true;
                }
                else if (_context.LichSuTaiXuongs.Any(ls => ls.MaTaiLieu == id && ls.MaNd == viewerMaND))
                {
                    isFullyVisible = true;
                }
                else if (viewerRole == "GiangVien")
                {
                    var uploaderTk = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTk == taiLieu.MaNguoiDang || tk.MaSv == taiLieu.MaNguoiDang || tk.MaGv == taiLieu.MaNguoiDang);
                    bool isUploaderStudent = uploaderTk?.MaSv != null || uploaderTk?.MaGv == null;
                    if (isUploaderStudent)
                    {
                        isFullyVisible = true;
                    }
                }
            }
            ViewBag.IsFullyVisible = isFullyVisible;

            ViewBag.PreviewUrl = _azureBlobService.GenerateSasLink(taiLieu.DuongDanFile, 20);

            return View(taiLieu);
        }

        // GET: Documents/Edit
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var taiLieu = _context.TaiLieus.Find(id);
            if (taiLieu == null) return NotFound();
            ViewBag.MaMonHoc = _context.MonHocs.ToList();
            ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
            return View(taiLieu);
        }

        // POST: Documents/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TaiLieu taiLieu, IFormFile fileUpdate)
        {
            if (id != taiLieu.MaTaiLieu) return NotFound();
            try
            {
                var existingDoc = _context.TaiLieus.AsNoTracking().FirstOrDefault(t => t.MaTaiLieu == id);
                if (fileUpdate != null && fileUpdate.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + fileUpdate.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await fileUpdate.CopyToAsync(fileStream);
                    taiLieu.DuongDanFile = uniqueFileName;
                    taiLieu.LoaiFile = Path.GetExtension(fileUpdate.FileName).Replace(".", "").ToUpper();
                }
                else
                {
                    taiLieu.DuongDanFile = existingDoc.DuongDanFile;
                    taiLieu.LoaiFile = existingDoc.LoaiFile;
                }
                taiLieu.MaNguoiDang = existingDoc.MaNguoiDang;
                taiLieu.NgayDang = existingDoc.NgayDang;
                taiLieu.TrangThaiDuyet = existingDoc.TrangThaiDuyet;
                taiLieu.LuotTai = existingDoc.LuotTai;
                _context.Update(taiLieu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TaiLieus.Any(e => e.MaTaiLieu == taiLieu.MaTaiLieu)) return NotFound();
                else throw;
            }
        }

        // GET: Documents/Report
        public IActionResult Report(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            return RedirectToAction("RecentReports", new { id });
        }

        // POST: Documents/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(BaoCaoViPham baoCao)
        {
            var maND = HttpContext.Session.GetString("MaTaiKhoan") ?? "SV001";
            var lastBC = _context.BaoCaoViPhams.OrderByDescending(b => b.MaBaoCao).FirstOrDefault();
            baoCao.MaBaoCao = lastBC != null ? "BC" + (int.Parse(lastBC.MaBaoCao.Substring(2)) + 1).ToString("D3") : "BC001";
            baoCao.NgayBaoCao = DateTime.Now;
            baoCao.TrangThaiXuLy = "Chờ xử lý";
            baoCao.NguoiBaoCao = maND;

            ModelState.Remove("MaBaoCao");
            ModelState.Remove("MaTaiLieuNavigation");
            ModelState.Remove("NguoiBaoCaoNavigation");

            if (ModelState.IsValid)
            {
                var taiLieu = await _context.TaiLieus.FirstOrDefaultAsync(t => t.MaTaiLieu == baoCao.MaTaiLieu);
                if (taiLieu != null)
                {
                    if (maND == taiLieu.MaNguoiDang)
                    {
                        taiLieu.CheDoHienThi = false;
                        _context.Update(taiLieu);
                        baoCao.LyDo = "Tác giả yêu cầu gỡ bài khẩn cấp";
                    }
                }

                _context.BaoCaoViPhams.Add(baoCao);
                var lastTB = _context.ThongBaos.OrderByDescending(t => t.MaTb).FirstOrDefault();
                var nextTB = lastTB != null ? "TB" + (int.Parse(lastTB.MaTb.Substring(2)) + 1).ToString("D3") : "TB001";
                _context.ThongBaos.Add(new ThongBao
                {
                    MaTb = nextTB,
                    TieuDe = "Báo cáo vi phạm",
                    NoiDung = $"CẢNH BÁO: Tài liệu '{baoCao.MaTaiLieu}' bị báo cáo vi phạm!",
                    NgayTao = DateTime.Now,
                    TrangThai = "Chưa đọc",
                    MaNguoiNhan = "ADMIN"
                });
                await _context.SaveChangesAsync();
                return RedirectToAction("RecentReports");
            }
            return RedirectToAction("RecentReports");
        }

        // GET: Documents/Upload
        public IActionResult Upload()
        {
            ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            return View();
        }

        // POST: Documents/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(TaiLieu taiLieu, IFormFile fileUpload)
        {
            if (taiLieu == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu tải lên không hợp lệ hoặc kích thước file vượt quá giới hạn cho phép (Tối đa 50MB)!";
                ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
                ViewBag.Khoas = _context.Khoas.ToList();
                return View(new TaiLieu());
            }

            if (string.IsNullOrWhiteSpace(taiLieu.TieuDe) || taiLieu.TieuDe.Trim().Length < 10)
            {
                TempData["ErrorMessage"] = "Tiêu đề quá ngắn! Vui lòng nhập ít nhất 10 ký tự.";
                ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
                ViewBag.Khoas = _context.Khoas.ToList();
                return View(taiLieu);
            }
            if (!taiLieu.TieuDe.Contains(" - "))
            {
                TempData["ErrorMessage"] = "Tiêu đề phải theo định dạng: [Tên môn] - [Loại tài liệu] - [Mô tả].";
                ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
                ViewBag.Khoas = _context.Khoas.ToList();
                return View(taiLieu);
            }
            var isDuplicate = await _context.TaiLieus.AnyAsync(t => t.TieuDe == taiLieu.TieuDe && t.MaMonHoc == taiLieu.MaMonHoc);
            if (isDuplicate)
            {
                TempData["ErrorMessage"] = "Tài liệu này đã tồn tại trong danh mục của môn học này!";
                ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
                ViewBag.Khoas = _context.Khoas.ToList();
                return View(taiLieu);
            }
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var loaiND = HttpContext.Session.GetString("LoaiNguoiDung");
                var maNDPre = loaiND == "SinhVien"
                    ? HttpContext.Session.GetString("MaSinhVien")
                    : HttpContext.Session.GetString("MaGiangVien");
                string ext = Path.GetExtension(fileUpload.FileName);
                string standardFileName = GenerateStandardFileName(taiLieu.MaMonHoc, taiLieu.MaLoaiTl, taiLieu.TieuDe, maNDPre, ext);

                using var memoryStream = new MemoryStream();
                await fileUpload.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                string fileHash = "";
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(fileBytes);
                    fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }

                if (!string.IsNullOrEmpty(fileHash))
                {
                    // Kiểm tra trùng lặp tệp tin tuyệt đối bằng mã băm (Hash)
                    var exactDuplicate = await _context.TaiLieus
                        .Where(t => t.MoTa != null && t.MoTa.Contains(fileHash))
                        .FirstOrDefaultAsync();

                    if (exactDuplicate != null)
                    {
                        TempData["ErrorMessage"] = $"TÀI LIỆU BỊ CHẶN: File này đã tồn tại trên hệ thống (Trùng với tài liệu: {exactDuplicate.TieuDe}). Vui lòng không upload lại cùng một nội dung file vật lý.";
                        ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
                        ViewBag.Khoas = _context.Khoas.ToList();
                        return View(taiLieu);
                    }
                }

                memoryStream.Position = 0;
                await _azureBlobService.UploadFileAsync(memoryStream, standardFileName);

                string rawText = string.Empty;
                try
                {
                    memoryStream.Position = 0;
                    rawText = await _nlpService.ExtractTextAsync(memoryStream, ext);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lỗi khi trích xuất văn bản từ file");
                }

                var globalCandidates = await _context.TaiLieus
                    .OrderByDescending(t => t.NgayDang)
                    .Take(30)
                    .Select(t => new { t.MaTaiLieu, t.TieuDe, t.MoTa })
                    .ToListAsync();

                var listToCompare = new List<ExistingDocInfo>();
                foreach (var doc in globalCandidates)
                {
                    string fingerprint = doc.TieuDe;
                    if (!string.IsNullOrEmpty(doc.MoTa) && doc.MoTa.Contains("--- AI EVALUATION ---"))
                    {
                        try
                        {
                            var jsonPart = doc.MoTa.Split("--- AI EVALUATION ---").Last().Trim();
                            var oldEval = System.Text.Json.JsonSerializer.Deserialize<RarenessEvaluation>(jsonPart);
                            if (!string.IsNullOrEmpty(oldEval?.ContentFingerprint)) fingerprint = oldEval.ContentFingerprint;
                        }
                        catch { }
                    }
                    listToCompare.Add(new ExistingDocInfo { MaTaiLieu = doc.MaTaiLieu, TieuDe = doc.TieuDe, ContentFingerprint = fingerprint });
                }

                var loaiTl = _context.LoaiTaiLieus.Include(l => l.MaDqNavigation).FirstOrDefault(l => l.MaLtl == taiLieu.MaLoaiTl);
                int diemYc = loaiTl?.MaDqNavigation?.DiemTl ?? 0;

                var evaluation = await _nlpService.EvaluateRarenessAsync(
                    taiLieu.TieuDe,
                    ext.Replace(".", "").ToUpper(),
                    loaiTl?.TenLtl ?? "Tài liệu",
                    taiLieu.Nxb ?? "Tác giả tự do",
                    taiLieu.NamXb,
                    diemYc,
                    rawText,
                    listToCompare
                );

                // Giữ nguyên Security_Note từ NLP Service để không làm hiển thị box vàng vô lý ở UI Details
                if (evaluation.Security_Note == null) evaluation.Security_Note = "Normal";
                evaluation.FileHash = fileHash;

                if (evaluation.Similarity_Check?.Similarity_Percentage >= 50)
                {
                    TempData["ErrorMessage"] = $"TÀI LIỆU BỊ CHẶN: AI phát hiện trùng lặp nội dung ({evaluation.Similarity_Check.Similarity_Percentage}%). Trùng với: {evaluation.Similarity_Check.Matched_With_Document}";
                    ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
                    ViewBag.Khoas = _context.Khoas.ToList();
                    return View(taiLieu);
                }

                string evalJson = System.Text.Json.JsonSerializer.Serialize(evaluation, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                // Lưu metadata ẩn vào MoTa để hỗ trợ truy vấn nhanh và chống upload trùng file vật lý
                taiLieu.MoTa = (taiLieu.MoTa ?? "") + "\n\n--- AI EVALUATION ---\n" + evalJson + $"\n[METADATA_HASH:{fileHash}]";

                taiLieu.DuongDanFile = standardFileName;
                taiLieu.LoaiFile = ext.Replace(".", "").ToUpper();
                if (taiLieu.LoaiFile?.Length > 10) taiLieu.LoaiFile = taiLieu.LoaiFile.Substring(0, 10);
                taiLieu.TrangThaiDuyet = "Chờ duyệt";
                taiLieu.NgayDang = DateTime.Now;
                taiLieu.LuotTai = 0;
                taiLieu.LanTaiBan = taiLieu.LanTaiBan ?? 1;

                double rarenessScore = evaluation.Evaluation.Final_Rareness_Score;
                if (rarenessScore >= 6.0)
                {
                    taiLieu.DiemYeuCau = (int)Math.Round(diemYc + rarenessScore);
                }
                else
                {
                    taiLieu.DiemYeuCau = diemYc;
                }

                var lastItem = _context.TaiLieus.OrderByDescending(t => t.MaTaiLieu).FirstOrDefault();
                taiLieu.MaTaiLieu = lastItem != null
                    ? "TL" + (int.Parse(lastItem.MaTaiLieu.Substring(2)) + 1).ToString("D3")
                    : "TL001";

                var loaiNguoiDung = HttpContext.Session.GetString("LoaiNguoiDung");
                taiLieu.MaNguoiDang = loaiNguoiDung == "SinhVien"
                    ? HttpContext.Session.GetString("MaSinhVien")
                    : HttpContext.Session.GetString("MaGiangVien");

                _context.TaiLieus.Add(taiLieu);

                var lastTB = _context.ThongBaos.OrderByDescending(t => t.MaTb).FirstOrDefault();
                var nextTB = lastTB != null ? "TB" + (int.Parse(lastTB.MaTb.Substring(2)) + 1).ToString("D3") : "TB001";
                _context.ThongBaos.Add(new ThongBao
                {
                    MaTb = nextTB,
                    TieuDe = "Tài liệu mới",
                    NoiDung = $"Có tài liệu mới '{taiLieu.TieuDe}' đang chờ duyệt.",
                    NgayTao = DateTime.Now,
                    TrangThai = "Chưa đọc",
                    MaNguoiNhan = "ADMIN"
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MaLoaiTl = _context.LoaiTaiLieus.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            return View(taiLieu);
        }

        // [GET] Kiểm tra điểm trước khi tải
        [HttpGet]
        public async Task<IActionResult> CheckDownloadPoints(string id)
        {
            var maSV = HttpContext.Session.GetString("MaSinhVien");
            if (string.IsNullOrEmpty(maSV)) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu == null) return Json(new { success = false, message = "Tài liệu không tồn tại!" });

            var loaiND = HttpContext.Session.GetString("LoaiNguoiDung");
            bool isOwner = taiLieu.MaNguoiDang == maSV;
            bool isStudent = loaiND == "SinhVien";

            if (isStudent && !isOwner && (taiLieu.DiemYeuCau ?? 0) > 0)
            {
                var sinhVien = await _context.SinhViens.FindAsync(maSV);
                if (sinhVien == null) return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên!" });

                if ((sinhVien.DiemTichLuy ?? 0) < (taiLieu.DiemYeuCau ?? 0))
                {
                    return Json(new
                    {
                        success = false,
                        needsPoints = true,
                        required = taiLieu.DiemYeuCau,
                        current = sinhVien.DiemTichLuy,
                        message = $"Bạn không đủ điểm! Cần {taiLieu.DiemYeuCau} điểm (Hiện có {sinhVien.DiemTichLuy})."
                    });
                }
                return Json(new { success = true, confirmNeeded = true, points = taiLieu.DiemYeuCau });
            }

            return Json(new { success = true, confirmNeeded = false });
        }

        // [GET] Tải tài liệu
        public async Task<IActionResult> Download(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var maSV = HttpContext.Session.GetString("MaSinhVien");
            var maND = HttpContext.Session.GetString("MaTaiKhoan");

            if (string.IsNullOrEmpty(maND))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tải tài liệu!";
                return RedirectToAction("Login", "Auth");
            }

            var taiLieu = await _context.TaiLieus.Include(t => t.MaLoaiTlNavigation).FirstOrDefaultAsync(t => t.MaTaiLieu == id);
            if (taiLieu == null) return NotFound();

            var loaiNguoiDung = HttpContext.Session.GetString("LoaiNguoiDung");
            bool isOwner = taiLieu.MaNguoiDang == maND || taiLieu.MaNguoiDang == maSV;
            bool isLecturer = loaiNguoiDung == "GiangVien";
            bool isStudent = loaiNguoiDung == "SinhVien";

            if (isStudent && !isOwner && (taiLieu.DiemYeuCau ?? 0) > 0)
            {
                var sinhVien = await _context.SinhViens.FindAsync(maSV);
                if (sinhVien == null) return NotFound();

                if ((sinhVien.DiemTichLuy ?? 0) < (taiLieu.DiemYeuCau ?? 0))
                {
                    TempData["ErrorMessage"] = $"Bạn không đủ điểm để tải tài liệu này! Tài liệu này rất quý, cần {taiLieu.DiemYeuCau} điểm để tải (Bạn hiện có {sinhVien.DiemTichLuy} điểm).";
                    return RedirectToAction("Details", new { id });
                }

                sinhVien.DiemTichLuy -= taiLieu.DiemYeuCau;
                var currentHK = await _context.HocKies.OrderByDescending(h => h.MaHk).FirstOrDefaultAsync();

                _context.LichSuDiems.Add(new LichSuDiem
                {
                    MaSv = maSV,
                    SoDiemThayDoi = -(taiLieu.DiemYeuCau ?? 0),
                    LyDo = $"Tải tài liệu độ quý cao: {taiLieu.TieuDe}",
                    NgayThayDoi = DateTime.Now,
                    MaHk = currentHK?.MaHk
                });

                HttpContext.Session.SetString("DiemTichLuy", sinhVien.DiemTichLuy?.ToString() ?? "0");
            }

            if (!isOwner)
            {
                _context.LichSuTaiXuongs.Add(new LichSuTaiXuong
                {
                    MaTaiLieu = id,
                    MaNd = maND,
                    NgayTai = DateTime.Now
                });
                taiLieu.LuotTai = (taiLieu.LuotTai ?? 0) + 1;
                await _context.SaveChangesAsync();
            }

            string sasUrl = _azureBlobService.GenerateSasLink(taiLieu.DuongDanFile, 5);
            if (string.IsNullOrEmpty(sasUrl))
            {
                TempData["ErrorMessage"] = "Không thể lấy liên kết tải xuống từ Cloud!";
                return RedirectToAction("Details", new { id });
            }
            return Redirect(sasUrl);
        }

        // [GET] Danh sách báo cáo vi phạm
        public IActionResult RecentReports(string id)
        {
            ViewBag.DanhSachTaiLieu = _context.TaiLieus.Where(t => t.TrangThaiDuyet == "Đã duyệt").ToList();
            ViewBag.SelectedId = id;
            var lichSu = _context.BaoCaoViPhams
                .Include(b => b.MaTaiLieuNavigation)
                .OrderByDescending(b => b.NgayBaoCao)
                .ToList();
            return View(lichSu);
        }
    }
}