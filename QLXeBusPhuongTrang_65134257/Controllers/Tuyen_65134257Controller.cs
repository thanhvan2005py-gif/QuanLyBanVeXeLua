using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QLXeBusPhuongTrang_65134257.Models;

namespace QLXeBusPhuongTrang_65134257.Controllers
{
    public class Tuyen_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: Tuyen_65134257
        public ActionResult Index(int? BenDauId, int? BenCuoiId)
        {
            // 1. Chuẩn bị Dropdown List cho Bến Xe
            // Dùng SelectList để dễ dàng liên kết trong View
            ViewBag.BenDauId = new SelectList(db.BenXes.ToList(), "MaBen", "TenBen", BenDauId);
            ViewBag.BenCuoiId = new SelectList(db.BenXes.ToList(), "MaBen", "TenBen", BenCuoiId);

            // 2. Bắt đầu truy vấn
            IQueryable<Tuyen> tuyens = db.Tuyens
                                        .Include(t => t.BenXe) // Bến Đi
                                        .Include(t => t.BenXe1); // Bến Đến (BenCuoi)

            // 3. Xử lý Lọc theo Bến Đi (BenDau)
            if (BenDauId.HasValue)
            {
                tuyens = tuyens.Where(t => t.BenDau == BenDauId.Value);
            }

            // 4. Xử lý Lọc theo Bến Đến (BenCuoi)
            if (BenCuoiId.HasValue)
            {
                tuyens = tuyens.Where(t => t.BenCuoi == BenCuoiId.Value);
            }

            // Trả về danh sách đã lọc (hoặc toàn bộ nếu không có lọc)
            return View(tuyens.ToList());
        }

        // GET: Tuyen_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tuyen tuyen = db.Tuyens.Find(id);
            if (tuyen == null)
            {
                return HttpNotFound();
            }
            return View(tuyen);
        }

        // GET: Tuyen_65134257/Create
        public ActionResult Create()
        {
            ViewBag.BenCuoi = new SelectList(db.BenXes, "MaBen", "TenBen");
            ViewBag.BenDau = new SelectList(db.BenXes, "MaBen", "TenBen");
            return View();
        }

        // POST: Tuyen_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaTuyen,TenTuyen,BenDau,BenCuoi,QuangDuongKm")] Tuyen tuyen)
        {
            if (ModelState.IsValid)
            {
                db.Tuyens.Add(tuyen);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BenCuoi = new SelectList(db.BenXes, "MaBen", "TenBen", tuyen.BenCuoi);
            ViewBag.BenDau = new SelectList(db.BenXes, "MaBen", "TenBen", tuyen.BenDau);
            return View(tuyen);
        }

        // GET: Tuyen_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tuyen tuyen = db.Tuyens.Find(id);
            if (tuyen == null)
            {
                return HttpNotFound();
            }
            ViewBag.BenCuoi = new SelectList(db.BenXes, "MaBen", "TenBen", tuyen.BenCuoi);
            ViewBag.BenDau = new SelectList(db.BenXes, "MaBen", "TenBen", tuyen.BenDau);
            return View(tuyen);
        }

        // POST: Tuyen_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaTuyen,TenTuyen,BenDau,BenCuoi,QuangDuongKm")] Tuyen tuyen)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tuyen).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BenCuoi = new SelectList(db.BenXes, "MaBen", "TenBen", tuyen.BenCuoi);
            ViewBag.BenDau = new SelectList(db.BenXes, "MaBen", "TenBen", tuyen.BenDau);
            return View(tuyen);
        }

        // GET: Tuyen_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tuyen tuyen = db.Tuyens.Find(id);
            if (tuyen == null)
            {
                return HttpNotFound();
            }
            return View(tuyen);
        }

        // POST: Tuyen_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                // 1. Tìm tuyến cần xóa
                Tuyen tuyen = db.Tuyens.Find(id);

                // 2. Xóa và lưu
                db.Tuyens.Remove(tuyen);
                db.SaveChanges();

                // 3. Thông báo thành công
                TempData["Success"] = "Đã xóa tuyến xe thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 4. Nếu lỗi (do Tuyến đang có Lịch Chạy sử dụng)
                TempData["Error"] = "Không thể xóa tuyến này vì đang có Lịch chạy hoặc dữ liệu liên quan!";
                return RedirectToAction("Index");
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
