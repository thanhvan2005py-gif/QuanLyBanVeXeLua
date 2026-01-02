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
    public class KhachHang_65134257Controller : Controller
    {
        private QLXeBusPhuongTrang_65134257Entities db = new QLXeBusPhuongTrang_65134257Entities();

        // GET: KhachHang_65134257
        public ActionResult Index()
        {
            return View(db.KhachHangs.ToList());
        }

        // GET: KhachHang_65134257/Details/5
        public ActionResult Details(int? id)
        {
            // 1. Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["KhachHang"] == null)
            {
                // Chưa đăng nhập thì đá về trang đăng nhập
                return RedirectToAction("DangNhap", "Account_65134257");
            }

            // 2. Lấy thông tin khách hàng từ Session
            // (Lưu ý: Bạn cần ép kiểu về đúng Model KhachHang của bạn)
            var khachHangSession = Session["KhachHang"] as QLXeBusPhuongTrang_65134257.Models.KhachHang;

            // 3. Logic xử lý ID:
            // Nếu không truyền id trên URL (trường hợp click từ menu), thì lấy ID của chính người đang đăng nhập
            if (id == null)
            {
                id = khachHangSession.MaKH;
            }

            // (Tùy chọn) Bảo mật: Nếu khách cố tình nhập ID của người khác trên URL
            // thì chặn lại, ép về ID của chính họ
            if (id != khachHangSession.MaKH)
            {
                id = khachHangSession.MaKH;
            }

            // 4. Truy vấn Database
            var khachHangDb = db.KhachHangs.Find(id);

            if (khachHangDb == null)
            {
                return HttpNotFound();
            }

            return View(khachHangDb);
        }
        // GET: KhachHang_65134257/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: KhachHang_65134257/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaKH,TenKH,DienThoai,Email,MatKhau")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KhachHangs.Add(khachHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(khachHang);
        }

        // GET: KhachHang_65134257/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // POST: KhachHang_65134257/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaKH,TenKH,DienThoai,Email,MatKhau")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khachHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // GET: KhachHang_65134257/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // POST: KhachHang_65134257/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            db.KhachHangs.Remove(khachHang);
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
