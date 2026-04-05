using System;
using System.Net;
using System.Net.Mail;

namespace DKS_HotelManager.Helpers
{
    public static class MailService
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SmtpUser = "2324802010017@student.tdmu.edu.vn";
        private const string SmtpPass = "ekyd ehti enon ljkx";

        public static void SendMail(string email, string subject, string body, bool isHtml = true)
        {
            using (var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = new NetworkCredential(SmtpUser, SmtpPass),
                EnableSsl = true
            })
            using (var message = new MailMessage
            {
                From = new MailAddress(SmtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            })
            {
                message.To.Add(new MailAddress(email));
                client.Send(message);
            }
        }

        public static void SendConfirmationCode(string email, string code)
        {
            var body = $"<p>Mã xác nhận đăng ký của bạn là <strong>{code}</strong>.</p>" +
                       "<p>Vui lòng nhập mã này trong vòng 10 phút để hoàn tất đăng ký.</p>";
            SendMail(email, "Mã xác nhận đăng ký - DKS Hotel", body);
        }
    }
}
