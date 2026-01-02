using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLXeBusPhuongTrang_65134257.Models.ViewModels
{
    public class ThanhToanHienThiViewModel
    {
        public int MaTT { get; set; } // Mã đại diện để nút Sửa/Xóa hoạt động
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; }
        public DateTime? ThoiGianThanhToan { get; set; }
        public string TrangThai { get; set; }
        public string DanhSachGhe { get; set; } // Chuỗi ghế đã gộp (Ví dụ: "1A, 1B")
        public string LoaiXe { get; set; }
        public string BienSo { get; set; }
        public int SoCho { get; set; }
    }
}