using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestNLP
{
    public class Program
    {
        public static void Main()
        {
            var tester = new NLPLogicTester();
            
            Console.WriteLine("=== BẮT ĐẦU KIỂM THỬ LOGIC RARENESS SCORE ===");
            
            // Kịch bản 1: Gian lận năm xuất bản
            tester.RunTest("GIAN LẬN NĂM", "Tài liệu cũ", "Tài liệu", "Tự do", 1985, 10, "Đây là bản thảo năm 2026 mới nhất");
            
            // Kịch bản 2: Dữ liệu rác và điểm âm
            tester.RunTest("DỮ LIỆU RÁC & ĐIỂM ÂM", "qwertyuiopasdf", "Tham khảo", "1234567890", 2026, -50, "Nội dung bình thường");
            
            // Kịch bản 3: Tài liệu Giáo trình chính thống (Hợp lệ)
            tester.RunTest("GIÁO TRÌNH CHUẨN", "Giáo trình C# nâng cao", "Giáo trình", "NXB Giáo dục", 2000, 50, "Kiến thức chuyên sâu về lập trình");

            Console.WriteLine("=== KẾT THÚC KIỂM THỬ ===");
        }
    }

    public class NLPLogicTester
    {
        public void RunTest(string caseName, string tieuDe, string loaiTaiLieu, string nxb, int? namXb, int diemYeuCau, string rawText)
        {
            Console.WriteLine($"\n[TEST CASE: {caseName}]");
            Console.WriteLine($"- Input: {tieuDe}, Năm: {namXb}, Điểm: {diemYeuCau}, NXB: {nxb}");
            
            // Giả lập logic trong NLPService.CalculateLocalHeuristic
            string securityNote = "Normal";
            
            // 1. Check junk
            if (string.IsNullOrWhiteSpace(tieuDe) || tieuDe.Length < 3 || Regex.IsMatch(tieuDe, @"^[a-zA-Z0-9]{10,}$"))
                securityNote = "Phát hiện tiêu đề có dấu hiệu dữ liệu rác.";

            // 2. Yếu tố 1: Nội dung
            double s1 = 3.0;
            var keywords = new[] { "Đồ án phần mềm", "Báo cáo kết quả", "Usecase", "Database", "Giáo trình", "Đồ án tốt nghiệp", "Luận văn" };
            if (loaiTaiLieu.Contains("Giáo trình") || keywords.Any(k => (tieuDe + rawText).Contains(k))) s1 = 10.0;

            // 3. Yếu tố 2: Tác quyền
            double s2 = 3.0;
            if (nxb.Contains("NXB Giáo dục") || nxb.Contains("NXB Đại học Quốc gia")) s2 = 10.0;
            else if (nxb.Contains("Giảng viên") || nxb.Contains("SPKT")) s2 = 7.0;

            // 4. Yếu tố 3: Năm (Chống gian lận)
            int actualNamXb = namXb ?? 2026;
            var yearMatches = Regex.Matches(rawText, @"\b(19|20)\d{2}\b");
            int maxFoundYear = 0;
            foreach (Match m in yearMatches) {
                int y = int.Parse(m.Value);
                if (y > maxFoundYear && y <= 2026) maxFoundYear = y;
            }

            bool isFraud = false;
            if (namXb.HasValue && namXb.Value < 2000 && maxFoundYear >= 2020) {
                actualNamXb = maxFoundYear;
                isFraud = true;
                securityNote = $"PHÁT HIỆN GIAN LẬN NĂM. Nhập: {namXb}, Thực tế: {maxFoundYear}";
            }

            double s3 = 2.0;
            if (!isFraud) {
                int diff = 2026 - actualNamXb;
                if (diff > 20) s3 = 10.0;
                else if (diff >= 5) s3 = 6.0;
            }

            // 5. Yếu tố 4: Điểm
            double s4 = 2.0;
            int effectiveDiem = diemYeuCau < 0 ? 0 : diemYeuCau;
            if (effectiveDiem >= 50) s4 = 10.0;
            else if (effectiveDiem >= 20) s4 = 7.0;

            double final = (s1 * 0.35) + (s2 * 0.20) + (s3 * 0.20) + (s4 * 0.25);
            
            Console.WriteLine($"> Security Note: {securityNote}");
            Console.WriteLine($"> Scores: S1={s1}, S2={s2}, S3={s3}, S4={s4}");
            Console.WriteLine($"> FINAL RS: {final:F2} -> " + (final >= 8.5 ? "Rất Quý" : final >= 6.0 ? "Quý / Hiếm" : "Thông Thường"));
        }
    }
}
