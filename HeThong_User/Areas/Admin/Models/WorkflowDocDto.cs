using System;

namespace HeThong_User.Areas.Admin.Models
{
    public class WorkflowDocDto
    {
        public string maTaiLieu { get; set; } = "";
        public string tieuDe { get; set; } = "";
        public string loaiFile { get; set; } = "";
        public string trangThaiDuyet { get; set; } = "";
        public string? lyDoTuChoi { get; set; }
        public DateTime? ngayDang { get; set; }
        public string urlXemTruoc { get; set; } = "";
    }
}
