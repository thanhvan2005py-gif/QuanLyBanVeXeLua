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
    public class TaiXe_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: TaiXe_65134257
        public ActionResult Index()
        {
            return View(db.TaiXes.ToList());
        }

        // GET: TaiXe_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaiXe taiXe = db.TaiXes.Find(id);
            if (taiXe == null)
            {
                return HttpNotFound();
            }
            return View(taiXe);
        }

        // GET: TaiXe_65134257/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TaiXe_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaTaiXe,HoTen,DienThoai,BangLai")] TaiXe taiXe)
        {
            if (ModelState.IsValid)
            {
                db.TaiXes.Add(taiXe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(taiXe);
        }

        // GET: TaiXe_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaiXe taiXe = db.TaiXes.Find(id);
            if (taiXe == null)
            {
                return HttpNotFound();
            }
            return View(taiXe);
        }

        // POST: TaiXe_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaTaiXe,HoTen,DienThoai,BangLai")] TaiXe taiXe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(taiXe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taiXe);
        }

        // GET: TaiXe_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaiXe taiXe = db.TaiXes.Find(id);
            if (taiXe == null)
            {
                return HttpNotFound();
            }
            return View(taiXe);
        }

        // POST: TaiXe_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                // 1. Tìm tài xế
                TaiXe taiXe = db.TaiXes.Find(id);

                if (taiXe != null)
                {
                    // 2. Xóa và lưu
                    db.TaiXes.Remove(taiXe);
                    db.SaveChanges();
                }

                // 3. Thông báo thành công
                TempData["Success"] = "Đã xóa tài xế thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 4. Nếu lỗi (Tài xế này đang có trong Lịch Chạy hoặc phân công khác)
                TempData["Error"] = "Không thể xóa tài xế này vì đang có Lịch chạy hoặc dữ liệu liên quan!";
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
