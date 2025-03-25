using System.IO;
using System.Threading.Tasks;

namespace ParkIRC.Extensions
{
    public static class FileExtensions
    {
        public static async Task<string> ReadAllTextAsync(this FileInfo file)
        {
            using (var reader = file.OpenText())
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task WriteAllTextAsync(this FileInfo file, string content)
        {
            using (var writer = file.CreateText())
            {
                await writer.WriteAsync(content);
            }
        }

        public static async Task AppendAllTextAsync(this FileInfo file, string content)
        {
            using (var writer = file.AppendText())
            {
                await writer.WriteAsync(content);
            }
        }

        public static string GetFileSizeString(this FileInfo file)
        {
            if (!file.Exists)
                return "0 B";

            var size = file.Length;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = size;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public static string GetMimeType(this FileInfo file)
        {
            var extension = file.Extension.ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }

        public static bool IsImage(this FileInfo file)
        {
            var mimeType = file.GetMimeType();
            return mimeType.StartsWith("image/");
        }

        public static bool IsDocument(this FileInfo file)
        {
            var mimeType = file.GetMimeType();
            return mimeType.StartsWith("application/") && 
                   (mimeType.Contains("word") || 
                    mimeType.Contains("excel") || 
                    mimeType.Contains("pdf"));
        }

        public static bool IsArchive(this FileInfo file)
        {
            var mimeType = file.GetMimeType();
            return mimeType.Contains("zip") || mimeType.Contains("rar");
        }

        public static string GetRelativePath(this FileInfo file, DirectoryInfo baseDirectory)
        {
            var fullPath = file.FullName;
            var basePath = baseDirectory.FullName;
            return fullPath.Substring(basePath.Length + 1);
        }

        public static async Task<bool> TryDeleteAsync(this FileInfo file)
        {
            try
            {
                if (file.Exists)
                {
                    file.Delete();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryMoveAsync(this FileInfo file, string destinationPath)
        {
            try
            {
                if (file.Exists)
                {
                    file.MoveTo(destinationPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryCopyAsync(this FileInfo file, string destinationPath)
        {
            try
            {
                if (file.Exists)
                {
                    file.CopyTo(destinationPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
} 