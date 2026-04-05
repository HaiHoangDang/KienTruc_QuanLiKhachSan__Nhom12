using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DKS_HotelManager.Helpers
{
    public class Util
    {
        public static string Hmac(string hashType, string key, string inputData)
        {
            var algorithm = string.IsNullOrWhiteSpace(hashType) ? "SHA512" : hashType.Trim().ToUpperInvariant();
            var keyBytes = Encoding.UTF8.GetBytes(key ?? string.Empty);
            var inputBytes = Encoding.UTF8.GetBytes(inputData ?? string.Empty);
            using (var hmac = CreateHmacInstance(algorithm, keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                var hash = new StringBuilder();
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
                return hash.ToString().ToUpperInvariant();
            }
        }

        public static string HmacSHA512(string key, string inputData)
        {
            return Hmac("SHA512", key, inputData);
        }

        private static HMAC CreateHmacInstance(string algorithm, byte[] keyBytes)
        {
            switch (algorithm)
            {
                case "SHA256":
                    return new HMACSHA256(keyBytes);
                case "SHA384":
                    return new HMACSHA384(keyBytes);
                case "SHA1":
                    return new HMACSHA1(keyBytes);
                case "MD5":
                    return new HMACMD5(keyBytes);
                default:
                    return new HMACSHA512(keyBytes);
            }
        }

        public static string GetIpAddress()
        {
            string ipAddress;
            try
            {
                ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
                {
                    ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP:" + ex.Message;
            }
            return ipAddress;
        }
    }
}
