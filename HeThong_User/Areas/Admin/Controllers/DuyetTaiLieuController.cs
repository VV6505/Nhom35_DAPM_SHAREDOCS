using HeThong_User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThong_User.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DuyetTaiLieuController : Controller
    {
        private readonly HeThongChiaSeTaiLieu_V1 _context;
        private readonly HeThong_User.Services.AzureBlobService _azureBlobService;

        private const double TimeDelay = 30.0 / 3600.0; // Hằng số thời gian delay (30 giây quy ra giờ)

        public DuyetTaiLieuController(HeThongChiaSeTaiLieu_V1 context, HeThong_User.Services.AzureBlobService azureBlobService)
        {
            _context = context;
            _azureBlobService = azureBlobService;
        }

        // [GET] Danh sách phê duyệt: Lấy danh sách tài liệu cần duyệt dựa trên vai trò (Admin thấy bài của toàn trường, CBK thấy bài của khoa).
        public async Task<IActionResult> Index(string? trangthai, string? khoa, string? search)
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            var adminRole = HttpContext.Session.GetString("AdminRole");

            var cutOffTime = DateTime.Now.AddSeconds(-30);
            var query = _context.TaiLieus
                .Include(t => t.MaMonHocNavigation)
                    .ThenInclude(m => m!.MaNganhNavigation)
                        .ThenInclude(n => n!.MaKhoaNavigation)
                .Where(t => t.NgayDang == null || t.NgayDang < cutOffTime) // Ẩn tài liệu trong vòng 30 giây ân hạn
                .AsQueryable();

            // Phân quyền hiển thị: Cán bộ khoa chỉ thấy bài của khoa mình, Admin thấy bài từ khoa gửi lên
            if (adminRole == "VT004" || adminRole == "VT002") 
            {
                var tk = await _context.TaiKhoans.Include(x => x.MaGvNavigation).FirstOrDefaultAsync(x => x.MaTk == adminId);
                var maKhoaCBK = tk?.MaGvNavigation?.MaKhoa;

                if (!string.IsNullOrEmpty(maKhoaCBK))
                {
                    // Lọc bài thuộc khoa của cán bộ đang đăng nhập
                    query = query.Where(t => t.MaMonHocNavigation != null
                        && t.MaMonHocNavigation.MaNganhNavigation != null
                        && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == maKhoaCBK);

                    if (string.IsNullOrEmpty(trangthai))
                        query = query.Where(t => t.TrangThaiDuyet == "Chờ duyệt");
                }
            }
            else if (adminRole == "VT001") 
            {
                // Admin xử lý nốt các bài đã qua vòng duyệt của Khoa hoặc bài tự do
                if (string.IsNullOrEmpty(trangthai))
                {
                    query = query.Where(t => t.TrangThaiDuyet == "Chờ Admin duyệt" 
                        || (t.TrangThaiDuyet == "Chờ duyệt" && (t.MaMonHocNavigation == null || t.MaMonHocNavigation.MaNganhNavigation == null)));
                }
                else if (trangthai == "Chờ duyệt")
                {
                    // Nếu chọn cụ thể "Chờ duyệt", chỉ hiện bài tự do không thuộc Khoa/Môn nào
                    query = query.Where(t => t.TrangThaiDuyet == "Chờ duyệt" && (t.MaMonHocNavigation == null || t.MaMonHocNavigation.MaNganhNavigation == null));
                }
            }

            // Lọc trạng thái (nếu chọn cụ thể và không phải là Chờ duyệt của Admin để tránh đè điều kiện trên)
            if (!string.IsNullOrEmpty(trangthai) && trangthai != "all" && !(adminRole == "VT001" && trangthai == "Chờ duyệt"))
            {
                query = query.Where(t => t.TrangThaiDuyet == trangthai);
            }

            // Lọc theo khoa (dành cho Admin)
            if (adminRole == "VT001" && !string.IsNullOrEmpty(khoa) && khoa != "all")
            {
                query = query.Where(t => t.MaMonHocNavigation != null
                    && t.MaMonHocNavigation.MaNganhNavigation != null
                    && t.MaMonHocNavigation.MaNganhNavigation.MaKhoa == khoa);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TieuDe.Contains(search));
            }

            var taiLieus = await query.OrderByDescending(t => t.NgayDang).ToListAsync();
            var khoas = await _context.Khoas.OrderBy(k => k.TenKhoa).ToListAsync();

            var nguoiDangMap = new Dictionary<string, string>();
            var previewLinks = new Dictionary<string, string>();
            foreach (var tl in taiLieus)
            {
                if (!string.IsNullOrEmpty(tl.MaNguoiDang) && !nguoiDangMap.ContainsKey(tl.MaNguoiDang))
                {
                    var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.MaSv == tl.MaNguoiDang);
                    var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.MaGv == tl.MaNguoiDang);
                    nguoiDangMap[tl.MaNguoiDang] = sv?.TenSv ?? gv?.TenGv ?? tl.MaNguoiDang;
                }

                if (!string.IsNullOrEmpty(tl.DuongDanFile))
                {
                    previewLinks[tl.MaTaiLieu] = _azureBlobService.GenerateSasLink(tl.DuongDanFile, 60);
                }
            }

            ViewBag.Khoas = khoas;
            ViewBag.TrangThai = trangthai;
            ViewBag.Khoa = khoa;
            ViewBag.Search = search;
            ViewBag.NguoiDangMap = nguoiDangMap;
            ViewBag.PreviewLinks = previewLinks;

            return View(taiLieus);
        }

        // [POST] Phê duyệt: Chuyển trạng thái tài liệu thành "Chờ Admin duyệt" (nếu là CBK) hoặc "Đã duyệt" (nếu là Admin).
        [HttpPost]
        public async Task<IActionResult> Duyet(string id)
        {
            var adminRole = HttpContext.Session.GetString("AdminRole");
            var tl = await _context.TaiLieus.FindAsync(id);
            
            if (tl != null)
            {
                // Nếu Khoa duyệt -> Đẩy lên cho Admin xác nhận. Nếu Admin duyệt -> Cho phép hiển thị luôn.
                if (adminRole == "VT004" || adminRole == "VT002") 
                {
                    tl.TrangThaiDuyet = "Chờ Admin duyệt";
                }
                else if (adminRole == "VT001") 
                {
                    tl.TrangThaiDuyet = "Đã duyệt";
                    tl.CheDoHienThi = true; // Khôi phục hiển thị/mở ẩn tài liệu khi được duyệt

                    // CỘNG ĐIỂM THƯỞNG CHO NGƯỜI ĐĂNG (NẾU LÀ SINH VIÊN) - CHỈ KHI ADMIN PHÊ DUYỆT XONG
                    var uploader = await _context.SinhViens.FindAsync(tl.MaNguoiDang);
                    if (uploader != null)
                    {
                        // Lấy học kỳ hiện tại
                        var currentHK = await _context.HocKies.OrderByDescending(h => h.MaHk).FirstOrDefaultAsync();

                        // Thưởng điểm bằng đúng giá trị của tài liệu (Base + Rareness nếu có)
                        int rewardPoints = tl.DiemYeuCau ?? 0;
                        uploader.DiemTichLuy = (uploader.DiemTichLuy ?? 0) + rewardPoints;

                        _context.LichSuDiems.Add(new LichSuDiem
                        {
                            MaSv = tl.MaNguoiDang,
                            SoDiemThayDoi = rewardPoints,
                            LyDo = $"Thưởng điểm đóng góp tài liệu được phê duyệt: {tl.TieuDe}",
                            NgayThayDoi = DateTime.Now,
                            MaHk = currentHK?.MaHk
                        });
                    }
                }
                // GỬI THÔNG BÁO CHO NGƯỜI ĐĂNG
                var lastTB_AP = _context.ThongBaos.OrderByDescending(t => t.MaTb).FirstOrDefault();
                var nextTB_AP = lastTB_AP != null 
                    ? "TB" + (int.Parse(lastTB_AP.MaTb.Substring(2)) + 1).ToString("D3") 
                    : "TB001";

                var thongBao = new ThongBao
                {
                    MaTb = nextTB_AP,
                    TieuDe = "Kết quả phê duyệt",
                    NoiDung = (adminRole == "VT004" || adminRole == "VT002")
                        ? $"Tài liệu '{tl.TieuDe}' đã được Khoa duyệt, chờ Admin xác nhận."
                        : $"Chúc mừng! Tài liệu '{tl.TieuDe}' của bạn đã được phê duyệt.",
                    NgayTao = DateTime.Now,
                    TrangThai = "Chưa đọc",
                    MaNguoiNhan = tl.MaNguoiDang
                };
                _context.ThongBaos.Add(thongBao);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // [POST] Từ chối: Chuyển trạng thái tài liệu thành "Từ chối" và lưu lại lý do để người đăng được biết.
        [HttpPost]
        public async Task<IActionResult> TuChoi(string id, string lyDo)
        {
            var tl = await _context.TaiLieus.FindAsync(id);
            if (tl != null)
            {
                tl.TrangThaiDuyet = "Từ chối";
                tl.LyDoTuChoi = lyDo;

                // GỬI THÔNG BÁO CHO NGƯỜI ĐĂNG
                var lastTB_RJ = _context.ThongBaos.OrderByDescending(t => t.MaTb).FirstOrDefault();
                var nextTB_RJ = lastTB_RJ != null 
                    ? "TB" + (int.Parse(lastTB_RJ.MaTb.Substring(2)) + 1).ToString("D3") 
                    : "TB001";

                var thongBao = new ThongBao
                {
                    MaTb = nextTB_RJ,
                    TieuDe = "Tài liệu bị từ chối",
                    NoiDung = $"Rất tiếc, tài liệu '{tl.TieuDe}' bị từ chối. Lý do: {lyDo}",
                    NgayTao = DateTime.Now,
                    TrangThai = "Chưa đọc",
                    MaNguoiNhan = tl.MaNguoiDang
                };
                _context.ThongBaos.Add(thongBao);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(string id)
        {
            var tl = await _context.TaiLieus.FindAsync(id);
            if (tl != null)
            {
                _context.TaiLieus.Remove(tl);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
