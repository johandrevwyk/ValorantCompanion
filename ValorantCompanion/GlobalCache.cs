using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public static class GlobalCache
    {
        public static async Task<JsonDocument> LoadJsonWithCacheAsync(string url, string cacheFolder)
        {
            string hash = GetMd5Hash(url);
            string cachePath = Path.Combine(cacheFolder, hash + ".json");

            if (File.Exists(cachePath))
            {
                // Load JSON from cache
                string cachedJson = await File.ReadAllTextAsync(cachePath);
                return JsonDocument.Parse(cachedJson);
            }

            // Download JSON and cache it
            using var http = new HttpClient();
            string json = await http.GetStringAsync(url);
            await File.WriteAllTextAsync(cachePath, json);
            return JsonDocument.Parse(json);
        }

        public static async Task<Image> LoadImageWithCacheAsync(string url, string cacheFolder)
        {
            using var http = new HttpClient();

            // Create a hashed filename for the URL
            string hash = GetMd5Hash(url);
            string cachePath = Path.Combine(cacheFolder, hash + ".png");

            // If image is cached, load it directly
            if (File.Exists(cachePath))
            {
                return Image.FromFile(cachePath);
            }

            // Otherwise, download and cache it
            var bytes = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(cachePath, bytes);

            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        public static string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public class ValorantApiResponse
    {
        public List<Weapon>? Data { get; set; }
    }

    public class Weapon
    {
        public string Uuid { get; set; }
        public string DisplayName { get; set; }
        public string DefaultSkinUuid { get; set; }
        public string DisplayIcon { get; set; }
        public List<Skin> Skins { get; set; }
    }

    public class Skin
    {
        public string Uuid { get; set; }
        public string DisplayName { get; set; }
        public string DisplayIcon { get; set; }
    }
}
