using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DKS_HotelManager.Models.ViewModels
{
    public class ContactCommentFormViewModel
    {
        public int? HotelId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận.")]
        [StringLength(500, ErrorMessage = "Bình luận tối đa 500 ký tự.")]
        public string Content { get; set; }
    }

    public class ContactCommentItemViewModel
    {
        public string CustomerName { get; set; }
        public string HotelName { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ContactCommentPageViewModel
    {
        public IEnumerable<SelectListItem> HotelOptions { get; set; } = new List<SelectListItem>();
        public ContactCommentFormViewModel Form { get; set; } = new ContactCommentFormViewModel();
        public IEnumerable<ContactCommentItemViewModel> Comments { get; set; } = new List<ContactCommentItemViewModel>();
    }
}
