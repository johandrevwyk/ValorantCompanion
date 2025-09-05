using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public static class GlobalCache
    {
        // Folder where all cached files will be stored
        private static readonly string CachePath = Path.Combine(Application.StartupPath, "Cache");

        /// <summary>
        /// Ensures that the cache folder exists.
        /// </summary>
        private static void EnsureCacheFolder()
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }

        /// <summary>
        /// Returns a cached image if available, otherwise downloads and caches it.
        /// </summary>
        public static async Task<Image> GetCachedImageAsync(string url)
        {
            try
            {
                EnsureCacheFolder();

                // Generate a safe filename from the URL
                string fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                string filePath = Path.Combine(CachePath, fileName);

                // If already cached, load it from disk
                if (File.Exists(filePath))
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fs);
                    }
                }

                // Otherwise, download and cache
                using var http = new HttpClient();
                var data = await http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(filePath, data);

                using (var ms = new MemoryStream(data))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cache Error] Failed to load image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns cached JSON data if available, otherwise downloads and caches it.
        /// </summary>
        public static async Task<string> GetCachedJsonAsync(string url, string cacheFileName, int expirationDays = 7)
        {
            try
            {
                EnsureCacheFolder();
                string filePath = Path.Combine(CachePath, cacheFileName);

                // If cache exists and is not expired, load from disk
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    if ((DateTime.Now - fileInfo.LastWriteTime).TotalDays < expirationDays)
                    {
                        return await File.ReadAllTextAsync(filePath);
                    }
                }

                // Otherwise, download fresh data
                using var http = new HttpClient();
                string json = await http.GetStringAsync(url);

                await File.WriteAllTextAsync(filePath, json);
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cache Error] Failed to load JSON: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
