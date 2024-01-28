using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DownloadOpenVR
{
    public class DownloadOpenVR : Task
    {
        [Required]
        public string DestinationFolder { get; set; }

        [Required]
        public string Version { get; set; }

        public override bool Execute()
        {
            bool success = true;
            using HttpClient client = new();

            string root = $"https://cdn.jsdelivr.net/gh/ValveSoftware/openvr@refs/tags/v{Version}";

            string licenseText = string.Join("\n", DownloadString(client, root, "LICENSE")?.Split('\n').Select(l => $"/// {l}"));

            if (string.IsNullOrWhiteSpace(licenseText))
            {
                Log.LogError("License text could not be downloaded");
                success = false;
            }

            success &= DownloadFile(client, root, "headers/openvr_api.cs", Path.Combine(DestinationFolder, "openvr_api.cs"), $"/// <copyright file=\"openvr_api.cs\" company=\"Valve Software\">\n{licenseText}\n/// </copyright>\n/// <auto-generated />\n");
            success &= DownloadFile(client, root, "bin/win64/openvr_api.dll", Path.Combine(DestinationFolder, "Native", "openvr_api.dll"));

            return success;
        }

        private HttpResponseMessage DownloadData(HttpClient client, string root, string path)
        {
            HttpResponseMessage response = client.GetAsync($"{root}/{path}").GetAwaiter().GetResult();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Log.LogError($"File '{path}' not found for version {Version}");
                return null;
            }

            response.EnsureSuccessStatusCode();

            return response;
        }

        private string DownloadString(HttpClient client, string root, string path)
        {
            using HttpResponseMessage response = DownloadData(client, root, path);
            using Stream stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            if (stream == null)
            {
                return null;
            }

            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }

        private bool DownloadFile(HttpClient client, string root, string path, string destinationFile, string prepend = null)
        {
            using HttpResponseMessage response = DownloadData(client, root, path);
            using Stream stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            if (stream == null)
            {
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));

            using FileStream fileStream = File.OpenWrite(destinationFile);

            if (prepend != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(prepend);
                fileStream.Write(data, 0, data.Length);
            }

            stream.CopyTo(fileStream);

            return true;
        }
    }
}