using QLXeBusPhuongTrang_65134257.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace QLXeBusPhuongTrang_65134257.Controllers
{
    public class Account_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();
        // 1. ĐĂNG KÝ (CHỈ DÀNH CHO KHÁCH HÀNG)
        // ==========================================================

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(KhachHang model)
        {
            if (ModelState.IsValid)
            {
                // KIỂM TRA ĐỊNH DẠNG SỐ ĐIỆN THOẠI ---
                // Regex: Bắt đầu bằng số 0, theo sau là 9 chữ số bất kỳ
                if (!Regex.IsMatch(model.DienThoai, @"^0\d{9}$"))
                {
                    ModelState.AddModelError("DienThoai", "Số điện thoại không hợp lệ (Phải bắt đầu bằng số 0 và đủ 10 số).");
                    return View(model);
                }
                // Kiểm tra số điện thoại đã tồn tại chưa
                var checkSdt = db.KhachHangs.FirstOrDefault(k => k.DienThoai == model.DienThoai);
                if (checkSdt != null)
                {
                    ModelState.AddModelError("DienThoai", "Số điện thoại này đã được đăng ký.");
                    return View(model);
                }

                // Kiểm tra Email trùng (nếu có nhập)
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var checkEmail = db.KhachHangs.FirstOrDefault(k => k.Email == model.Email);
                    if (checkEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                        return View(model);
                    }
                }

                // Thêm khách hàng mới
                // model.MatKhau = MaHoa(model.MatKhau); // TODO: Nên mã hóa mật khẩu tại đây
                db.KhachHangs.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            return View(model);
        }

        //ĐĂNG KÝ CHO ADMIN
        [HttpGet]
        public ActionResult DangKyAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKyAdmin(TaiKhoan model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra tên đăng nhập đã tồn tại chưa
                var checkUser = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == model.TenDangNhap);
                if (checkUser != null)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác.");
                    return View(model);
                }

                // 2. Kiểm tra định dạng số điện thoại (tương tự khách hàng)
                if (!string.IsNullOrEmpty(model.DienThoai) && !Regex.IsMatch(model.DienThoai, @"^0\d{9}$"))
                {
                    ModelState.AddModelError("DienThoai", "Số điện thoại không hợp lệ (Phải bắt đầu bằng số 0 và đủ 10 số).");
                    return View(model);
                }

                // 3. Thiết lập các giá trị mặc định cho Admin mới
                model.NgayTao = DateTime.Now;   // Ngày tạo là hiện tại
                model.TrangThai = true;         // Mặc định là Active (Hoạt động) để đăng nhập được ngay

                // model.MatKhau = MaHoa(model.MatKhau); // TODO: Mã hóa mật khẩu nếu cần

                // 4. Lưu vào Database
                db.TaiKhoans.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Tạo tài khoản quản trị thành công! Mời đăng nhập.";
                return RedirectToAction("DangNhap");
            }

            return View(model);
        }
        // ==========================================================
        // 2. ĐĂNG NHẬP (DÙNG CHUNG CHO CẢ KHÁCH VÀ ADMIN)
        // ==========================================================

        [HttpGet]
        public ActionResult DangNhap(string returnUrl)
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ ngay
            if (Session["KhachHang"] != null || Session["TaiKhoan"] != null)
            {
                return RedirectToAction("Index", "Tuyen_65134257");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string ThongTinDangNhap, string MatKhau,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // --- TRƯỜNG HỢP 1: TÌM TRONG BẢNG KHÁCH HÀNG (Ưu tiên SĐT) ---
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.DienThoai == ThongTinDangNhap && k.MatKhau == MatKhau);

                if (khachHang != null)
                {
                    // Logic đăng nhập thành công cho Khách
                    
                    Session["KhachHang"] = khachHang; // Lưu session mới
                    Session["TaiKhoan"] = null;
                    return RedirectToAction("Index", "Tuyen_65134257");
                }

                // --- TRƯỜNG HỢP 2: TÌM TRONG BẢNG ADMIN (Username) ---
                var admin = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == ThongTinDangNhap && t.MatKhau == MatKhau && t.TrangThai == true);

                if (admin != null)
                {
                    // Logic đăng nhập thành công cho Admin
                    Session["TaiKhoan"] = admin; // Lưu session mới
                    Session["KhachHang"] = null;
                    return RedirectToAction("Index", "Tuyen_65134257");
                }

                // --- TRƯỜNG HỢP 3: KHÔNG TÌM THẤY ---
                ViewBag.LoiDangNhap = "Thông tin đăng nhập hoặc mật khẩu không chính xác.";
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ==========================================================
        // 3. ĐĂNG XUẤT (UNIFIED LOGOUT)
        // ==========================================================

        public ActionResult DangXuat()
        {
            // Xóa cache trình duyệt để người dùng không bấm Back được
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            // Hủy toàn bộ Session (Cả Khách lẫn Admin bay màu hết)
            Session.Abandon();

            // Về trang chủ
            return RedirectToAction("Index", "Tuyen_65134257");
        }

        // Dọn dẹp kết nối
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
