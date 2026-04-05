using System;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CommentsController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new CommentsPageViewModel
            {
                Comments = GetCommentItems(),
                CustomerOptions = GetCustomerSelectList(),
                HotelOptions = GetHotelSelectList(),
                RoomOptions = GetRoomSelectList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveComment(CommentInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AdminError"] = "Thông tin bình luận không hợp lệ.";
                return RedirectToAction("Index");
            }

            var customer = db.KHACHHANGs.Find(model.MKH);
            if (customer == null)
            {
                TempData["AdminError"] = "Khách hàng chọn không tồn tại.";
                return RedirectToAction("Index");
            }

            BINHLUAN comment;
            if (model.MaBL.HasValue && model.MaBL.Value > 0)
            {
                comment = db.BINHLUANs.FirstOrDefault(b => b.MaBL == model.MaBL.Value);
                if (comment == null)
                {
                    TempData["AdminError"] = "Không tìm thấy bình luận để cập nhật.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                comment = new BINHLUAN();
                db.BINHLUANs.Add(comment);
            }

            comment.MaKS = model.MaKS;
            comment.MaPhong = model.MaPhong;
            comment.MKH = model.MKH;
            comment.NoiDung = model.NoiDung?.Trim();
            comment.NgayBL = model.NgayBL ?? DateTime.Now;
            db.SaveChanges();

            TempData["AdminSuccess"] = model.MaBL.HasValue ? "Đã cập nhật bình luận." : "Đã thêm bình luận mới.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int maBL)
        {
            var comment = db.BINHLUANs.FirstOrDefault(b => b.MaBL == maBL);
            if (comment == null)
            {
                TempData["AdminError"] = "Bình luận không tồn tại.";
                return RedirectToAction("Index");
            }

            db.BINHLUANs.Remove(comment);
            db.SaveChanges();
            TempData["AdminSuccess"] = "Đã xóa bình luận.";
            return RedirectToAction("Index");
        }
    }
}
