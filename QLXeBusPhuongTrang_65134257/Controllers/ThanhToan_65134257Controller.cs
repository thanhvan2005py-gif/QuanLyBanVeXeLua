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

namespace QLXeBusPhuongTrang_65134257.Controllers
{
    public class ThanhToan_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: ThanhToan_65134257
        public ActionResult Index()
        {
            // 1. Lấy dữ liệu thô từ DB bằng cách Include các Navigation Property cần thiết
            // Tải theo chuỗi: ThanhToan -> Ve -> Chuyen -> LichChay -> Xe
            var rawData = db.ThanhToans
                // Bắt đầu từ ThanhToan, Include Ve
                .Include(t => t.Ve)
                // Từ Ve, Include Chuyen (Giả định: Ve có Navigation Property tên là Chuyen)
                .Include(t => t.Ve.Chuyen)
                // Từ Chuyen, Include LichChay (Giả định: Chuyen có Navigation Property tên là LichChay)
                .Include(t => t.Ve.Chuyen.LichChay)
                // Từ LichChay, Include Xe (Giả định: LichChay có Navigation Property tên là Xe)
                .Include(t => t.Ve.Chuyen.LichChay.Xe)
                .ToList(); // Kéo dữ liệu về bộ nhớ

            // 2. Xử lý Gộp nhóm và tạo ViewModel
            var listHienThi = rawData
                .GroupBy(x => new
                {
                    // Gộp theo thời gian, phương thức, trạng thái
                    TimeGroup = x.ThoiGianThanhToan.ToString("yyyyMMddHHmmss"),
                    x.PhuongThuc,
                    x.TrangThai

                    // 🌟 NẾU CẦN: Gộp thêm theo Loại Xe để mỗi loại xe là một nhóm riêng biệt
                    // LoaiXe = x.Ve != null && x.Ve.Chuyen.LichChay.Xe != null ? x.Ve.Chuyen.LichChay.Xe.LoaiXe : "N/A"
                })
                .Select(g => new ThanhToanHienThiViewModel
                {
                    MaTT = g.First().MaTT,
                    SoTien = g.Sum(v => v.SoTien),
                    PhuongThuc = g.Key.PhuongThuc,
                    ThoiGianThanhToan = g.First().ThoiGianThanhToan,
                    TrangThai = g.Key.TrangThai,

                    // 🌟 LẤY LOẠI XE
                    // Lấy thông tin Loại Xe từ dòng đầu tiên trong nhóm
                    LoaiXe = g.First().Ve != null
                             && g.First().Ve.Chuyen != null
                             && g.First().Ve.Chuyen.LichChay != null
                             && g.First().Ve.Chuyen.LichChay.Xe != null ?

                             // Đảm bảo tên thuộc tính trong Model Xe là chính xác (ví dụ: TenLoaiXe/LoaiXe)
                             g.First().Ve.Chuyen.LichChay.Xe.LoaiXe : "N/A",

                    // Gộp số ghế từ bảng Ve
                    // Bạn đã có thuộc tính SoGhe trong Model Ve (x.Ve.SoGhe)
                    DanhSachGhe = string.Join(", ", g.Select(x => x.Ve != null ? x.Ve.SoGhe : "N/A").OrderBy(s => s))
                })
                .OrderByDescending(x => x.ThoiGianThanhToan)
                .ToList();

            return View(listHienThi);
        }

        // GET: ThanhToan_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 1. Tìm bản ghi gốc và tải toàn bộ chuỗi quan hệ cần thiết
            var thanhToanGoc = db.ThanhToans
                .Include(t => t.Ve)
                .Include(t => t.Ve.Chuyen.LichChay.Xe) // Đã Include Xe
                .FirstOrDefault(t => t.MaTT == id);

            if (thanhToanGoc == null)
            {
                return HttpNotFound();
            }

            // Lấy thông tin nhóm từ bản ghi gốc:
            var phuongThucKey = thanhToanGoc.PhuongThuc;
            var trangThaiKey = thanhToanGoc.TrangThai;
            var thoiGianGoc = thanhToanGoc.ThoiGianThanhToan;

            // 2. Truy vấn và lọc tất cả các bản ghi thuộc cùng một nhóm giao dịch
            var rawDataCuaNhom = db.ThanhToans
                .Include(t => t.Ve.Chuyen.LichChay.Xe) // Đã Include Xe
                .Where(t => t.PhuongThuc == phuongThucKey &&
                            t.TrangThai == trangThaiKey &&
                            t.ThoiGianThanhToan.Year == thoiGianGoc.Year &&
                            t.ThoiGianThanhToan.Month == thoiGianGoc.Month &&
                            t.ThoiGianThanhToan.Day == thoiGianGoc.Day &&
                            t.ThoiGianThanhToan.Hour == thoiGianGoc.Hour &&
                            t.ThoiGianThanhToan.Minute == thoiGianGoc.Minute &&
                            t.ThoiGianThanhToan.Second == thoiGianGoc.Second)
                .ToList();

            // 3. Gộp nhóm và chuyển đổi thành ViewModel
            var chiTietNhom = rawDataCuaNhom
                .GroupBy(x => new
                {
                    TimeGroup = x.ThoiGianThanhToan.ToString("yyyyMMddHHmmss"),
                    x.PhuongThuc,
                    x.TrangThai
                })
                .Select(g =>
                {
                    var firstItem = g.First();
                    // Lấy đối tượng Xe an toàn (sử dụng toán tử Elvis hoặc Null-conditional)
                    var xe = firstItem.Ve?.Chuyen?.LichChay?.Xe;

                    return new ThanhToanHienThiViewModel
                    {
                        MaTT = firstItem.MaTT,
                        SoTien = g.Sum(v => v.SoTien),
                        PhuongThuc = g.Key.PhuongThuc,
                        ThoiGianThanhToan = firstItem.ThoiGianThanhToan,
                        TrangThai = g.Key.TrangThai,

                        // 🌟 Gán Loại Xe (Đã có)
                        LoaiXe = xe != null ? xe.LoaiXe : "N/A",

                        // 🌟 GÁN THÊM BIỂN SỐ (NEW)
                        BienSo = xe != null ? xe.BienSo : "N/A",

                        // 🌟 GÁN THÊM SỐ CHỖ (NEW)
                        // Giả định SoCho là int, nếu null thì gán 0
                        SoCho = xe != null ? xe.SoCho : 0,

                        // Gộp số ghế (Logic đã có)
                        DanhSachGhe = string.Join(", ", g.Select(x => x.Ve != null ? x.Ve.SoGhe : "N/A").OrderBy(s => s))
                    };
                })
                .FirstOrDefault();

            if (chiTietNhom == null)
            {
                return HttpNotFound();
            }

            return View(chiTietNhom);
        }

        // GET: ThanhToan_65134257/Create
        public ActionResult Create()
        {
            ViewBag.MaVe = new SelectList(db.Ves, "MaVe", "SoGhe");
            return View();
        }

        // POST: ThanhToan_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaTT,MaVe,SoTien,PhuongThuc,ThoiGianThanhToan,TrangThai")] ThanhToan thanhToan)
        {
            if (ModelState.IsValid)
            {
                db.ThanhToans.Add(thanhToan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaVe = new SelectList(db.Ves, "MaVe", "SoGhe", thanhToan.MaVe);
            return View(thanhToan);
        }

        // GET: ThanhToan_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThanhToan thanhToan = db.ThanhToans.Find(id);
            if (thanhToan == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaVe = new SelectList(db.Ves, "MaVe", "SoGhe", thanhToan.MaVe);
            return View(thanhToan);
        }

        // POST: ThanhToan_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaTT,MaVe,SoTien,PhuongThuc,ThoiGianThanhToan,TrangThai")] ThanhToan thanhToan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(thanhToan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaVe = new SelectList(db.Ves, "MaVe", "SoGhe", thanhToan.MaVe);
            return View(thanhToan);
        }

        // GET: ThanhToan_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThanhToan thanhToan = db.ThanhToans.Find(id);
            if (thanhToan == null)
            {
                return HttpNotFound();
            }
            return View(thanhToan);
        }

        // POST: ThanhToan_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Dùng Transaction để đảm bảo an toàn: Xóa sạch hoặc không xóa gì cả
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tìm bản ghi đại diện (ID mà người dùng bấm nút Xóa)
                    var ttDaiDien = db.ThanhToans.Find(id);

                    if (ttDaiDien == null)
                    {
                        return HttpNotFound();
                    }

                    // 2. Lấy các Key để tìm nhóm (Logic phải KHỚP với hàm Index)
                    var timeKey = ttDaiDien.ThoiGianThanhToan;
                    var phuongThucKey = ttDaiDien.PhuongThuc;
                    var trangThaiKey = ttDaiDien.TrangThai;

                    // 3. Truy vấn lại toàn bộ nhóm thanh toán đó
                    // Quan trọng: Phải .Include(t => t.Ve) để lấy được Vé ra xóa
                    var nhomThanhToanCanXoa = db.ThanhToans
                        .Include(t => t.Ve)
                        .Where(t => t.PhuongThuc == phuongThucKey &&
                                    t.TrangThai == trangThaiKey &&
                                    // So sánh thời gian chính xác đến GIÂY (như logic Index)
                                    t.ThoiGianThanhToan.Year == timeKey.Year &&
                                    t.ThoiGianThanhToan.Month == timeKey.Month &&
                                    t.ThoiGianThanhToan.Day == timeKey.Day &&
                                    t.ThoiGianThanhToan.Hour == timeKey.Hour &&
                                    t.ThoiGianThanhToan.Minute == timeKey.Minute &&
                                    t.ThoiGianThanhToan.Second == timeKey.Second)
                        .ToList();

                    // 4. Tách riêng danh sách Vé để xóa trước
                    // (Vì Vé có khóa ngoại trỏ đến ThanhToan hoặc ngược lại, nên xóa Vé/Chi tiết trước cho an toàn)
                    var danhSachVe = nhomThanhToanCanXoa
                                     .Where(t => t.Ve != null)
                                     .Select(t => t.Ve)
                                     .ToList();

                    // 5. THỰC HIỆN XÓA

                    // Bước A: Xóa các Vé liên quan trước
                    if (danhSachVe.Count > 0)
                    {
                        db.Ves.RemoveRange(danhSachVe);
                    }

                    // Bước B: Xóa danh sách Thanh Toán sau
                    if (nhomThanhToanCanXoa.Count > 0)
                    {
                        db.ThanhToans.RemoveRange(nhomThanhToanCanXoa);
                    }

                    // 6. Lưu thay đổi và Hoàn tất Transaction
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Đã xóa thành công !";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Gặp lỗi thì hoàn tác, không xóa gì cả
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
