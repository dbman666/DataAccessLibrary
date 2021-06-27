using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Coveo.Dal;

public delegate void LambdaFn();

public static class CmfUtil
{
    public static byte[] PRINTABLE_CHARS =
    {
//     00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15
        0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, //  00                            \t \n       \r
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //  16            
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //  32     !  "  #  $  %  &  '  (  )  *  +  ,  -  .  /
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //  48  0  1  2  3  4  5  6  7  8  9  :  ;  <  =  >  ?
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //  64  @  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //  80  P  Q  R  S  T  U  V  W  X  Y  Z  [  \  ]  ^  _
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //  96  `  a  b  c  d  e  f  g  h  i  j  k  l  m  n  o
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 112  p  q  r  s  t  u  v  w  x  y  z  {  |  }  ~
    };

    static Regex BASE64_REGEX = new Regex(@"^[a-zA-Z0-9\+/\n\r]*={0,2}$", RegexOptions.Compiled);
        
    public static DateTime DateTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Contents of dbkeys.jks, extracted with KeyStore Epxlorer: aes, 128, raw, encoded: 96610422C64D666C12FBC12F53C3774
    private static byte[] ENCODED_KEY = { 0x09, 0x66, 0x10, 0x42, 0x2C, 0x64, 0xD6, 0x66, 0xC1, 0x2F, 0xBC, 0x12, 0xF5, 0x3C, 0x37, 0x74 }; // from 'encoded' above; zero-padding at the start

    const ulong G_BYTES = 1024 * 1024 * 1024;
    const ulong M_BYTES = 1024 * 1024;
    const ulong K_BYTES = 1024;

    public static void Throw(string p_Message)
    {
        throw new Exception(p_Message);
    }

    public static bool EqNoCase(string s1, string s2)
    {
        return string.Compare(s1, s2, true) == 0;
    }

    public static string ReadBody(HttpWebRequest p_Request, out bool p_Failed)
    {
        p_Failed = false;
        try {
            return ReadAllStreamAsUtf8String(p_Request.GetResponse());
        } catch (Exception ex) {
            p_Failed = true;
            if (ex is WebException wex) {
                var reply = wex.Response == null ? null : ReadAllStreamAsUtf8String(wex.Response);
                return reply ?? wex.Message;
            }
        }
        return null;
    }

    public static string ReadAllStreamAsUtf8String(WebResponse p_Response)
    {
        using (var inStream = p_Response.GetResponseStream()) {
            return inStream == null ? null : ReadAllStreamAsUtf8String(inStream);
        }
    }

    public static string ReadAllStreamAsUtf8String(Stream p_Stream)
    {
        var outStream = new MemoryStream();
        byte[] bytes = new byte[65536];
        while (true)
        {
            int bytesRead = p_Stream.Read(bytes, 0, 65536);
            if (bytesRead == 0) break;
            outStream.Write(bytes, 0, bytesRead);
        }
        return Encoding.UTF8.GetString(outStream.ToArray());
    }

    public static StringBuilder Space(this StringBuilder p_Sb)
    {
        return p_Sb.Append(Ctes.CHAR_SPACE);
    }

    public static StringBuilder CommaSpace(this StringBuilder p_Sb)
    {
        return p_Sb.Append(Ctes.SEP_COMMA_SPACE);
    }

    public static StringBuilder CommaSpace(this StringBuilder sb, int i)
    {
        return i != 0 ? sb.Append(Ctes.SEP_COMMA_SPACE) : sb;
    }

    public static StringBuilder ColonSpace(this StringBuilder p_Sb)
    {
        return p_Sb.Append(Ctes.SEP_COLON_SPACE);
    }

    public static string QuotedIfString(this object p_Obj)
    {
        return p_Obj is string ? Ctes.CHAR_DOUBLE_QUOTE + (string)p_Obj + Ctes.CHAR_DOUBLE_QUOTE : p_Obj.ToString();
    }

    public static bool IsNullOrEmpty<T>(this List<T> p_List)
    {
        return p_List == null || p_List.Count == 0;
    }

    public static string RemoveBackquote(this string p_Str)
    {
        if (p_Str == null)
            return null;
        if (p_Str.Length >= 2 && p_Str[0] == Ctes.CHAR_BACK_QUOTE)
            return p_Str.Substring(1, p_Str.Length - 2);
        return p_Str;
    }

    public static string RemoveCrLf(this string p_Str)
    {
        return p_Str?.Replace(Ctes.CHAR_CR, Ctes.CHAR_SPACE).Replace(Ctes.CHAR_LF, Ctes.CHAR_SPACE);
    }

    public static string ToCamelCase(this string p_Str)
    {
        if (string.IsNullOrEmpty(p_Str))
            return p_Str;
        return char.ToUpper(p_Str[0]) + p_Str.Substring(1);
    }

    public static string SingleToDoubleQuotes(this string p_Str)
    {
        return p_Str.Replace(Ctes.CHAR_SINGLE_QUOTE, Ctes.CHAR_DOUBLE_QUOTE);
    }

    public static double SecondsSinceEpoch(DateTime p_DateTime)
    {
        return (p_DateTime.ToUniversalTime() - DateTimeEpoch).TotalSeconds;
    }

    public static long MillisSinceEpoch(DateTime p_DateTime)
    {
        return (long)(p_DateTime.ToUniversalTime() - DateTimeEpoch).TotalMilliseconds;
    }

    public static DateTime MillisFromEpoch2DateTime(long p_Millis)
    {
        return DateTimeEpoch.AddMilliseconds(p_Millis);
    }

    public static DateTime SecondsFromEpoch2DateTime(int p_Seconds)
    {
        return DateTimeEpoch.AddSeconds(p_Seconds);
    }

    public static DateTime Epoch2DateTime(long p_Long)
    {
        // Check for what comes from .Net (JobStatus.Details.EndDate for instance). It's 1899-12-30.
        if (p_Long == -2209161600)
            return default;
        // For instance: 1422552023629 is for 2015-01-29T17:20:23.629
        return p_Long / 1400000000000 == 0 ? DateTimeEpoch.AddSeconds(p_Long) : DateTimeEpoch.AddMilliseconds(p_Long);
    }

    public static bool CanBeEpoch(string s, out bool hasMillis)
    {
        hasMillis = false;
        switch (s.Length) {
        case 10:
            return true;
        case 13:
            hasMillis = true;
            return true;
        }
        return false;
    }

    public static void GetEpochSeconds(string startStr, string endStr, out long start, out long end)
    {
        var now = (long)SecondsSinceEpoch(DateTime.UtcNow);
        start = startStr.StartsWith("now") ? now + SecondsFromTimeSpan(startStr.Substring(3)) : long.Parse(startStr);
        end = endStr.StartsWith("now") ? now + SecondsFromTimeSpan(endStr.Substring(3)) : long.Parse(endStr);
    }

    public static void GetDateTimes(string startStr, string endStr, out DateTime start, out DateTime end)
    {
        var now = DateTime.UtcNow;
        start = startStr.StartsWith("now") ? now + TimeSpan.FromSeconds(SecondsFromTimeSpan(startStr.Substring(3))) : Epoch2DateTime(long.Parse(startStr));
        end = endStr.StartsWith("now") ? now + TimeSpan.FromSeconds(SecondsFromTimeSpan(endStr.Substring(3))) : Epoch2DateTime(long.Parse(endStr));
    }

    public static int SecondsFromTimeSpan(string span)
    {
        if (span.Length == 0)
            return 0;
        var splitPos = span.Length - 1;
        var unit = span[splitPos];
        var number = span.Substring(0, splitPos);
        if (!int.TryParse(number, out var nb)) throw new DalException($"SecondsFromTimeSpan: Invalid span '{span}'.");
        return nb * unit switch
        {
            's' => 1,
            'm' => 60,
            'h' => 3600,
            'd' => 24 * 3600,
            'w' => 7 * 24 * 3600,
            _ => throw new DalException($"SecondsFromTimeSpan: Unknown span unit '{unit}'.")
        };
    }

    public static TimeSpan TimeSpanFromUtcNow(DateTime p_Dt)
    {
        return new TimeSpan(0, 0, (int)(DateTime.UtcNow - p_Dt).TotalSeconds);
    }

    public static DateTime FromEpoch(this object p_Obj)
    {
        return Epoch2DateTime(p_Obj is long l ? l : (p_Obj is double d ? (long)d : throw new DalException($"Can't convert {p_Obj} to epoch.")));
    }

    public static TimeSpan DropMillis(this TimeSpan p_Timespan)
    {
        return new TimeSpan(0, 0, (int)p_Timespan.TotalSeconds);
    }

    public static string LastFullNonWeekendDayAsString()
    {
        var dt = DateTime.UtcNow.AddDays(-1);
        if (dt.DayOfWeek == DayOfWeek.Sunday)
            dt = dt.AddDays(-1);
        if (dt.DayOfWeek == DayOfWeek.Saturday)
            dt = dt.AddDays(-1);
        return dt.ToString("yyyy-MM-dd");
    }

    public static string ToPrintableAscii(byte[] p_Bytes, int p_MaxNb = 2000)
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        foreach (byte b in p_Bytes) {
            if (++i == p_MaxNb) {
                break;
            }
            sb.Append(b < 127 && PRINTABLE_CHARS[b] == 1 ? (char)b : '.');
        }
        return sb.ToString();
    }

    public static string ToLocalIsoStringWithMillis(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
    
    public static string ToIsoWithMillis(this DateTime dt)
    {
        return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    public static string ToIsoNoMillis(this DateTime dt)
    {
        return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    public static object HexDump(byte[] data)
    {
        // http://blog.dezfowler.com/2010/11/pretty-print-hex-dump-in-linqpad.html
        return data
            .Select((b, i) => new {Byte = b, Index = i})
            .GroupBy(o => o.Index / 16)
            .Select(g =>
                g
                    .Aggregate(
                        new {Hex = new StringBuilder(), Chars = new StringBuilder()},
                        (a, o) =>
                        {
                            a.Hex.AppendFormat("{0:X2} ", o.Byte);
                            a.Chars.Append(Convert.ToChar(o.Byte));
                            return a;
                        },
                        a => new {Hex = a.Hex.ToString(), Chars = a.Chars.ToString()}
                    )
            )
            .ToList();
    }
    
    public static byte[] HexStringToByteArray(string hex)
    {
        var nbChars = hex.Length;
        var bytes = new byte[nbChars / 2];
        for (int i = 0; i < nbChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    public static void ChangeDateTimeDefaultFormat()
    {
        // Needed when reading from ES index settings ! Beats me.
        var newCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        newCulture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
        newCulture.DateTimeFormat.DateSeparator = "-";
        newCulture.DateTimeFormat.ShortTimePattern = "HH:mm";
        newCulture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
        System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
    }

    public static long ToGigs(this long p_Size)
    {
        var nb = ((double)(p_Size + (long)G_BYTES / 2) / G_BYTES);
        return (long)nb;
    }

    public static ulong ToGigs(this ulong p_Size)
    {
        var nb = ((double)(p_Size + G_BYTES / 2) / G_BYTES);
        return (ulong)nb;
    }

    public static ulong ToMegs(this ulong p_Size)
    {
        var nb = ((double)(p_Size + M_BYTES / 2) / M_BYTES);
        return (ulong)nb;
    }

    public static ulong ToKilos(this ulong p_Size)
    {
        var nb = ((double)(p_Size + K_BYTES / 2) / K_BYTES);
        return (ulong)nb;
    }

    public static string ToSizeString(this ulong p_Size)
    {
        var n = p_Size.ToGigs();
        if (n != 0) return n + "g";
        n = p_Size.ToMegs();
        if (n != 0) return n + "m";
        n = p_Size.ToKilos();
        if (n != 0) return n + "k";
        return p_Size + "b";
    }

    public static string JoinSingleQuoted(ICollection<string> p_Strings, string p_Sep = Ctes.SEP_COMMA_SPACE)
    {
        var sb = new StringBuilder();
        int i = 0;
        foreach (var str in p_Strings) {
            if (i++ != 0) sb.Append(p_Sep);
            sb.Append($"'{str}'");
        }
        return sb.ToString();
    }

    public static string GenerateExceptionReport(Exception p_Exception)
    {
        var sb = new StringBuilder();
        int level = 0;
        for (var exc = p_Exception; exc != null; exc = exc.InnerException) {
            if (level++ != 0)
                sb.AppendLine("Caused by:");
            sb.AppendLine($"Type: {exc.GetType().Name}");
            sb.AppendLine($"Source: {exc.Source}");
            sb.AppendLine($"Message: {exc.Message}");
            sb.AppendLine(exc.StackTrace);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public static string RestGet(string p_Url)
    {
        return ReadAllStreamAsUtf8String(WebRequest.Create(p_Url).GetResponse());
    }

    public static byte[] Compress(byte[] p_Bytes, bool p_AddPrefixAndSuffix = false)
    {
        var dstStream = new MemoryStream();
        //using (var zip = new GZipStream(dstStream, CompressionMode.Compress, true)) {
        using (var zip = new DeflateStream(dstStream, CompressionMode.Compress, true)) {
            zip.Write(p_Bytes, 0, p_Bytes.Length);
        }
        var zippedBytes = dstStream.ToArray();
        if (!p_AddPrefixAndSuffix)
            return zippedBytes;

        // Can't seem to find a way to add the prefix/suffix through standard libs.
        var newBytes = new byte[zippedBytes.Length + 6];
        Array.Copy(zippedBytes, 0, newBytes, 2, zippedBytes.Length);
        newBytes[0] = 0x78;
        newBytes[1] = 0xDA; // BestCompression
        var suf = newBytes.Length - 4;
        var adler32 = Adler32(p_Bytes); // Checksum the uncompressed bytes.
        newBytes[suf + 0] = (byte)((adler32 & 0xFF000000) >> 24);
        newBytes[suf + 1] = (byte)((adler32 & 0x00FF0000) >> 16);
        newBytes[suf + 2] = (byte)((adler32 & 0x0000FF00) >> 08);
        newBytes[suf + 3] = (byte)((adler32 & 0x000000FF));
        return newBytes;
    }

    public static byte[] UnGzip(byte[] zipped)
    {
        using var inMs = new MemoryStream(zipped);
        using var inStream = new GZipStream(inMs, CompressionMode.Decompress);
        using var outStream = new MemoryStream();
        inStream.CopyTo(outStream);
        return outStream.ToArray();
    }
    
    public static uint Adler32(byte[] bytes)
    {
        const int mod = 65521;
        uint a = 1, b = 0;
        foreach (byte c in bytes)
        {
            a = (a + c) % mod;
            b = (b + a) % mod;
        }
        return (b << 16) | a;
    }

    public static string ToJson(this bool b) => b ? Ctes.TRUE : Ctes.FALSE;

    public static string BlankIfZero(this long p_N)
    {
        return p_N == 0 ? "" : p_N.ToString();
    }

    public static string BlankIfZero(this uint p_N)
    {
        return p_N == 0 ? "" : p_N.ToString();
    }
    
    public static string DecryptBase64StringFromJava(string p_Encrypted)
    {
        return DecryptStringFromBytes_Aes(Convert.FromBase64String(p_Encrypted.Substring(24)),
                                          ENCODED_KEY,
                                          Convert.FromBase64String(p_Encrypted.Substring(0, 24)));
    }

    public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Create an Aes object with the specified key and IV.
        using (var aesAlg = Aes.Create()) {
            if (aesAlg == null) throw new DalException("Unexpected null returned by Aes.Create().");
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
                        // Read the decrypted bytes from the decrypting stream.
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
    
    public static bool IsBase64(string p_Str)
    {
        return /*(p_Str.Length % 4 == 0) && */BASE64_REGEX.IsMatch(p_Str);
    }

    public static string GetMd5(string str)
    {
        return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str)));
    }

    public static string GetSha1(string str)
    {
        return BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(str)));
    }
    
    public static List<List<T>> SplitList<T>(List<T> locations, int nSize)
    {
        var list = new List<List<T>>();
        for (int i = 0; i < locations.Count; i += nSize) {
            list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
        }
        return list;
    }

    public static bool CertificateValidationTrue(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
    
    public static IEnumerable<T[]> Chunk<T>(IEnumerable<T> items, int size)
    {
        var array = items as T[] ?? items.ToArray();
        for (int i = 0; i < array.Length; i += size) {
            var chunk = new T[Math.Min(size, array.Length - i)];
            Array.Copy(array, i, chunk, 0, chunk.Length);
            yield return chunk;
        }
    }

    public static string HtmlEncode(this string str)
    {
        return WebUtility.HtmlEncode(str);
    }
    
    // Split a collection in bucket containing a specified number of items.
    public static IEnumerable<IEnumerable<TSource>> Split<TSource>(this IEnumerable<TSource> source, int bucketSize)
    {
        var bucket = new List<TSource>();
        foreach (var item in source) {
            bucket.Add(item);
            if (bucket.Count == bucketSize) {
                yield return bucket;
                bucket = new List<TSource>();
            }
        }
        if (bucket.Count > 0) {
            yield return bucket;
        }
    }

    public static string QuoteAndJoin(IEnumerable<string> strings)
    {
        return string.Join(',', strings.Select(s => $"'{s}'"));
    }
}
