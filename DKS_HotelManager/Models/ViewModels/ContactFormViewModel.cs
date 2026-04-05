using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DKS_HotelManager.Models.ViewModels
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(120, ErrorMessage = "Họ tên tối đa 120 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập chủ đề.")]
        [StringLength(120, ErrorMessage = "Chủ đề tối đa 120 ký tự.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung liên hệ.")]
        [StringLength(1500, ErrorMessage = "Nội dung tối đa 1500 ký tự.")]
        public string Message { get; set; }
    }
}

