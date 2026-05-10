using Microsoft.AspNetCore.Http;

namespace EList.Common.Support
{
    public static class MimeTypeUtility
    {
        private const string DEFAULT_MIME_TYPE = "application/octet-stream";
        private const string DEFAULT_FILE_EXTENSION = "";

        public static string FileExtensionToMimeType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension));

            if (extension.StartsWith("."))
                extension = extension.Substring(1);

            var found = map.TryGetValue(extension.ToLower(), out var result);
            return found ? result.FirstOrDefault() : DEFAULT_MIME_TYPE;
        }

        public static string MimeTypeToFileExtension(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentNullException(nameof(mimeType));

            var result = map.FirstOrDefault(x => x.Value.Contains(mimeType.ToLower())).Key;
            return result ?? DEFAULT_FILE_EXTENSION;
        }

        private static string GetMimeTypeFromSignature(byte[] fileHeader)
        {
            if (fileHeader == null || fileHeader.Length < 12)
                return "application/octet-stream";

            // ========== ИЗОБРАЖЕНИЯ ==========

            // JPEG
            if (fileHeader.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
                return "image/jpeg";

            // PNG
            if (fileHeader.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                return "image/png";

            // GIF
            if (fileHeader.Take(6).SequenceEqual(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }) ||
                fileHeader.Take(6).SequenceEqual(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }))
                return "image/gif";

            // BMP
            if (fileHeader.Take(2).SequenceEqual(new byte[] { 0x42, 0x4D }))
                return "image/bmp";

            // WEBP
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) &&
                fileHeader.Length >= 12 &&
                fileHeader.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 }))
                return "image/webp";

            // TIFF
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x49, 0x49, 0x2A, 0x00 }) ||
                fileHeader.Take(4).SequenceEqual(new byte[] { 0x4D, 0x4D, 0x00, 0x2A }))
                return "image/tiff";

            // ICO
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x00, 0x00, 0x01, 0x00 }))
                return "image/x-icon";

            // HEIC / HEIF
            if (fileHeader.Length >= 12 &&
                fileHeader.Skip(4).Take(4).SequenceEqual(new byte[] { 0x68, 0x65, 0x69, 0x63 }))
                return "image/heic";

            // ========== ВИДЕО (ПРАВИЛЬНАЯ ВЕРСИЯ) ==========

            // Проверяем наличие атома ftyp (это ключевая сигнатура для MP4-based контейнеров)
            if (fileHeader.Skip(4).Take(4).SequenceEqual(new byte[] { 0x66, 0x74, 0x79, 0x70 })) // 'ftyp'
            {
                if (fileHeader.Length < 12)
                    return "video/mp4"; // Базовый MP4, если не хватает данных для уточнения

                // Читаем brand (тип контейнера)
                string brand = System.Text.Encoding.ASCII.GetString(fileHeader, 8, 4);

                // Очищаем brand от null-символов и пробелов
                brand = brand.Trim('\0', ' ');

                switch (brand)
                {
                    // QuickTime / MOV
                    case "qt":
                    case "qt ":
                    case "QTM":
                        return "video/quicktime";

                    // M4V (Apple видео)
                    case "M4V":
                    case "M4V ":
                        return "video/x-m4v";

                    // MP4 варианты
                    case "mp41":
                    case "mp42":
                    case "isom":
                    case "avc1":
                        return "video/mp4";

                    // 3GP
                    case "3gp4":
                    case "3gp5":
                    case "3gp6":
                        return "video/3gpp";

                    // Другие
                    default:
                        return "video/mp4"; // По умолчанию считаем MP4
                }
            }

            // AVI
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) && // 'RIFF'
                fileHeader.Length >= 12 &&
                fileHeader.Skip(8).Take(4).SequenceEqual(new byte[] { 0x41, 0x56, 0x49, 0x20 })) // 'AVI '
                return "video/x-msvideo";

            // Matroska / WebM
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }))
            {
                // Можно попробовать определить WebM по наличию "webm" в метаданных
                // Для простоты возвращаем Matroska, это безопасно
                return "video/x-matroska";
            }

            // OGG
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x4F, 0x67, 0x67, 0x53 }))
                return "video/ogg";

            // FLV
            if (fileHeader.Take(3).SequenceEqual(new byte[] { 0x46, 0x4C, 0x56 }))
                return "video/x-flv";

            // MPEG Program Stream
            if (fileHeader.Take(4).SequenceEqual(new byte[] { 0x00, 0x00, 0x01, 0xBA }))
                return "video/mpeg";

            // WMV / ASF
            if (fileHeader.Length >= 16 &&
                fileHeader.Take(16).SequenceEqual(new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C }))
                return "video/x-ms-wmv";

            return "application/octet-stream";
        }

        public static async Task<string> GetMimeTypeFromFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "application/octet-stream";

            // Читаем достаточно байт для определения всех форматов (максимум 16 байт)
            byte[] header = new byte[16];
            using (var stream = file.OpenReadStream())
            {
                await stream.ReadAsync(header, 0, 16);
            }

            return GetMimeTypeFromSignatureWithDebug(header);
        }

        public static async Task<string> GetMimeTypeFromFileAsync(Stream fileStream)
        {
            if (fileStream == null || fileStream.Length == 0)
                return "application/octet-stream";

            // Читаем достаточно байт для определения всех форматов (максимум 16 байт)
            byte[] header = new byte[16];
            fileStream.Position = 0;
            await fileStream.ReadAsync(header, 0, 16);

            return GetMimeTypeFromSignatureWithDebug(header);
        }

        public static string GetMimeTypeFromSignatureWithDebug(byte[] fileHeader)
        {
            Console.WriteLine("File header bytes: " + BitConverter.ToString(fileHeader));
            Console.WriteLine("ASCII preview: " + System.Text.Encoding.ASCII.GetString(fileHeader.Take(12).ToArray()).Replace("\0", "\\0"));

            if (fileHeader.Length >= 8)
            {
                bool isFtyp = fileHeader.Skip(4).Take(4).SequenceEqual(new byte[] { 0x66, 0x74, 0x79, 0x70 });
                Console.WriteLine($"Has 'ftyp' at position 4: {isFtyp}");

                if (isFtyp && fileHeader.Length >= 12)
                {
                    string brand = System.Text.Encoding.ASCII.GetString(fileHeader, 8, 4);
                    Console.WriteLine($"Brand: '{brand}' (bytes: {BitConverter.ToString(fileHeader.Skip(8).Take(4).ToArray())})");
                }
            }

            return GetMimeTypeFromSignature(fileHeader);
        }

        public static bool IsImage(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return false;

            return mimeType.StartsWith("image/");
        }

        public static bool IsVideo(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return false;

            return mimeType.StartsWith("video/");
        }

        public static string GetCategory(string mimeType)
        {
            if (IsImage(mimeType)) return "image";
            if (IsVideo(mimeType)) return "video";
            return "other";
        }



        // TODO: Add more file extensions if need

        private static readonly Dictionary<string, string[]> map = new Dictionary<string, string[]>
        {
            { "aac",    new []{"audio/aac"} },
            { "abw",    new []{"application/x-abiword"}  },
            { "arc",    new []{"application/x-freearc"}  },
            { "azw",    new []{"application/vnd.amazon.ebook"}  },
            { "bin",    new []{"application/octet-stream"}  },
            { "bmp",    new []{"image/bmp"}  },
            { "bz",     new []{"application/x-bzip"}  },
            { "bz2",    new []{"application/x-bzip2"}  },
            { "csh",    new []{"application/x-csh"}  },
            { "css",    new []{"text/css"}  },
            { "csv",    new []{"text/csv"}  },
            { "doc",    new []{"application/msword"}  },
            { "docx",   new []{"application/vnd.openxmlformats-officedocument.wordprocessingml.document"}  },
            { "eot",    new []{"application/vnd.ms-fontobject"}  },
            { "epub",   new []{"application/epub+zip"}  },
            { "gz",     new []{"application/gzip"}  },
            { "gif",    new []{"image/gif"}  },
            { "html",   new []{"text/html"}  },
            { "ico",    new []{"image/vnd.microsoft.icon"}  },
            { "ics",    new []{"text/calendar"}  },
            { "jar",    new []{"application/java-archive"}  },
            { "jpg",    new []{"image/jpeg"}  },
            { "js",     new []{"text/javascript"}  },
            { "json",   new []{"application/json"}  },
            { "jsonld", new []{"application/ld+json"}  },
            { "mid",    new []{"audio/midi audio/x-midi"}  },
            { "mjs",    new []{"text/javascript"}  },
            { "mp3",    new []{"audio/mpeg"}  },
            { "mp4",    new []{"video/mp4"}  },
            { "mpeg",   new []{"video/mpeg"}  },
            { "mpkg",   new []{"application/vnd.apple.installer+xml"}  },
            { "odp",    new []{"application/vnd.oasis.opendocument.presentation"}  },
            { "ods",    new []{"application/vnd.oasis.opendocument.spreadsheet"}  },
            { "odt",    new []{"application/vnd.oasis.opendocument.text"}  },
            { "oga",    new []{"audio/ogg"}  },
            { "ogv",    new []{"video/ogg"}  },
            { "ogx",    new []{"application/ogg"}  },
            { "opus",   new []{"audio/opus"}  },
            { "otf",    new []{"font/otf"}  },
            { "png",    new []{"image/png"}  },
            { "pdf",    new []{"application/pdf", "application/pdf-converted-uml", "application/pdf-converted-html", "application/pdf-converted-plain", "application/pdf-converted-xml"}  },
            { "php",    new []{"application/php"}  },
            { "ppt",    new []{"application/vnd.ms-powerpoint"}  },
            { "pptx",   new []{"application/vnd.openxmlformats-officedocument.presentationml.presentation"}  },
            { "rar",    new []{"application/vnd.rar"}  },
            { "rtf",    new []{"application/rtf"}  },
            { "sh",     new []{"application/x-sh"}  },
            { "svg",    new []{"image/svg+xml"}  },
            { "swf",    new []{"application/x-shockwave-flash"}  },
            { "tar",    new []{"application/x-tar"}  },
            { "tiff",   new []{"image/tiff"}  },
            { "ts",     new []{"video/mp2t"} },
            { "ttf",    new []{"font/ttf"}  },
            { "txt",    new []{"text/plain"}  },
            { "vsd",    new []{"application/vnd.visio"}  },
            { "wav",    new []{"audio/wav"}  },
            { "weba",   new []{"audio/webm"}  },
            { "webm",   new []{"video/webm"}  },
            { "webp",   new []{"image/webp"}  },
            { "woff",   new []{"font/woff"}  },
            { "woff2",  new []{"font/woff2"}  },
            { "xhtml",  new []{"application/xhtml+xml"}  },
            { "xls",    new []{"application/vnd.ms-excel"}  },
            { "xlsx",   new []{"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}  },
            { "xml",    new []{"application/xml", "text/xml"}  },
            { "xul",    new []{"application/vnd.mozilla.xul+xml"}  },
            { "zip",    new []{"application/zip"}  },
            { "3gp",    new []{"video/3gpp"}  },
            { "7z",     new []{"application/x-7z-compressed"}  },
            { "sig",    new []{"application/x-pkcs7-signature", "application/x-pkcs7-practitioner", "application/x-pkcs7-organization"} },
            { "adts",   new []{"audio/vnd.dlna.adts"} },
            { "aiff",   new []{"audio/aiff"} },
            { "avi",    new []{"video/avi"} },
            { "docm",   new []{"application/vnd.ms-word.document.macroEnabled.12"} },
            { "m4a",    new []{"audio/x-m4a"} },
            { "mov",    new []{"video/quicktime"} },
            { "ppsx",   new []{"application/vnd.openxmlformats-officedocument.presentationml.slideshow"} },
            { "wma",    new []{"audio/x-ms-wma"} },
            { "wmd",    new []{"application/x-ms-wmd"} },
            { "wmv",    new []{"video/x-ms-wmv"} },
            { "wmz",    new []{"application/x-ms-wmz"} },
            { "xltm",   new []{"application/vnd.ms-excel.template.macroEnabled.12"} },
            { "xltx",   new []{"application/vnd.openxmlformats-officedocument.spreadsheetml.template"} }
        };
    }
}
