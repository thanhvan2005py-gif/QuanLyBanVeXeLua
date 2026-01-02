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
    public class Xe_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: Xe_65134257
        public ActionResult Index()
        {
            return View(db.Xes.ToList());
        }

        // GET: Xe_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Xe xe = db.Xes.Find(id);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }

        // GET: Xe_65134257/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Xe_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaXe,BienSo,SoCho,LoaiXe,TrangThai")] Xe xe)
        {
            if (ModelState.IsValid)
            {
                db.Xes.Add(xe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(xe);
        }

        // GET: Xe_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Xe xe = db.Xes.Find(id);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }

        // POST: Xe_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaXe,BienSo,SoCho,LoaiXe,TrangThai")] Xe xe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(xe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(xe);
        }

        // GET: Xe_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Xe xe = db.Xes.Find(id);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }

        // POST: Xe_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                // 1. Tìm xe
                Xe xe = db.Xes.Find(id);

                if (xe != null)
                {
                    // 2. Xóa và lưu
                    db.Xes.Remove(xe);
                    db.SaveChanges();
                }

                // 3. Thông báo thành công
                TempData["Success"] = "Đã xóa xe thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 4. Nếu lỗi (Xe này đang được dùng trong Lịch Chạy hoặc Chuyến)
                TempData["Error"] = "Không thể xóa xe này vì đang có Lịch chạy hoặc dữ liệu liên quan!";
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
