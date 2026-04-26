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
