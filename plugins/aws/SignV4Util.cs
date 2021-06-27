using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

// https://docs.aws.amazon.com/general/latest/gr/sigv4-create-canonical-request.html
// https://stackoverflow.com/questions/37664755/aws-what-a-canonical-request-is-really

namespace Coveo.Dal
{
    public interface IProxyRequest
    {
        string Method { get; }
        Uri RequestUri { get; }
        byte[] Body { get; }
        IProxyHeaders Headers { get; }
    }

    public interface IProxyHeaders
    {
        string Get(string name);
        void Set(string name, string value);
        IEnumerable<string> GetValues(string name);
        IEnumerable<string> Keys { get; }
    }

    // Note: in GetCanonicalHeaders I only use the 'host' and 'x-amz-date'. It works fine. And when using chrome to ping servers, there seemed to be problematic headers...
    
    public static class SignV4Util
    {
        static readonly char[] _datePartSplitChars = {'T'};

        private static string HDR_X_AMZ_DATE = "x-amz-date";
        private static string HDR_AUTHORIZATION = "authorization";
        private static string HDR_X_AMZ_SECURITY_TOKEN = "x-amz-security-token";

        public static void SignRequest(string method, Uri requestUri, IProxyHeaders headers, byte[] body, AwsCredentials credentials, string region, string service)
        {
            var date = DateTime.UtcNow;
            //date = new DateTime(2020, 7, 5, 15, 19, 17, DateTimeKind.Utc); // 20200705T151917Z
            var dateStamp = date.ToString("yyyyMMdd");
            var amzDate = date.ToString("yyyyMMddTHHmmssZ");
            headers.Set(HDR_X_AMZ_DATE, amzDate);

            var canonicalHeaders = GetCanonicalHeaders(requestUri, headers);
            var signingKey = GetSigningKey(credentials.SecretKey, dateStamp, region, service);
            var stringToSign = GetStringToSign(method, requestUri, headers, canonicalHeaders, body, region, service);
            //Console.Write($"========== String to Sign ==========\r\n{stringToSign}\r\n========== String to Sign ==========\r\n");
            var signature = signingKey.GetHmacSha256Hash(stringToSign).ToLowercaseHex();
            var auth = $"AWS4-HMAC-SHA256 Credential={credentials.AccessKey}/{GetCredentialScope(dateStamp, region, service)}, SignedHeaders={GetSignedHeaders(canonicalHeaders)}, Signature={signature}";
            headers.Set(HDR_AUTHORIZATION, auth);
            if (!string.IsNullOrWhiteSpace(credentials.Token))
                headers.Set(HDR_X_AMZ_SECURITY_TOKEN, credentials.Token);
        }

        public static byte[] GetSigningKey(string secretKey, string dateStamp, string region, string service)
        {
            return _encoding.GetBytes("AWS4" + secretKey)
                .GetHmacSha256Hash(dateStamp)
                .GetHmacSha256Hash(region)
                .GetHmacSha256Hash(service)
                .GetHmacSha256Hash("aws4_request");
        }

        public static byte[] GetHmacSha256Hash(this byte[] key, string data)
        {
            using var kha = new HMACSHA256 {Key = key};
            return kha.ComputeHash(_encoding.GetBytes(data));
        }

        public static string GetStringToSign(string method, Uri requestUri, IProxyHeaders headers, Dictionary<string, string> canonicalHeaders, byte[] data, string region, string service)
        {
            var canonicalRequest = GetCanonicalRequest(method, requestUri, canonicalHeaders, data);
            //Console.Write($"========== Canonical Request ==========\r\n{canonicalRequest}\r\n========== Canonical Request ==========\r\n");
            var awsDate = headers.Get(HDR_X_AMZ_DATE);
            Debug.Assert(Regex.IsMatch(awsDate, @"\d{8}T\d{6}Z"));
            var datePart = awsDate.Split(_datePartSplitChars, 2)[0];
            return string.Join("\n",
                "AWS4-HMAC-SHA256",
                awsDate,
                GetCredentialScope(datePart, region, service),
                GetHash(canonicalRequest).ToLowercaseHex()
            );
        }

        public static string GetCredentialScope(string date, string region, string service)
        {
            return $"{date}/{region}/{service}/aws4_request";
        }

        public static string GetCanonicalRequest(string method, Uri requestUri, Dictionary<string, string> canonicalHeaders, byte[] data)
        {
            var result = new StringBuilder();
            result.Append(method);
            result.Append('\n');
            result.Append(GetPath(requestUri));
            result.Append('\n');
            result.Append(requestUri.GetCanonicalQueryString());
            result.Append('\n');
            WriteCanonicalHeaders(canonicalHeaders, result);
            result.Append('\n');
            WriteSignedHeaders(canonicalHeaders, result);
            result.Append('\n');
            WriteRequestPayloadHash(data, result);
            return result.ToString();
        }

        private static string GetPath(Uri uri)
        {
            var path = uri.AbsolutePath;
            if (path.Length == 0) return "/";

            IEnumerable<string> segments = path
                .Split('/')
                .Select(segment => WebUtility.UrlEncode(segment).Replace("*", "%2A"));
            return string.Join("/", segments);
        }

        private static Dictionary<string, string> GetCanonicalHeaders(Uri requestUri, IProxyHeaders headers)
        {
            //var q = from string key in headers.Keys
            //    let headerName = key.ToLowerInvariant()
            //    where headerName != "connection"
            //    let headerValues = string.Join(",", headers.GetValues(key) ?? Enumerable.Empty<string>().Select(v => v.Trimall()))
            //    select new {headerName, headerValues};
            //var result = q.ToDictionary(v => v.headerName, v => v.headerValues);
            var result = new Dictionary<string, string>();
            result[HDR_X_AMZ_DATE] = headers.Get(HDR_X_AMZ_DATE);
            result["host"] = requestUri.Host.ToLowerInvariant();
            return result;
        }

        private static void WriteCanonicalHeaders(Dictionary<string, string> canonicalHeaders, StringBuilder output)
        {
            var q = from pair in canonicalHeaders
                orderby pair.Key
                select $"{pair.Key}:{pair.Value}\n";
            foreach (var line in q) {
                output.Append(line);
            }
        }

        private static string GetSignedHeaders(Dictionary<string, string> canonicalHeaders)
        {
            var result = new StringBuilder();
            WriteSignedHeaders(canonicalHeaders, result);
            return result.ToString();
        }

        private static void WriteSignedHeaders(Dictionary<string, string> canonicalHeaders, StringBuilder output)
        {
            bool started = false;
            foreach (var pair in canonicalHeaders.OrderBy(v => v.Key)) {
                if (started) output.Append(';');
                output.Append(pair.Key.ToLowerInvariant());
                started = true;
            }
        }

        public static string GetCanonicalQueryString(this Uri uri)
        {
            if (string.IsNullOrWhiteSpace(uri.Query))
                return string.Empty;
            
            // As per the reference at the beginning of the class, the query params have to be sorted by name, then value.
            // I just pre-concatenate them as 'q=v' strings, where v is encoded, then sort at them very end.
            // This assumes that
            //    1- There are no chars to encode in the names. Which should be true for the redirects we do anyway. I.e. we'll have '-' and '_', but that's it.
            //    2- The 'encoded' values will end up sorted the same as if they were not encoded. Which should be true since the encoding is '%nn' where nn is the ascii, and 'char' sorting would result in the same order.
            var qvList = new List<string>();
            var nameValues = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var nb = nameValues.Count;
            for (var i = 0; i < nb; ++i) {
                var key = nameValues.GetKey(i);
                var values = nameValues.GetValues(i);
                if (values == null)
                    qvList.Add(key + '=');
                else
                    foreach (var val in values)
                        qvList.Add(key + '=' + Uri.EscapeDataString(val));//)EncodeString(val)); // todo - this escaping seems to work but I don't think it matches the rules of EncodeString. See references at the beginning of the class.
            }
            return string.Join('&', qvList.OrderBy(x => x, StringComparer.Ordinal));
        }

        //private static string EncodeString(string val)
        //{
        //    //return Uri.EscapeDataString(val);
        //    var sb = new StringBuilder();
        //    sb.WriteEncoded(val);
        //    return sb.ToString();
        //}

        //private static void WriteEncoded(this StringBuilder output, string value)
        //{
        //    for (var i = 0; i < value.Length; ++i) {
        //        if (value[i].RequiresEncoding()) {
        //            output.Append(Uri.EscapeDataString(value[i].ToString()));
        //        } else {
        //            output.Append(value[i]);
        //        }
        //    }
        //}

        //private static bool RequiresEncoding(this char value)
        //{
        //    if ('A' <= value && value <= 'Z') return false;
        //    if ('a' <= value && value <= 'z') return false;
        //    if ('0' <= value && value <= '9') return false;
        //    switch (value) {
        //    case '-':
        //    case '_':
        //    case '.':
        //    case '~':
        //        return false;
        //    }
        //    return true;
        //}

        static readonly byte[] _emptyBytes = new byte[0];

        public static void WriteRequestPayloadHash(byte[] data, StringBuilder output)
        {
            data ??= _emptyBytes;
            var hash = GetHash(data);
            foreach (var b in hash) {
                output.AppendFormat("{0:x2}", b);
            }
        }

        public  static string ToLowercaseHex(this byte[] data)
        {
            var result = new StringBuilder();
            foreach (var b in data) {
                result.AppendFormat("{0:x2}", b);
            }
            return result.ToString();
        }

        static readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        public static byte[] GetHash(string data)
        {
            return GetHash(_encoding.GetBytes(data));
        }

        private static byte[] GetHash(this byte[] data)
        {
            using var algo = SHA256.Create();
            return algo.ComputeHash(data);
        }
    }

    // todo - this should be a Dictionary<string, string> because it comes as json.
    public class MyHeaders : IProxyHeaders
    {
        private readonly WebHeaderCollection _headers;

        public MyHeaders(WebHeaderCollection headers)
        {
            _headers = headers;
        }

        public string Get(string name) => _headers[name];
        
        public void Set(string name, string value)
        {
            _headers.Set(name, value);
            //_headers.Add(name, value);
        }

        public IEnumerable<string> GetValues(string name) => _headers.GetValues(name);
	
        public IEnumerable<string> Keys => _headers.AllKeys;
    }
}
