using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace GitHashes
{
    class EntityChange
    {
        public string Path { get; }
        public string GithubPath { get; }
        public EntityTypes Type { get; }
        public WatcherChangeTypes WatcherChangeType { get; }

        public EntityChange(string path, string githubPath, EntityTypes type, WatcherChangeTypes watcherChangeType)
        {
            Path = path;
            GithubPath = githubPath;
            Type = type;
            WatcherChangeType = watcherChangeType;
        }

        public static void DownloadFile(string path, string url, string token = null)
        {
            Hashes.SendRequest(url, (response, s) =>
            {
                using (FileStream fs = File.Create(path))
                {
                    s.CopyTo(fs);
                }
            }, token, "application/vnd.github.VERSION.raw");
        }

        public static void DownloadDirectory(string path, string url, string token = null)
        {
            PinkJson.Json json = null;
            Hashes.SendRequest(url, (response, s) =>
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    json = new PinkJson.Json(sr.ReadToEnd());
                    Hashes.CheckErrors(json, response.Headers["X-RateLimit-Reset"]);
                }
            }, token, "application/vnd.github.VERSION.raw");

            Directory.CreateDirectory(path);

            foreach (var obj in json["tree"].Get<PinkJson.JsonArray>())
            {
                var pth = System.IO.Path.Combine(path, obj["path"].Get<string>());
                if (obj["mode"].Get<string>() == "040000")
                    DownloadDirectory(pth, obj["url"].Get<string>(), token);
                else if (obj["mode"].Get<string>() == "100644")
                    DownloadFile(pth, obj["url"].Get<string>(), token);
            }
        }

        public void Sync(string token = null)
        {
            switch (WatcherChangeType)
            {
                case WatcherChangeTypes.Created:
                    if (Type == EntityTypes.Directory)
                        Directory.Delete(Path, true);
                    else if (Type == EntityTypes.File)
                        File.Delete(Path);
                    break;
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Deleted:
                    if (Type == EntityTypes.File)
                        DownloadFile(Path, GithubPath, token);
                    else if (Type == EntityTypes.Directory)
                        DownloadDirectory(Path, GithubPath, token);
                    break;
            }
        }

        public override string ToString()
        {
            var str = "";
            switch (WatcherChangeType)
            {
                case WatcherChangeTypes.Created:
                    str = "[ + ]";
                    break;
                case WatcherChangeTypes.Changed:
                    str = "[ ~ ]";
                    break;
                case WatcherChangeTypes.Deleted:
                    str = "[ - ]";
                    break;
            }
            return $"{str} {Path} {GithubPath}";
        }
    }

    enum EntityTypes
    {
        Directory,
        File
    }
}
