using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace OnDemandTutor.API.VNPayLibrary
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private SortedList<String, String> _requestData = new SortedList<String, String>(new VnPayCompare());
        private SortedList<String, String> _responseData = new SortedList<String, String>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        #region Request
        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var data = new StringBuilder();
            foreach (var (key, value) in _requestData)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
                }
            }
            var queryString = data.ToString().TrimEnd('&');
            baseUrl += "?" + queryString;
            var signData = queryString;
            var vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "&vnp_SecureHash=" + vnp_SecureHash;
            return baseUrl;
        }
        #endregion

        #region Response process
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            foreach (var (key, value) in _responseData)
            {
                if (!string.IsNullOrEmpty(value) && key != "vnp_SecureHashType" && key != "vnp_SecureHash")
                {
                    data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
                }
            }
            return data.ToString().TrimEnd('&');
        }
        #endregion
    }

    public class Utils
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "Unable to retrieve IP address.";
            return ipAddress;
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
