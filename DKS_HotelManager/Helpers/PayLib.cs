using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DKS_HotelManager.Helpers
{
    public class PayLib
    {
        private readonly SortedList<string, string> requestData = new SortedList<string, string>(new PayCompare());
        private readonly SortedList<string, string> responseData = new SortedList<string, string>(new PayCompare());

        public void AddRequestData(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            value = value ?? string.Empty;
            if (requestData.ContainsKey(key))
            {
                requestData[key] = value;
            }
            else
            {
                requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("Base URL for the payment gateway must be provided.", nameof(baseUrl));
            }

            var data = new StringBuilder();
            var hash = new StringBuilder();
            foreach (var kv in requestData)
            {
                if (string.IsNullOrEmpty(kv.Value))
                {
                    continue;
                }

                data.Append(WebUtility.UrlEncode(kv.Key));
                data.Append("=");
                data.Append(WebUtility.UrlEncode(kv.Value));
                data.Append("&");

                hash.Append(WebUtility.UrlEncode(kv.Key));
                hash.Append("=");
                hash.Append(WebUtility.UrlEncode(kv.Value));
                hash.Append("&");
            }

            var queryString = data.ToString().TrimEnd('&');
            var signData = hash.ToString().TrimEnd('&');
            var secureHash = Util.HmacSHA512(hashSecret, signData);

            var separator = baseUrl.Contains("?") ? "&" : "?";
            return $"{baseUrl}{separator}{queryString}&vnp_SecureHash={secureHash}";
        }

        public void AddResponseData(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            value = value ?? string.Empty;
            if (responseData.ContainsKey(key))
            {
                responseData[key] = value;
            }
            else
            {
                responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        private string BuildResponseRawData()
        {
            var data = new StringBuilder();

            if (responseData.ContainsKey("vnp_SecureHashType"))
            {
                responseData.Remove("vnp_SecureHashType");
            }
            if (responseData.ContainsKey("vnp_SecureHash"))
            {
                responseData.Remove("vnp_SecureHash");
            }

            foreach (var kv in responseData)
            {
                if (string.IsNullOrEmpty(kv.Value))
                {
                    continue;
                }

                data.Append(WebUtility.UrlEncode(kv.Key));
                data.Append("=");
                data.Append(WebUtility.UrlEncode(kv.Value));
                data.Append("&");
            }

            return data.ToString().TrimEnd('&');
        }

        public bool ValidateSignature(string hashSecret)
        {
            var inputHash = GetResponseData("vnp_SecureHash");
            return ValidateSignature(inputHash, hashSecret);
        }

        public bool ValidateSignature(string inputHash, string hashSecret)
        {
            var rspRaw = BuildResponseRawData();
            var calculatedHash = Util.HmacSHA512(hashSecret, rspRaw);
            return calculatedHash.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}