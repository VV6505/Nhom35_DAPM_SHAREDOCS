using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace HeThong_User.Services
{
    public class NLPService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NLPService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> ExtractTextAsync(Stream fileStream, string extension)
        {
            try
            {
                if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    using (var document = PdfDocument.Open(fileStream))
                    {
                        var text = new StringBuilder();
                        foreach (var page in document.GetPages())
                        {
                            text.Append(page.Text);
                        }
                        return text.ToString();
                    }
                }
                else if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    using (var wordDocument = WordprocessingDocument.Open(fileStream, false))
                    {
                        var body = wordDocument.MainDocumentPart.Document.Body;
                        return body.InnerText;
                    }
                }
                // Basic text extraction for other types or fallback
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<RarenessEvaluation> EvaluateRarenessAsync(string tieuDe, string loaiFile, string loaiTaiLieu, string nxb, int? namXb, int diemYeuCau, string rawText, List<ExistingDocInfo> existingDocs)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                var fallback = CalculateLocalHeuristic(tieuDe, loaiTaiLieu, nxb, namXb, diemYeuCau, rawText);
                return fallback;
            }

            var existingDocsJson = JsonSerializer.Serialize(existingDocs ?? new List<ExistingDocInfo>());

            var prompt = $@"
[ROLE & CONTEXT]
Bạn là một công cụ AI chuyên trách (NLP Engine) tích hợp tại phân hệ Backend Admin của dự án ""Hệ thống chia sẻ tài liệu học tập trường Đại học Sư phạm Kỹ thuật Đà Nẵng"". Nhiệm vụ của bạn là kiểm tra trùng lặp (đạo văn), kiểm tra tính toàn vẹn dữ liệu chống gian lận, và tính toán Điểm Độ Quý (Rareness Score - RS) theo thuật toán Heuristic trọng số trước khi ghi nhận vào Database.
[INPUT DATA FROM INTERFACE]
- Tiêu đề tài liệu mới: {tieuDe}
- Định dạng tệp: {loaiFile}
- Loại tài liệu: {loaiTaiLieu}
- Nhà xuất bản / Đơn vị tác quyền: {nxb}
- Năm xuất bản (do người dùng nhập): {namXb}
- Điểm yêu cầu tích lũy: {diemYeuCau}
[CHỐT CHẶN 1: PLAGIARISM & SIMILARITY CHECK (KIỂM TRA ĐẠO VĂN)]
Hãy tiến hành so sánh nội dung tại mục [RAW TEXT INPUT FOR NLP PROCESSING] với danh sách các văn bản cũ tại mục [EXISTING DATABASE TEXTS] để tính toán tỷ lệ tương đồng (Similarity Percentage):
1. Ngưỡng Chặn Cứng (Tỷ lệ >= 50%): Xác định là sao chép thô bạo hoặc đổi tên file. Bạn phải LẬP TỨC dừng luồng tính điểm, gán Final_Rareness_Score = 0.0, gán nhãn Classification = ""BỊ CHẶN - ĐẠO VĂN >=50%"".
2. Ngưỡng Chờ Duyệt (Tỷ lệ từ 25% đến dưới 50%): Tài liệu có dấu hiệu sao chép một phần. Vẫn tiến hành tính điểm Heuristic như bình thường nhưng bắt buộc phải đánh dấu trạng thái nghi vấn vào chuỗi ""Security_Note"".
[CHỐT CHẶN 2: ANTI-FRAUD & DATA VERIFICATION RULES]
(Chỉ thực hiện nếu tỷ lệ trùng < 50%)
1. Kiểm tra chéo năm xuất bản (Cross-Verification): Quét nội dung văn bản thô của file mới để tìm mốc thời gian thực tế (Ví dụ: Trang bìa ghi ""năm 2026""). Nếu phát hiện người dùng nhập năm trên Form ({namXb}) là năm cũ (1980, 1990) nhằm thao túng điểm khoảng cách năm nhưng ruột file là năm mới (2026), bạn phải lấy năm thực tế trong file (2026) để tính toán và hạ điểm tiêu chí năm về mức sàn 2.0.
2. Xử lý dữ liệu sai/rác/khuyết thiếu: Nếu trường dữ liệu bị rỗng (Null), chứa ký tự rác vô nghĩa (""asdfgh"", ""12345""), hoặc số điểm yêu cầu bị nhập số âm, hãy tự động ép các giá trị này về mức điểm sàn an toàn (Mặc định = 2.0 điểm).
[RULES FOR HEURISTIC SCORING (SCORING MATRIX)]
(Chỉ áp dụng nếu tài liệu vượt qua vòng kiểm tra trùng lặp < 50%. Thang điểm từ 1.0 đến 10.0):
1. Yếu tố 1: Tính chất nội dung và Thể loại (Trọng số: 0.35)
   - Nếu Loại tài liệu là ""Giáo trình"", ""Đồ án tốt nghiệp"", ""Luận văn"" hoặc ruột file chứa từ khóa học thuật trọng tâm như ""Đồ án phần mềm"", ""Báo cáo kết quả"", ""Usecase"", ""Database"": 10 điểm.
   - Nếu Loại tài liệu là ""Slide bài giảng"", ""Đề cương chi tiết"", ""Tài liệu hướng dẫn thực hành"": 6 điểm.
   - Nếu là tài liệu tham khảo ngắn, tiểu luận, bài tập nộp tuần thông thường hoặc tiêu đề chứa ký tự rác: 3 điểm.
2. Yếu tố 2: Đơn vị cấp phép và Tác quyền (Trọng số: 0.20)
   - Nếu Nhà xuất bản thuộc các đơn vị chính thống (""NXB Giáo dục"", ""NXB Đại học Quốc gia"", ""NXB Khoa học Kỹ thuật""): 10 điểm.
   - Nếu ghi nhận ""Giảng viên biên soạn"" hoặc ""Nội bộ trường ĐH SPKT"": 7 điểm.
   - Nếu ghi nhận ""Tác giả tự do"", thông tin chứa ký tự rác hoặc bị bỏ trống: 3 điểm.
3. Yếu tố 3: Khoảng cách năm phát hành (Trọng số: 0.20)
   - Lấy năm hiện tại hệ thống mặc định là năm 2026 trừ đi [Năm xuất bản] (Năm đã qua kiểm tra chéo).
   - Nếu khoảng cách thời gian trên 20 năm: 10 điểm.
   - Nếu khoảng cách thời gian từ 5 đến 20 năm: 6 điểm.
   - Nếu khoảng cách dưới 5 năm, trùng năm hiện tại (2026), hoặc phát hiện gian lận năm: 2.0 điểm.
4. Yếu tố 4: Giá trị trao đổi hệ thống - Điểm yêu cầu (Trọng số: 0.25)
   - Nếu Điểm yêu cầu >= 50 điểm: 10 điểm.
   - Nếu Điểm yêu cầu từ 20 đến dưới 50 điểm: 7 điểm.
   - Nếu Điểm yêu cầu từ 5 đến dưới 20 điểm: 5 điểm.
   - Nếu Điểm yêu cầu = 0 (Tài liệu miễn phí) hoặc bị nhập số âm: 2 điểm.
[MATHEMATICAL FORMULA]
Final_Rareness_Score = (Điểm_Yếu_Tố_1 * 0.35) + (Điểm_Yếu_Tố_2 * 0.20) + (Điểm_Yếu_Tố_3 * 0.20) + (Điểm_Yếu_Tố_4 * 0.25)
[STRICT OUTPUT FORMAT CONSTRAINTS]
- CHỈ trả về duy nhất một chuỗi định dạng JSON hợp lệ, không bọc khối dạng ```json ... ```, không kèm chữ thừa.
{{
  ""Similarity_Check"": {{
    ""Is_Plagiarized"": false,
    ""Similarity_Percentage"": 0.0,
    ""Matched_With_Document"": ""Tiêu đề file cũ bị trùng (ghi 'None' nếu < 25%)""
  }},
  ""Evaluation"": {{
    ""S1_ContentType_Score"": 0.0,
    ""S2_Authority_Score"": 0.0,
    ""S3_YearDistance_Score"": 0.0,
    ""S4_PointsRequired_Score"": 0.0,
    ""Final_Rareness_Score"": 0.0
  }},
  ""Classification"": ""Chuỗi nhãn"",
  ""Security_Note"": ""Ghi cụ thể lỗi ví dụ: 'Normal' hoặc 'Phát hiện đạo văn mức độ nhẹ: trùng X% với tài liệu Y' hoặc 'Phát hiện khai gian năm xuất bản'""
}}
[EXISTING DATABASE TEXTS]
{existingDocsJson}
[RAW TEXT INPUT FOR NLP PROCESSING]
{rawText}
";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}",
                    new
                    {
                        contents = new[] { new { parts = new[] { new { text = prompt } } } }
                    });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var contentText = result.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    return JsonSerializer.Deserialize<RarenessEvaluation>(contentText);
                }
            }
            catch (Exception) { }

            return CalculateLocalHeuristic(tieuDe, loaiTaiLieu, nxb, namXb, diemYeuCau, rawText);
        }

        private RarenessEvaluation CalculateLocalHeuristic(string tieuDe, string loaiTaiLieu, string nxb, int? namXb, int diemYeuCau, string rawText)
        {
            string securityNote = "Normal";
            
            // Check for junk data
            if (string.IsNullOrWhiteSpace(tieuDe) || tieuDe.Length < 3 || Regex.IsMatch(tieuDe, @"^[a-zA-Z0-9]{10,}$"))
            {
                securityNote = "Phát hiện tiêu đề có dấu hiệu dữ liệu rác.";
            }

            double s1 = 3.0;
            var keywords = new[] { "Đồ án phần mềm", "Báo cáo kết quả", "Usecase", "Database", "Giáo trình", "Đồ án tốt nghiệp", "Luận văn" };
            if (loaiTaiLieu.Contains("Giáo trình") || loaiTaiLieu.Contains("Đồ án tốt nghiệp") || loaiTaiLieu.Contains("Luận văn") || keywords.Any(k => (tieuDe + rawText).Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                s1 = 10.0;
            }
            else if (loaiTaiLieu.Contains("Slide") || loaiTaiLieu.Contains("Đề cương") || loaiTaiLieu.Contains("Thực hành"))
            {
                s1 = 6.0;
            }

            double s2 = 3.0;
            if (string.IsNullOrEmpty(nxb) || Regex.IsMatch(nxb, @"^[a-zA-Z0-9]{10,}$")) { s2 = 3.0; }
            else if (nxb.Contains("NXB Giáo dục") || nxb.Contains("NXB Đại học Quốc gia") || nxb.Contains("NXB Khoa học Kỹ thuật")) { s2 = 10.0; }
            else if (nxb.Contains("Giảng viên biên soạn") || nxb.Contains("SPKT")) { s2 = 7.0; }

            // Year Fraud Detection
            int actualNamXb = namXb ?? 2026;
            var yearMatches = Regex.Matches(rawText, @"\b(19|20)\d{2}\b");
            int maxFoundYear = 0;
            foreach (Match match in yearMatches)
            {
                if (int.TryParse(match.Value, out int year))
                {
                    if (year > maxFoundYear && year <= 2026) maxFoundYear = year;
                }
            }

            bool isFraud = false;
            if (namXb.HasValue && namXb.Value < 2000 && maxFoundYear >= 2020)
            {
                actualNamXb = maxFoundYear;
                isFraud = true;
                securityNote = $"Phát hiện hành vi gian lận năm xuất bản. Năm nhập: {namXb}, Năm thực tế ước tính: {maxFoundYear}";
            }

            double s3 = 2.0;
            if (isFraud) { s3 = 2.0; }
            else
            {
                int diff = 2026 - actualNamXb;
                if (diff > 20) { s3 = 10.0; }
                else if (diff >= 5) { s3 = 6.0; }
                else { s3 = 2.0; }
            }

            double s4 = 2.0;
            int effectiveDiem = diemYeuCau < 0 ? 0 : diemYeuCau;
            if (effectiveDiem >= 50) { s4 = 10.0; }
            else if (effectiveDiem >= 20) { s4 = 7.0; }
            else if (effectiveDiem >= 5) { s4 = 5.0; }
            else if (effectiveDiem == 0) { s4 = 2.0; }

            double final = (s1 * 0.35) + (s2 * 0.20) + (s3 * 0.20) + (s4 * 0.25);

            string classification = "Thông Thường";
            if (final >= 8.5) classification = "Rất Quý";
            else if (final >= 6.0) classification = "Quý / Hiếm";

            return new RarenessEvaluation
            {
                Similarity_Check = new SimilarityCheckResult { Is_Plagiarized = false, Similarity_Percentage = 0, Matched_With_Document = "None" },
                Evaluation = new EvaluationScores
                {
                    S1_ContentType_Score = s1,
                    S2_Authority_Score = s2,
                    S3_YearDistance_Score = s3,
                    S4_PointsRequired_Score = s4,
                    Final_Rareness_Score = final
                },
                Classification = classification,
                Security_Note = securityNote
            };
        }
    }

    public class ExistingDocInfo
    {
        public string MaTaiLieu { get; set; }
        public string TieuDe { get; set; }
        public string ContentFingerprint { get; set; }
    }

    public class RarenessEvaluation
    {
        public SimilarityCheckResult Similarity_Check { get; set; }
        public EvaluationScores Evaluation { get; set; }
        public string Classification { get; set; }
        public string Security_Note { get; set; }
        public string ContentFingerprint { get; set; } // Keep for potential future use or internal tracking
        public EvaluationReasoning Reasoning { get; set; }
    }

    public class SimilarityCheckResult
    {
        public bool Is_Plagiarized { get; set; }
        public double Similarity_Percentage { get; set; }
        public string Matched_With_Document { get; set; }
    }

    public class EvaluationScores
    {
        public double S1_ContentType_Score { get; set; }
        public double S2_Authority_Score { get; set; }
        public double S3_YearDistance_Score { get; set; }
        public double S4_PointsRequired_Score { get; set; }
        public double Final_Rareness_Score { get; set; }
    }

    public class EvaluationReasoning
    {
        public string S1_Reason { get; set; }
        public string S2_Reason { get; set; }
        public string S3_Reason { get; set; }
        public string S4_Reason { get; set; }
    }
}
