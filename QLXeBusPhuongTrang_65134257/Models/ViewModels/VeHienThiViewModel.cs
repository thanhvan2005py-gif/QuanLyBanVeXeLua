using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLXeBusPhuongTrang_65134257.Models.ViewModels
{
    public class VeHienThiViewModel
    {
        public int MaVeDaiDien { get; set; } // Lấy ID đầu tiên để làm link chi tiết/sửa/xóa
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public string TenTuyen { get; set; } // Tuyến xe (Bến đi -> Bến đến)
        public DateTime NgayDi { get; set; }
        public TimeSpan GioDi { get; set; }

        // Đây là cột quan trọng nhất: Chứa chuỗi ghế đã gộp (VD: "3C, 4C")
        public string DanhSachGhe { get; set; }

        // Hiển thị tổng tiền của tất cả các ghế
        public decimal TongTien { get; set; }

        public DateTime ThoiGianDat { get; set; }
        public string TrangThai { get; set; }
    }
}