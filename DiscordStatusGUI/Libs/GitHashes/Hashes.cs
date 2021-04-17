using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Net;

namespace GitHashes
{
    class Hashes
    {
        public static List<EntityChange> CompareWithGithub(string path, string user, string repositoryName, string treeHash, string type, string token = null, string[] exclusion = null)
        {
            PinkJson.Json json = null;
            string XRateLimitReset = null;
            List<EntityChange> changes = new List<EntityChange>();

            if (exclusion != null)
                for (var i = 0; i < exclusion.Length; i++)
                    exclusion[i] = Path.GetFullPath(exclusion[i]);

            SendRequest($"https://api.github.com/repos/{user}/{repositoryName}/git/{type}/{treeHash}", (response, s) =>
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    json = new PinkJson.Json(sr.ReadToEnd());
                    XRateLimitReset = response.Headers["X-RateLimit-Reset"];
                }
            }, token);

            CheckErrors(json, XRateLimitReset);

            if (json.IndexByKey("tree") != -1)
            {
                var files = Directory.GetFiles(path).ToList();
                var directories = Directory.GetDirectories(path).ToList();

                foreach (var gitfile in json["tree"].Get<PinkJson.JsonArray>())
                {
                    var gitpath = gitfile["path"].Get<string>();
                    if (exclusion != null && exclusion.FirstOrDefault(ex => ex.Equals(Path.GetFullPath(Path.Combine(path, gitpath)), StringComparison.OrdinalIgnoreCase)) != default)
                        continue;

                    var gitmode = gitfile["mode"].Get<string>();
                    var giturl = gitfile["url"].Get<string>();

                    if (gitmode == "040000")
                    {
                        var index = directories.FindIndex(dir => gitpath == Path.GetFileName(dir));
                        if (index == -1)
                            changes.Add(new EntityChange(Path.Combine(path, gitpath), giturl, EntityTypes.Directory, WatcherChangeTypes.Deleted));
                        else
                        {
                            changes.AddRange(CompareWithGithub(directories[index], user, repositoryName, gitfile["sha"].Get<string>(), "trees", token));
                            directories.RemoveAt(index);
                        }
                    } else if (gitmode == "100644")
                    {
                        var index = files.FindIndex(file => gitpath == Path.GetFileName(file));
                        if (index == -1)
                            changes.Add(new EntityChange(Path.Combine(path, gitpath), giturl, EntityTypes.File, WatcherChangeTypes.Deleted));
                        else
                        {
                            var hash = GetString(FileHash(files[index]));
                            if (hash != gitfile["sha"].Get<string>())
                                changes.Add(new EntityChange(files[index], giturl, EntityTypes.File, WatcherChangeTypes.Changed));
                            files.RemoveAt(index);
                        }
                    } else
                    {
                        throw new Exception("Unknown mode: " + gitmode);
                    }
                }
                foreach (var file in files)
                    changes.Add(new EntityChange(file, null, EntityTypes.File, WatcherChangeTypes.Created));
                foreach (var dir in directories)
                    changes.Add(new EntityChange(dir, null, EntityTypes.Directory, WatcherChangeTypes.Created));
            } else
            {
                if (File.Exists(path) && json.IndexByKey("message") == -1)
                {
                    var hash = GetString(FileHash(path));
                    if (hash != json["sha"].Get<string>())
                        changes.Add(new EntityChange(path, json["url"].Get<string>(), EntityTypes.File, WatcherChangeTypes.Changed));
                }
                else if (!File.Exists(path) && json.IndexByKey("message") != -1)
                    CheckErrors(json, XRateLimitReset);
                else if (!File.Exists(path))
                    changes.Add(new EntityChange(path, json["url"].Get<string>(), EntityTypes.File, WatcherChangeTypes.Deleted));
                else if (json.IndexByKey("message") != -1)
                    changes.Add(new EntityChange(path, null, EntityTypes.File, WatcherChangeTypes.Created));
            }

            return changes;
        }

        public static string GetHashByPath(string user, string repositoryName, string[] parts, string token = null)
        {
            string treeUrl = null;
            string sha = null;
            SendRequest($"https://api.github.com/repos/{user}/{repositoryName}/branches/{parts[0]}", (response, s) =>
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    var json = new PinkJson.Json(sr.ReadToEnd());
                    CheckErrors(json, response.Headers["X-RateLimit-Reset"]);
                    treeUrl = json["commit"]["commit"]["tree"]["url"].Get<string>();
                    sha = json["commit"]["commit"]["tree"]["sha"].Get<string>();
                }
            }, token);
            for (var i = 1; i < parts.Length; i++)
            {
                SendRequest(treeUrl, (response, s) =>
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        var json = new PinkJson.Json(sr.ReadToEnd());
                        CheckErrors(json, response.Headers["X-RateLimit-Reset"]);
                        foreach (var obj in json["tree"].Get<PinkJson.JsonArray>())
                            if (obj["path"].Get<string>() == parts[i])
                            {
                                treeUrl = obj["url"].Get<string>();
                                sha = obj["sha"].Get<string>();
                                break;
                            }
                    }
                }, token);
            }
            return sha;
        }

        public static byte[] DirectoryHash(string path)
        {
            var input = new List<byte>();

            foreach (var file in Directory.GetFiles(path))
                input.AddRange(Encoding.UTF8.GetBytes("100644 " + Path.GetFileName(file) + "\0").Concat(FileHash(file)));
            foreach (var directory in Directory.GetDirectories(path))
                // FIX ME
                input.AddRange(Encoding.UTF8.GetBytes("040000 " + Path.GetFileName(directory) + "\0").Concat(DirectoryHash(directory)));

            return Sha1(Encoding.UTF8.GetBytes("tree " + input.LongCount() + "\0").Concat(input).ToArray());
        }

        public static byte[] FileHash(string path)
        {
            using (StreamFormatReader sfr = new StreamFormatReader(path))
                return Sha1(sfr);
            // return ContentHash(File.ReadAllBytes(path));
        }

        public static byte[] ContentHash(byte[] bytes)
        {
            bytes = bytes.Where(b => b != 13).ToArray();
            return Sha1(Encoding.UTF8.GetBytes("blob " + bytes.LongCount() + "\0").Concat(bytes).ToArray());
        }

        public static string GetString(byte[] hash)
        {
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        public static byte[] Sha1(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
                return sha1.ComputeHash(input);
        }

        public static byte[] Sha1(Stream stream)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
                return sha1.ComputeHash(stream);
        }

        public static void CheckErrors(PinkJson.Json json, string unix)
        {
            if (json.IndexByKey("message") != -1)
            {
                if (json["message"].Get<string>().Contains("rate limit"))
                    throw new Exception("Rate limit, reset in " + FromUNIX(unix));
                else
                    throw new Exception(json["message"].Get<string>());
            }
        }

        private static DateTime FromUNIX(string unix)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(long.Parse(unix)).ToLocalTime();
            return dtDateTime;
        }

        public static void SendRequest(string url, Action<WebResponse, Stream> onresponse, string token = null, string accept = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.118 Safari/537.36";
            if (token != null)
                request.Headers.Add("Authorization", "token " + token);
            if (accept != null)
                request.Accept = accept;
            using (WebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream s = response.GetResponseStream())
            {
                onresponse(response, s);
            }
        }
    }
}
