using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QLXeBusPhuongTrang_65134257.Models;
using QLXeBusPhuongTrang_65134257.Models.ViewModels;

namespace QLXeBusPhuongTrang_65134257.Controllers
{
    public class Chuyen_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: Chuyen_65134257
        public ActionResult LichChay(int? id) // id ở đây chính là MaTuyen
        {
            // 1. Kiểm tra Tuyến có tồn tại không
            var tuyen = db.Tuyens
                          .Include(t => t.BenXe) // Bến Đầu
                          .Include(t => t.BenXe1) // Bến Cuối
                          .FirstOrDefault(t => t.MaTuyen == id);

            if (tuyen == null)
            {
                return HttpNotFound();
            }

            // Lưu thông tin Tuyến vào ViewBag để hiển thị trong View
            ViewBag.Tuyen = tuyen;

            // 2. Truy vấn Lịch Chạy của Tuyến đó
            // Phải Include các bảng liên quan: Xe, TaiXe và Chuyen
            var lichChays = db.LichChays
                              .Include(lc => lc.Xe)
                              .Include(lc => lc.TaiXe)
                              // Chỉ lấy các lịch chạy đang "Hoạt động" (tùy chọn)
                              .Where(lc => lc.MaTuyen == id && lc.TrangThai == "Hoạt động")
                              .ToList();

            // 3. Xử lý dữ liệu Chuyến cho từng Lịch Chạy
            // Chúng ta sẽ lọc các chuyến cho ngày hiện tại và 2-3 ngày tiếp theo
            DateTime ngayBatDau = DateTime.Today;
            int soNgayTiepTheo = 3;

            foreach (var lichChay in lichChays)
            {
                // Lấy danh sách các chuyến có sẵn (Chuyen) từ ngày hôm nay trở đi
                lichChay.Chuyens = lichChay.Chuyens
                                            .Where(c => c.Ngay >= ngayBatDau && c.Ngay <= ngayBatDau.AddDays(soNgayTiepTheo))
                                            .OrderBy(c => c.Ngay)
                                            .ToList();
            }

            ViewBag.ActivePage = "Tuyen";
            return View(lichChays);
        }
        public ActionResult ChonGhe(int MaChuyen)
        {
            // 1. Kiểm tra đăng nhập
            if (Session["KhachHang"] == null)
            {
                return RedirectToAction("DangNhap", "Account_65134257");
            }

            var kh = (KhachHang)Session["KhachHang"];


            // 2. Lấy thông tin chuyến
            var chuyen = db.Chuyens
                           .Include(c => c.LichChay.Xe)
                           .Include(c => c.LichChay.Tuyen.BenXe)
                           .Include(c => c.LichChay.Tuyen.BenXe1)
                           .FirstOrDefault(c => c.MaChuyen == MaChuyen);

            if (chuyen == null)
            {
                return HttpNotFound("Không tìm thấy chuyến đi.");
            }

            // 3. Lấy danh sách ghế đã đặt / thanh toán
            var veDaDat = db.Ves
                            .Where(v => v.MaChuyen == MaChuyen)
                            .Select(v => new GheTrangThaiVM
                            {
                                SoGhe = v.SoGhe,
                                TrangThai = v.TrangThai
                            })
                            .ToList();

            // 4. Truyền dữ liệu sang View
            ViewBag.Chuyen = chuyen;
            ViewBag.SoCho = chuyen.LichChay.Xe.SoCho;
            ViewBag.LoaiXe = chuyen.LichChay.Xe.LoaiXe;
            ViewBag.VeDaDat = veDaDat;

            // 👉 THÔNG TIN KHÁCH HÀNG (đã đăng nhập)
            ViewBag.TenKH = kh.TenKH;
            ViewBag.DienThoai = kh.DienThoai;

            ViewBag.ActivePage = "Tuyen";

            return View(chuyen);
        }
        public ActionResult Index()
        {
            var chuyens = db.Chuyens.Include(c => c.LichChay);
            return View(chuyens.ToList());
        }

        // GET: Chuyen_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chuyen chuyen = db.Chuyens.Find(id);
            if (chuyen == null)
            {
                return HttpNotFound();
            }
            return View(chuyen);
        }

        // GET: Chuyen_65134257/Create
        public ActionResult Create()
        {
            ViewBag.MaLichChay = new SelectList(db.LichChays, "MaLichChay", "TrangThai","GiaVe");
            return View();
        }

        // POST: Chuyen_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaChuyen,MaLichChay,Ngay,GiaVe")] Chuyen chuyen)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm thông tin Lịch chạy dựa trên Mã người dùng nhập
                var lichChay = db.LichChays.Find(chuyen.MaLichChay);

                if (lichChay == null)
                {
                    ModelState.AddModelError("MaLichChay", "Mã lịch chạy không tồn tại!");
                    return View(chuyen);
                }

                // 2. Từ Lịch chạy, lấy thông tin Xe để biết số ghế
                // (Lưu ý: trong Model LichChay phải có quan hệ với Xe, hoặc query trực tiếp)
                var xe = db.Xes.Find(lichChay.MaXe);

                // 3. Gán số ghế của xe vào số ghế trống của chuyến
                chuyen.SoGheTrong = xe.SoCho;

                // 4. Lưu
                db.Chuyens.Add(chuyen);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(chuyen);
        }

        // GET: Chuyen_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chuyen chuyen = db.Chuyens.Find(id);
            if (chuyen == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaLichChay = new SelectList(db.LichChays, "MaLichChay", "TrangThai", chuyen.MaLichChay);
            return View(chuyen);
        }

        // POST: Chuyen_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaChuyen,MaLichChay,Ngay,SoGheTrong,GiaVe")] Chuyen chuyen)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chuyen).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaLichChay = new SelectList(db.LichChays, "MaLichChay", "TrangThai", chuyen.MaLichChay);
            return View(chuyen);
        }

        // GET: Chuyen_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chuyen chuyen = db.Chuyens.Find(id);
            if (chuyen == null)
            {
                return HttpNotFound();
            }
            return View(chuyen);
        }

        // POST: Chuyen_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Chuyen chuyen = db.Chuyens.Find(id);
            db.Chuyens.Remove(chuyen);
            db.SaveChanges();
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
    }
}
