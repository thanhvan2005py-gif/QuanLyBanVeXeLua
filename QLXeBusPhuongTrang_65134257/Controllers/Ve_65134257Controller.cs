using Newtonsoft.Json;
using QLXeBusPhuongTrang_65134257.Models;
using QLXeBusPhuongTrang_65134257.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
namespace QLXeBusPhuongTrang_65134257.Controllers
{
    public class Ve_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();


        // =========================================================
        // BƯỚC 1: XỬ LÝ ĐẶT VÉ (Nhận từ form ChonGhe bên ChuyenController)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatVe(int MaChuyen, string ListSoGhe, string TenKH, string DienThoai)
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(ListSoGhe) || string.IsNullOrEmpty(TenKH) || string.IsNullOrEmpty(DienThoai))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("ChonGhe", "Chuyen_65134257", new { MaChuyen = MaChuyen });
            }

            // 2. Tách ghế
            string[] selectedSeats = ListSoGhe.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim()).ToArray();

            var chuyen = db.Chuyens.Include(c => c.LichChay).FirstOrDefault(c => c.MaChuyen == MaChuyen);
            if (chuyen == null) return HttpNotFound();
            decimal giaVeCoDinh = chuyen.GiaVe;
            // 3. Xử lý Khách Hàng (SỬA LỖI TẠI ĐÂY)
            var khachHang = db.KhachHangs.FirstOrDefault(k => k.DienThoai == DienThoai);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (khachHang == null)
                    {
                        // NẾU LÀ KHÁCH MỚI -> PHẢI TẠO ĐỦ THÔNG TIN ĐỂ KHÔNG LỖI DB
                        khachHang = new KhachHang
                        {
                            TenKH = TenKH,
                            DienThoai = DienThoai,
                            MatKhau = DienThoai, // <--- BẮT BUỘC: Lấy SĐT làm mật khẩu mặc định
                            Email = null         
                        };
                        db.KhachHangs.Add(khachHang);
                        db.SaveChanges(); // Lưu ngay để lấy MaKH
                    }

                    // 4. Tạo vé
                    decimal tongTien = 0;
                    List<int> maVeList = new List<int>();

                    foreach (var soGhe in selectedSeats)
                    {
                        // Check ghế trùng lần cuối
                        if (db.Ves.Any(v => v.MaChuyen == MaChuyen && v.SoGhe == soGhe && v.TrangThai != "Đã hủy"))
                        {
                            transaction.Rollback();
                            TempData["ErrorMessage"] = $"Ghế {soGhe} đã bị đặt.";
                            return RedirectToAction("ChonGhe", "Chuyen_65134257", new { MaChuyen = MaChuyen });
                        }

                        decimal giaVe = giaVeCoDinh; // Hàm lấy giá của bạn
                        tongTien += giaVe;

                        var ve = new Ve
                        {
                            MaChuyen = MaChuyen,
                            MaKH = khachHang.MaKH,
                            SoGhe = soGhe,
                            Gia = giaVe,
                            ThoiGianDat = DateTime.Now,
                            TrangThai = "Chờ thanh toán"
                        };
                        db.Ves.Add(ve);
                        db.SaveChanges();
                        maVeList.Add(ve.MaVe);
                    }

                    // Trừ ghế trống
                    chuyen.SoGheTrong -= selectedSeats.Length;
                    db.Entry(chuyen).State = EntityState.Modified;
                    db.SaveChanges();

                    transaction.Commit();

                    // 5. Chuyển hướng thành công
                    TempData["MaVeList"] = maVeList;

                    // QUAN TRỌNG: Action XacNhanThanhToan nằm trong Ve_65134257Controller
                    // Nếu bạn đang ở Ve_65134257Controller thì chỉ cần tên Action
                    return RedirectToAction("XacNhanThanhToan", new { Total = tongTien });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Ghi lỗi ra để biết tại sao (ví dụ thiếu trường bắt buộc nào đó)
                    TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                    if (ex.InnerException != null) TempData["ErrorMessage"] += " - " + ex.InnerException.Message;

                    return RedirectToAction("ChonGhe", "Chuyen_65134257", new { MaChuyen = MaChuyen });
                }
            }
        }
        // =========================================================
        // BƯỚC 2: HIỂN THỊ MÀN HÌNH XÁC NHẬN THANH TOÁN (GET)
        // =========================================================
        public ActionResult XacNhanThanhToan(decimal? Total)
        {
            // 1. Kiểm tra danh sách vé từ TempData
            var maVeList = TempData["MaVeList"] as List<int>;

            if (maVeList == null || maVeList.Count == 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin vé hoặc phiên làm việc đã hết hạn.";
                return RedirectToAction("Index", "Tuyen_65134257");
            }

            // 2. Lấy danh sách TẤT CẢ các vé vừa đặt từ Database
            // (Thay vì chỉ lấy 1 vé, ta lấy hết để tính tổng tiền cho chính xác)
            var listVeVuaDat = db.Ves.Include(v => v.KhachHang)
                                     .Include(v => v.Chuyen.LichChay.Tuyen.BenXe)
                                     .Include(v => v.Chuyen.LichChay.Tuyen.BenXe1)
                                     .Where(v => maVeList.Contains(v.MaVe))
                                     .ToList();

            if (listVeVuaDat.Count == 0) return HttpNotFound();

            // 3. TÍNH TỔNG TIỀN TỪ DATABASE (An toàn hơn lấy từ URL)
            decimal tongTienThucTe = listVeVuaDat.Sum(v => v.Gia);

            // 4. Truyền dữ liệu sang View
            // Lấy vé đầu tiên làm đại diện để hiển thị thông tin tuyến/xe
            ViewBag.VeInfo = listVeVuaDat.FirstOrDefault();

            // Truyền tổng tiền đã tính toán lại
            ViewBag.TotalAmount = tongTienThucTe;

            // Chuyển danh sách ID vé thành JSON string để gửi qua Form POST kế tiếp
            ViewBag.MaVeListJson = JsonConvert.SerializeObject(maVeList);

            // Giữ lại TempData (quan trọng để F5 không bị mất dữ liệu ngay)
            TempData.Keep("MaVeList");

            return View();
        }

        // =========================================================
        // BƯỚC 3: HOÀN TẤT THANH TOÁN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HoanTatThanhToan(string MaVeListJson, decimal TotalAmount, string PhuongThuc)
        {
            List<int> maVeList = new List<int>();
            try
            {
                maVeList = JsonConvert.DeserializeObject<List<int>>(MaVeListJson);
            }
            catch
            {
                TempData["Error"] = "Lỗi dữ liệu vé (JSON không hợp lệ).";
                return RedirectToAction("Index", "Ve_65134257");
            }

            if (maVeList == null || maVeList.Count == 0) return RedirectToAction("Index", "Ve_65134257");

            // KIỂM TRA DỮ LIỆU ĐẦU VÀO TRƯỚC KHI LƯU
            // Nếu trong SQL cột PhuongThuc là nvarchar(50) mà bạn gửi chuỗi dài hơn sẽ gây lỗi
            if (PhuongThuc != null && PhuongThuc.Length > 50) PhuongThuc = PhuongThuc.Substring(0, 50);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    DateTime thoiGianChung = DateTime.Now;

                    foreach (var id in maVeList)
                    {
                        var ve = db.Ves.Find(id);
                        if (ve != null)
                        {
                            // 1. Tạo bản ghi ThanhToan
                            var thanhToan = new ThanhToan
                            {
                                MaVe = id,
                                // Nếu ve.Gia null thì lấy 0 để tránh lỗi
                                SoTien = ve.Gia,
                                PhuongThuc = PhuongThuc ?? "Chuyển khoản",
                                ThoiGianThanhToan = thoiGianChung,
                                TrangThai = "Chờ duyệt"
                            };
                            db.ThanhToans.Add(thanhToan);

                            // 2. Cập nhật vé
                            ve.TrangThai = "Chờ duyệt";
                            db.Entry(ve).State = EntityState.Modified;
                        }
                    }

                    // CỐ GẮNG LƯU VÀO DB
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["SuccessMessage"] = "Gửi yêu cầu thanh toán thành công! Vui lòng chờ Admin duyệt.";
                    return RedirectToAction("Index", "Ve_65134257");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    // Bắt lỗi Validation cụ thể của Entity Framework (ví dụ: trường bắt buộc bị null)
                    transaction.Rollback();
                    string msg = "Lỗi dữ liệu: ";
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            msg += validationError.ErrorMessage + "; ";
                        }
                    }
                    TempData["Error"] = msg;
                    return RedirectToAction("Index", "Ve_65134257");
                }
                catch (Exception ex)
                {
                    // Bắt các lỗi khác (SQL, kết nối...)
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi hệ thống: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "");
                    return RedirectToAction("Index", "Ve_65134257");
                }
            }
        }
        // GET: Ve_65134257
        public ActionResult Index(string searchString)
        {
            // 1. Kiểm tra đăng nhập (Bắt buộc phải đăng nhập mới xem được)
            if (Session["KhachHang"] == null && Session["TaiKhoan"] == null)
            {
                return RedirectToAction("DangNhap", "Account_65134257");
            }

            // 2. Lấy dữ liệu thô từ Database
            var rawData = db.Ves.Include(v => v.KhachHang)
                                .Include(v => v.Chuyen.LichChay.Tuyen.BenXe)
                                .Include(v => v.Chuyen.LichChay.Tuyen.BenXe1)
                                .AsQueryable();

            // =============================================================
            // ĐOẠN CODE QUAN TRỌNG NHẤT: PHÂN QUYỀN DỮ LIỆU
            // =============================================================

            // Nếu là KHÁCH HÀNG -> Chỉ lấy vé có MaKH trùng với Session
            if (Session["KhachHang"] != null)
            {
                var khach = (QLXeBusPhuongTrang_65134257.Models.KhachHang)Session["KhachHang"];
                rawData = rawData.Where(v => v.MaKH == khach.MaKH);
            }

            // Nếu là ADMIN (Session["TaiKhoan"]) -> Không làm gì cả (Xem tất cả)

            // =============================================================

            // 3. Tìm kiếm (Logic cũ giữ nguyên)
            if (!String.IsNullOrEmpty(searchString))
            {
                rawData = rawData.Where(s => s.KhachHang.TenKH.Contains(searchString) || s.KhachHang.DienThoai.Contains(searchString));
            }

            // 4. Chuyển sang List để xử lý GroupBy trong bộ nhớ
            var listVe = rawData.ToList();

            // 5. GỘP NHÓM DỮ LIỆU (Giữ nguyên logic cũ của bạn)
            var listHienThi = listVe
                .GroupBy(x => new {
                    x.MaChuyen,
                    x.MaKH,
                    ThoiGianGroup = x.ThoiGianDat.ToString("yyyyMMddHHmm"), 
                    x.TrangThai
                })
                .Select(g => new VeHienThiViewModel
                {
                    MaVeDaiDien = g.First().MaVe,
                    TenKH = g.First().KhachHang != null ? g.First().KhachHang.TenKH : "Khách vãng lai",
                    DienThoai = g.First().KhachHang != null ? g.First().KhachHang.DienThoai : "",

                    TenTuyen = g.First().Chuyen.LichChay.Tuyen.BenXe.TenBen + " → " + g.First().Chuyen.LichChay.Tuyen.BenXe1.TenBen,
                    NgayDi = g.First().Chuyen.Ngay,
                    GioDi = g.First().Chuyen.LichChay.GioKhoiHanh,

                    DanhSachGhe = string.Join(", ", g.Select(v => v.SoGhe).OrderBy(s => s)),

                    TongTien = g.Sum(v => v.Gia),
                    ThoiGianDat = g.First().ThoiGianDat,
                    TrangThai = g.Key.TrangThai
                })
                .OrderByDescending(x => x.ThoiGianDat)
                .ToList();

            return View(listHienThi);
        }

        // GET: Ve_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ve ve = db.Ves.Find(id);
            if (ve == null)
            {
                return HttpNotFound();
            }
            return View(ve);
        }

        // GET: Ve_65134257/Create
        public ActionResult Create()
        {
            ViewBag.MaChuyen = new SelectList(db.Chuyens, "MaChuyen", "MaChuyen");
            ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH");
            return View();
        }

        // POST: Ve_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaVe,MaChuyen,SoGhe,MaKH,Gia,ThoiGianDat,TrangThai")] Ve ve)
        {
            if (ModelState.IsValid)
            {
                db.Ves.Add(ve);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaChuyen = new SelectList(db.Chuyens, "MaChuyen", "MaChuyen", ve.MaChuyen);
            ViewBag.MaKH = new SelectList(db.KhachHangs, "MaKH", "TenKH", ve.MaKH);
            return View(ve);
        }

        // GET: Ve_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // 1. Tìm vé đại diện VÀ tải dữ liệu liên quan
            var veDaiDien = db.Ves
                              .Include(v => v.KhachHang)
                              .Include(v => v.Chuyen.LichChay.Tuyen.BenXe)
                              .Include(v => v.Chuyen.LichChay.Tuyen.BenXe1)
                              .FirstOrDefault(v => v.MaVe == id); // Dùng FirstOrDefault thay cho Find để dùng Include

            if (veDaiDien == null) return HttpNotFound();

            // 2. Tìm tất cả các vé cùng nhóm
            var thoiGianDatChuan = veDaiDien.ThoiGianDat.ToString("yyyyMMddHHmm"); 
            var cacVeCungNhom = db.Ves.AsNoTracking()
                .Where(v => v.MaChuyen == veDaiDien.MaChuyen &&
                            v.MaKH == veDaiDien.MaKH )
                .ToList()
                .Where(v => v.ThoiGianDat.ToString("yyyyMMddHHmm") == thoiGianDatChuan)
                .ToList();
            // 3. Đổ dữ liệu vào ViewModel
            var editModel = new VeHienThiViewModel
            {
                MaVeDaiDien = veDaiDien.MaVe,
                TenKH = veDaiDien.KhachHang?.TenKH ?? "Khách vãng lai", // Null check gọn hơn
                DienThoai = veDaiDien.KhachHang?.DienThoai ?? "",

                TenTuyen = veDaiDien.Chuyen.LichChay.Tuyen.BenXe.TenBen + " → " + veDaiDien.Chuyen.LichChay.Tuyen.BenXe1.TenBen,
                NgayDi = veDaiDien.Chuyen.Ngay,
                GioDi = veDaiDien.Chuyen.LichChay.GioKhoiHanh,

                // Gộp ghế
                DanhSachGhe = string.Join(", ", cacVeCungNhom.Select(v => v.SoGhe).OrderBy(s => s)),

                // TÍNH TỔNG TIỀN AN TOÀN: Xử lý NULL nếu Gia có thể null trong Model (dùng ?? 0m)
                // Nếu bạn chắc chắn Gia là decimal non-nullable, hãy dùng TongTien = cacVeCungNhom.Sum(v => v.Gia)
                TongTien = cacVeCungNhom.Sum(v => v.Gia),

                ThoiGianDat = veDaiDien.ThoiGianDat,
                TrangThai = veDaiDien.TrangThai
            };

            // 4. Tạo danh sách trạng thái
            var listTrangThai = new List<string> {"Chờ duyệt", "Đã thanh toán", "Đã hủy" };
            ViewBag.TrangThaiList = new SelectList(listTrangThai, editModel.TrangThai);

            return View(editModel);
        }

        // POST: Ve_65134257/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VeHienThiViewModel model)
        {
            // Tìm lại vé gốc để lấy thông tin truy vấn nhóm
            var veDaiDien = db.Ves.Find(model.MaVeDaiDien);

            if (veDaiDien != null)
            {
                // Lưu lại trạng thái cũ để kiểm tra xem có thay đổi không
                string trangThaiCu = veDaiDien.TrangThai;

                var thoiGianDatChuan = veDaiDien.ThoiGianDat.ToString("yyyyMMddHHmm");

                // Tìm lại cả nhóm vé
                var cacVeCungNhom = db.Ves.Where(v => v.MaChuyen == veDaiDien.MaChuyen &&
                                                      v.MaKH == veDaiDien.MaKH)
                    .ToList() // KÉO DỮ LIỆU VỀ
                    .Where(v => v.ThoiGianDat.ToString("yyyyMMddHHmm") == thoiGianDatChuan)
                    .ToList();

                // Cập nhật trạng thái cho TẤT CẢ các vé trong nhóm
                foreach (var item in cacVeCungNhom)
                {
                    item.TrangThai = model.TrangThai; // Lấy trạng thái từ dropdown
                    db.Entry(item).State = EntityState.Modified;
                    // Tìm bản ghi thanh toán tương ứng với vé này (nếu có)
                    var thanhToan = db.ThanhToans.FirstOrDefault(tt => tt.MaVe == item.MaVe);

                    if (thanhToan != null)
                    {
                        if (model.TrangThai == "Đã thanh toán")
                        {
                            // Nếu Admin duyệt Vé -> Thanh toán thành công
                            thanhToan.TrangThai = "Hoàn Tất";
                            thanhToan.ThoiGianThanhToan = DateTime.Now; // Cập nhật giờ xác nhận thực tế
                            db.Entry(thanhToan).State = EntityState.Modified;
                        }
                        else if (model.TrangThai == "Đã hủy")
                        {
                            // Nếu Admin hủy Vé -> Thanh toán thất bại/đã hủy
                            thanhToan.TrangThai = "Đã hủy";
                            db.Entry(thanhToan).State = EntityState.Modified;
                        }
                    }
                }

                db.SaveChanges(); // LƯU VÀO DATABASE TRƯỚC

                // --- BẮT ĐẦU LOGIC GỬI MAIL ---
                // Chỉ gửi mail khi:
                // 1. Admin chọn trạng thái mới là "Đã thanh toán"
                // 2. Trạng thái cũ CHƯA PHẢI là "Đã thanh toán" (tránh gửi lặp lại khi sửa thông tin khác)
                if (model.TrangThai == "Đã thanh toán" && trangThaiCu != "Đã thanh toán")
                {
                    // Vì db.Find() chưa load thông tin bảng liên kết, ta cần load thủ công để lấy Email, Tên Tuyến
                    db.Entry(veDaiDien).Reference(v => v.KhachHang).Load();
                    db.Entry(veDaiDien).Reference(v => v.Chuyen).Load();
                    db.Entry(veDaiDien.Chuyen).Reference(c => c.LichChay).Load();
                    db.Entry(veDaiDien.Chuyen.LichChay).Reference(l => l.Tuyen).Load();
                    // Load Bến xe để hiển thị tên tuyến đẹp hơn
                    db.Entry(veDaiDien.Chuyen.LichChay.Tuyen).Reference(t => t.BenXe).Load();
                    db.Entry(veDaiDien.Chuyen.LichChay.Tuyen).Reference(t => t.BenXe1).Load();

                    // Tính toán thông tin gửi mail
                    string tenKhach = veDaiDien.KhachHang?.TenKH ?? "Khách hàng";
                    string emailKhach = veDaiDien.KhachHang?.Email;
                    decimal tongTien = cacVeCungNhom.Sum(v => v.Gia);
                    string dsGhe = string.Join(", ", cacVeCungNhom.Select(v => v.SoGhe).OrderBy(s => s));

                    // Lấy tên tuyến (Điểm đi -> Điểm đến)
                    string tenTuyen = "Tuyến xe";
                    if (veDaiDien.Chuyen?.LichChay?.Tuyen != null)
                    {
                        var tuyen = veDaiDien.Chuyen.LichChay.Tuyen;
                        tenTuyen = $"{tuyen.BenXe.TenBen} đi {tuyen.BenXe1.TenBen}";
                    }

                    // Gọi hàm gửi mail
                    GuiEmailXacNhan(
                        emailKhach,
                        tenKhach,
                        veDaiDien.MaVe.ToString(),
                        tongTien,
                        dsGhe,
                        tenTuyen,
                        veDaiDien.Chuyen.Ngay,
                        veDaiDien.Chuyen.LichChay.GioKhoiHanh
                    );

                    TempData["SuccessMessage"] = $"Đã xác nhận thanh toán và gửi Email cho khách {tenKhach}!";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng thành công!";
                }
                // ------------------------------

                return RedirectToAction("Index");
            }

            TempData["Error"] = "Không tìm thấy thông tin vé để cập nhật.";
            return RedirectToAction("Index");
        }
        // GET: Ve_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ve ve = db.Ves.Find(id);
            if (ve == null)
            {
                return HttpNotFound();
            }
            return View(ve);
        }

        // POST: Ve_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // 1. Tìm vé đại diện (Vé mà người dùng bấm nút xóa)
            var veDaiDien = db.Ves.Find(id);

            if (veDaiDien == null)
            {
                return HttpNotFound();
            }

            //Tìm vé cùng nhóm
            var timeTarget = veDaiDien.ThoiGianDat; // Lấy mốc thời gian của vé đại diện

            var danhSachVeCanXoa = db.Ves.Where(v =>
                v.MaChuyen == veDaiDien.MaChuyen &&          // Cùng chuyến xe
                v.MaKH == veDaiDien.MaKH &&                  // Cùng khách hàng
                v.TrangThai == veDaiDien.TrangThai &&        // Cùng trạng thái

                // So sánh thời gian (Khớp đến từng Phút như logic Index)
                v.ThoiGianDat.Year == timeTarget.Year &&
                v.ThoiGianDat.Month == timeTarget.Month &&
                v.ThoiGianDat.Day == timeTarget.Day &&
                v.ThoiGianDat.Hour == timeTarget.Hour &&
                v.ThoiGianDat.Minute == timeTarget.Minute
            ).ToList();

            // 3. Thực hiện xóa danh sách
            if (danhSachVeCanXoa.Any())
            {
                db.Ves.RemoveRange(danhSachVeCanXoa); // Xóa một lần hết cả danh sách
                db.SaveChanges();
            }

            TempData["Success"] = "Đã hủy vé thành công!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public void GuiEmailXacNhan(string emailKhach, string tenKhach, string maVe, decimal tongTien, string dsGhe, string tenTuyen, DateTime ngayDi, TimeSpan gioDi)
        {
            if (string.IsNullOrEmpty(emailKhach)) return;

            try
            {
                var fromAddress = new MailAddress("thanhvan2005py@gmail.com", "Nhà xe Phương Trang");
                const string fromPassword = "vqtdghtnnnwjvzic"; // Nhớ thay mật khẩu ứng dụng
                var toAddress = new MailAddress(emailKhach, tenKhach);

                string subject = "XÁC NHẬN THANH TOÁN VÉ XE THÀNH CÔNG";

                // Format ngày giờ đẹp hơn
                string thoiGianKhoiHanh = $"{gioDi.ToString(@"hh\:mm")} ngày {ngayDi.ToString("dd/MM/yyyy")}";

                string body = $@"
            <div style='font-family:Arial, sans-serif; padding:20px; border:1px solid #ddd; background-color:#fff;'>
                <h2 style='color:#ff6b00; border-bottom: 2px solid #ff6b00; padding-bottom: 10px;'>Thanh toán thành công!</h2>
                <p>Xin chào <strong>{tenKhach}</strong>,</p>
                <p>Đơn hàng vé xe của bạn đã được Admin xác nhận thanh toán thành công.</p>
                <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <ul style='list-style: none; padding: 0;'>
                        <li style='margin-bottom: 10px;'><strong> Mã vé:</strong> {maVe}</li>
                        <li style='margin-bottom: 10px;'><strong> Tuyến xe:</strong> {tenTuyen}</li>
                        <li style='margin-bottom: 10px;'><strong> Thời gian:</strong> {thoiGianKhoiHanh}</li>
                        <li style='margin-bottom: 10px;'><strong> Số ghế:</strong> {dsGhe}</li>
                        <li style='margin-bottom: 10px;'><strong> Tổng tiền:</strong> <span style='color:red; font-weight:bold; font-size: 1.2em;'>{tongTien.ToString("N0")} đ</span></li>
                    </ul>
                </div>
                <p>Quý khách vui lòng có mặt tại bến xe trước 30 phút để lên xe.</p>
                <p><i>Cảm ơn quý khách đã sử dụng dịch vụ của Phương Trang (FUTA Bus Lines)!</i></p>
            </div>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Gửi mail thất bại: " + ex.Message);
            }
        }
    }
}
