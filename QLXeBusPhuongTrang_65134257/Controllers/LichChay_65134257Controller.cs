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
    public class LichChay_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: LichChay_65134257
        public ActionResult Index()
        {
            var lichChays = db.LichChays.Include(l => l.TaiXe).Include(l => l.Tuyen).Include(l => l.Xe);
            return View(lichChays.ToList());
        }

        // GET: LichChay_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LichChay lichChay = db.LichChays.Find(id);
            if (lichChay == null)
            {
                return HttpNotFound();
            }
            return View(lichChay);
        }

        // GET: LichChay_65134257/Create
        public ActionResult Create()
        {
            ViewBag.MaTaiXe = new SelectList(db.TaiXes, "MaTaiXe", "HoTen");
            ViewBag.MaTuyen = new SelectList(db.Tuyens, "MaTuyen", "TenTuyen");
            ViewBag.MaXe = new SelectList(db.Xes, "MaXe", "BienSo");
            return View();
        }

        // POST: LichChay_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaLichChay,MaTuyen,MaXe,MaTaiXe,GioKhoiHanh,GioDen,TrangThai")] LichChay lichChay)
        {
            if (ModelState.IsValid)
            {
                db.LichChays.Add(lichChay);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaTaiXe = new SelectList(db.TaiXes, "MaTaiXe", "HoTen", lichChay.MaTaiXe);
            ViewBag.MaTuyen = new SelectList(db.Tuyens, "MaTuyen", "TenTuyen", lichChay.MaTuyen);
            ViewBag.MaXe = new SelectList(db.Xes, "MaXe", "BienSo", lichChay.MaXe);
            return View(lichChay);
        }

        // GET: LichChay_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LichChay lichChay = db.LichChays.Find(id);
            if (lichChay == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaTaiXe = new SelectList(db.TaiXes, "MaTaiXe", "HoTen", lichChay.MaTaiXe);
            ViewBag.MaTuyen = new SelectList(db.Tuyens, "MaTuyen", "TenTuyen", lichChay.MaTuyen);
            ViewBag.MaXe = new SelectList(db.Xes, "MaXe", "BienSo", lichChay.MaXe);
            return View(lichChay);
        }

        // POST: LichChay_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLichChay,MaTuyen,MaXe,MaTaiXe,GioKhoiHanh,GioDen,TrangThai")] LichChay lichChay)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lichChay).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaTaiXe = new SelectList(db.TaiXes, "MaTaiXe", "HoTen", lichChay.MaTaiXe);
            ViewBag.MaTuyen = new SelectList(db.Tuyens, "MaTuyen", "TenTuyen", lichChay.MaTuyen);
            ViewBag.MaXe = new SelectList(db.Xes, "MaXe", "BienSo", lichChay.MaXe);
            return View(lichChay);
        }

        // GET: LichChay_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LichChay lichChay = db.LichChays.Find(id);
            if (lichChay == null)
            {
                return HttpNotFound();
            }
            return View(lichChay);
        }

        // POST: LichChay_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                LichChay lichChay = db.LichChays.Find(id);
                db.LichChays.Remove(lichChay);
                db.SaveChanges();

                // THÊM DÒNG NÀY: Lưu thông báo thành công
                TempData["Success"] = "Đã xóa lịch chạy thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Lưu thông báo lỗi
                TempData["Error"] = "Không thể xóa lịch chạy này vì đã có dữ liệu chuyến xe liên quan!";
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
