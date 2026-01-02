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
    public class BenXe_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: BenXe_65134257
        public ActionResult Index()
        {
            return View(db.BenXes.ToList());
        }

        // GET: BenXe_65134257/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BenXe benXe = db.BenXes.Find(id);
            if (benXe == null)
            {
                return HttpNotFound();
            }
            return View(benXe);
        }

        // GET: BenXe_65134257/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BenXe_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaBen,TenBen,DiaChi")] BenXe benXe)
        {
            if (ModelState.IsValid)
            {
                db.BenXes.Add(benXe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(benXe);
        }

        // GET: BenXe_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BenXe benXe = db.BenXes.Find(id);
            if (benXe == null)
            {
                return HttpNotFound();
            }
            return View(benXe);
        }

        // POST: BenXe_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaBen,TenBen,DiaChi")] BenXe benXe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(benXe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(benXe);
        }

        // GET: BenXe_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BenXe benXe = db.BenXes.Find(id);
            if (benXe == null)
            {
                return HttpNotFound();
            }
            return View(benXe);
        }

        // POST: BenXe_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                // 1. Tìm Bến Xe
                BenXe benXe = db.BenXes.Find(id);

                if (benXe != null)
                {
                    // 2. Xóa và lưu
                    db.BenXes.Remove(benXe);
                    db.SaveChanges();

                    // 3. Thông báo thành công
                    TempData["Success"] = $"Đã xóa bến xe '{benXe.TenBen}' thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy bến xe cần xóa.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 4. Nếu lỗi (Bến Xe này đang được dùng trong Tuyen, LichChay, v.v.)
                TempData["Error"] = "Không thể xóa bến xe này vì đang có Tuyến đường hoặc dữ liệu liên quan sử dụng!";
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
