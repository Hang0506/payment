using System;
using System.Linq;

namespace FRTTMO.PaymentCore
{
    public static class StringExtension
    {
        private static string[] _typeAppAcceptExts = { "apng", "avif", "gif", "jpg", "jpeg", "jfif", "pjpeg", "pjp", "png", "svg", "webp", "bmp", "ico", "cur", "tif", "tiff", "pdf", "doc", "docx" };

        private static string[] _mimeImageTypeAppAccept = { "image/apng", "image/avif", "image/gif", "image/jpeg", "image/png", "image/svg+xml", "image/webp", "image/bmp", "image/x-icon", "image/tiff" };
        private static string[] _mimeDocTypeAppAccept = { "application/msword", "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        public static bool IsContentTypeAppAccept(this string contentType)
        {
            var str = contentType.Trim().ToLower();
            return _mimeImageTypeAppAccept.Contains(str) || _mimeDocTypeAppAccept.Contains(str);
        }

        public static bool HasExtensionFileAppAccept(this string fileName)
        {
            if(string.IsNullOrEmpty(fileName)) return false;
            var arr = fileName.Split('.');
            if(arr.Length < 2) return false;
            return _typeAppAcceptExts.Contains(arr[^1].Trim().ToLower());
        }
    }
}
