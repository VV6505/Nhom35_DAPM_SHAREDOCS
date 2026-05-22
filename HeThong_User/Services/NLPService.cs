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

        public async Task<RarenessEvaluation> EvaluateRarenessAsync(string tieuDe, string loaiFile, string loaiTaiLieu, string nxb, int? namXb, int diemYeuCau, string rawText)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // Fallback to local heuristic if no API key is provided
                return CalculateLocalHeuristic(tieuDe, loaiTaiLieu, nxb, namXb, diemYeuCau, rawText);
            }

            var prompt = $@"
[ROLE & CONTEXT]
Bạn là một công cụ AI chuyên trách (NLP Engine) tích hợp tại phân hệ Backend Admin của dự án ""Hệ thống chia sẻ tài liệu học tập trường Đại học Sư phạm Kỹ thuật Đà Nẵng"". Nhiệm vụ của bạn là tiếp nhận siêu dữ liệu từ Form nhập liệu và đoạn văn bản thô (Raw Text) trích xuất từ file tài liệu, sau đó tính toán Điểm Độ Quý (Rareness Score) theo thuật toán Heuristic trọng số.
[INPUT METADATA FROM INTERFACE]
- Tiêu đề tài liệu: {tieuDe}
- Định dạng tệp: {loaiFile}
- Loại tài liệu: {loaiTaiLieu}
- Nhà xuất bản: {nxb}
- Năm xuất bản: {namXb}
- Điểm yêu cầu: {diemYeuCau}
[RULES FOR HEURISTIC SCORING (SCORING MATRIX)]
1. Yếu tố 1: Tính chất nội dung và Thể loại (Trọng số: 0.35)
   - Nếu Loại tài liệu là ""Giáo trình"", ""Bài giảng độc quyền"", hoặc văn bản chứa từ khóa học thuật trọng tâm như ""Đồ án phần mềm"", ""Đồ án tốt nghiệp"", ""Luận văn"", ""Usecase"", ""Database"": 10 điểm.
   - Nếu Loại tài liệu là ""Slide bài giảng"", ""Đề cương chi tiết"", ""Tài liệu hướng dẫn thực hành"": 6 điểm.
   - Nếu là tài liệu tham khảo ngắn, tiểu luận, bài tập nộp tuần thông thường: 3 điểm.
2. Yếu tố 2: Đơn vị cấp phép và Tác quyền (Trọng số: 0.20)
   - Nếu Nhà xuất bản thuộc các đơn vị chính thống (ví dụ: ""NXB Giáo dục"", ""NXB Đại học Quốc gia"", ""NXB Khoa học Kỹ thuật""): 10 điểm.
   - Nếu Nhà xuất bản ghi nhận ""Giảng viên biên soạn"" hoặc ""Nội bộ trường ĐH SPKT"": 7 điểm.
   - Nếu ghi nhận ""Tác giả tự do"" hoặc bỏ trống thông tin: 3 điểm.
3. Yếu tố 3: Khoảng cách năm phát hành (Trọng số: 0.20)
   - Lấy năm hiện tại của hệ thống mặc định là năm 2026 trừ đi giá trị tại trường [Năm xuất bản].
   - Nếu khoảng cách thời gian trên 20 năm (Tài liệu cổ, mang tính lưu trữ lịch sử): 10 điểm.
   - Nếu khoảng cách thời gian từ 5 đến 20 năm: 6 điểm.
   - Nếu khoảng cách dưới 5 năm hoặc trùng năm hiện tại (2026): 3 điểm.
4. Yếu tố 4: Giá trị trao đổi hệ thống - Điểm yêu cầu (Trọng số: 0.25)
   - Đọc giá trị từ trường [Điểm yêu cầu] do người dùng thiết lập trên giao diện:
   - Nếu Điểm yêu cầu >= 50 điểm: 10 điểm.
   - Nếu Điểm yêu cầu từ 20 đến dưới 50 điểm: 7 điểm.
   - Nếu Điểm yêu cầu từ 5 đến dưới 20 điểm: 5 điểm.
   - Nếu Điểm yêu cầu = 0 (Tài liệu miễn phí): 2 điểm.
[STRICT OUTPUT FORMAT CONSTRAINTS]
- CHỈ trả về duy nhất một chuỗi định dạng JSON hợp lệ.
- KHÔNG bao gồm các ký tự bọc khối dạng ```json ... ```.
- KHÔNG kèm theo lời chào, lời giải thích hoặc bất kỳ chữ thừa nào ngoài khối JSON.
{{
  ""Evaluation"": {{
    ""S1_ContentType_Score"": 0.0,
    ""S2_Authority_Score"": 0.0,
    ""S3_YearDistance_Score"": 0.0,
    ""S4_PointsRequired_Score"": 0.0,
    ""Final_Rareness_Score"": 0.0
  }},
  ""Classification"": ""Gán nhãn chuỗi"",
  ""Reasoning"": {{
    ""S1_Reason"": ""Giải thích lý do cho S1 (vd: Tại vì tài liệu chứa từ khóa chuyên sâu như... nên được 10 điểm)"",
    ""S2_Reason"": ""Giải thích lý do cho S2"",
    ""S3_Reason"": ""Giải thích lý do cho S3"",
    ""S4_Reason"": ""Giải thích lý do cho S4""
  }}
}}
[RAW TEXT INPUT]
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
            catch (Exception)
            {
                // Fallback to local
            }

            return CalculateLocalHeuristic(tieuDe, loaiTaiLieu, nxb, namXb, diemYeuCau, rawText);
        }

        private RarenessEvaluation CalculateLocalHeuristic(string tieuDe, string loaiTaiLieu, string nxb, int? namXb, int diemYeuCau, string rawText)
        {
            double s1 = 3.0;
            string r1 = "Vì tài liệu chưa có nhiều từ khóa chuyên sâu nên đạt mức điểm cơ bản.";
            var keywords = new[] { "Đồ án", "Luận văn", "Usecase", "Database", "Giáo trình", "Bài giảng độc quyền" };
            if (loaiTaiLieu.Contains("Giáo trình") || keywords.Any(k => (tieuDe + rawText).Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                s1 = 10.0;
                r1 = "Vì tài liệu thuộc loại Giáo trình hoặc chứa nội dung chuyên môn cao (Đồ án, Luận văn) nên được hệ thống đánh giá điểm tối đa.";
            }
            else if (loaiTaiLieu.Contains("Slide") || loaiTaiLieu.Contains("Đề cương") || loaiTaiLieu.Contains("Thực hành"))
            {
                s1 = 6.0;
                r1 = "Vì tài liệu là Slide bài giảng hoặc Đề cương hướng dẫn nên được mức điểm khá.";
            }

            double s2 = 3.0;
            string r2 = "Vì thông tin tác giả chưa rõ ràng nên đạt mức điểm tham khảo.";
            if (string.IsNullOrEmpty(nxb)) { s2 = 2.0; r2 = "Vì thiếu thông tin Nhà xuất bản nên hệ thống chỉ áp dụng mức điểm sàn."; }
            else if (nxb.Contains("NXB") || nxb.Contains("Đại học Quốc gia")) { s2 = 10.0; r2 = "Vì tài liệu được xuất bản bởi các đơn vị chính thống uy tín nên được điểm tối đa về tác quyền."; }
            else if (nxb.Contains("Giảng viên") || nxb.Contains("SPKT")) { s2 = 7.0; r2 = "Vì tài liệu do Giảng viên biên soạn hoặc lưu hành nội bộ nên có độ tin cậy cao."; }

            double s3 = 3.0;
            string r3 = "Vì tài liệu mới phát hành gần đây nên giá trị lưu trữ lịch sử ở mức trung bình.";
            if (!namXb.HasValue) { s3 = 2.0; r3 = "Vì không rõ năm phát hành nên hệ thống áp dụng mức điểm thấp nhất cho yếu tố thời gian."; }
            else
            {
                int diff = 2026 - namXb.Value;
                if (diff > 20) { s3 = 10.0; r3 = $"Vì tài liệu đã phát hành cách đây {diff} năm (trên 20 năm) nên mang giá trị lưu trữ lịch sử rất cao."; }
                else if (diff >= 5) { s3 = 6.0; r3 = $"Vì tài liệu có tuổi đời {diff} năm nên có giá trị tham khảo tốt qua nhiều thời kỳ."; }
            }

            double s4 = 2.0;
            string r4 = "Vì tài liệu được chia sẻ miễn phí nên điểm giá trị hệ thống ở mức cơ bản.";
            if (diemYeuCau >= 50) { s4 = 10.0; r4 = "Vì mức điểm yêu cầu tải rất cao (trên 50 điểm) nên hệ thống đánh giá đây là tài liệu cực kỳ giá trị."; }
            else if (diemYeuCau >= 20) { s4 = 7.0; r4 = "Vì mức điểm yêu cầu tải ở mức cao (20-50 điểm) nên đạt điểm cộng giá trị."; }
            else if (diemYeuCau >= 5) { s4 = 5.0; r4 = "Vì tài liệu có yêu cầu điểm tích lũy khi tải nên có giá trị trao đổi khá."; }

            double final = (s1 * 0.35) + (s2 * 0.20) + (s3 * 0.20) + (s4 * 0.25);

            string classification = "Thông Thường";
            if (final >= 8.5) classification = "Rất Quý";
            else if (final >= 6.0) classification = "Quý / Hiếm";

            return new RarenessEvaluation
            {
                Evaluation = new EvaluationScores
                {
                    S1_ContentType_Score = s1,
                    S2_Authority_Score = s2,
                    S3_YearDistance_Score = s3,
                    S4_PointsRequired_Score = s4,
                    Final_Rareness_Score = final
                },
                Classification = classification,
                Reasoning = new EvaluationReasoning
                {
                    S1_Reason = r1,
                    S2_Reason = r2,
                    S3_Reason = r3,
                    S4_Reason = r4
                }
            };
        }
    }

    public class RarenessEvaluation
    {
        public EvaluationScores Evaluation { get; set; }
        public string Classification { get; set; }
        public EvaluationReasoning Reasoning { get; set; }
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
