﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace HDT.Plugins.Advisor.Services
{
    public class Github
    {
        // Check if there is a newer release on Github than current
        public static async Task<GithubRelease> CheckForUpdate(string user, string repo, Version version)
        {
            try
            {
                var latest = await GetLatestRelease(user, repo);

                // tag needs to be in strict version format: e.g. 0.0.0
                var v = new Version(latest.TagName.TrimStart('v'));

                // check if latest is newer than current
                if (v.CompareTo(version) > 0)
                {
                    return latest;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Advisor.Notify($"{repo}: Plugin update check failed", ex.Message, 15, "error");
            }

            return null;
        }

        // Use the Github API to get the latest release for a repo
        public static async Task<GithubRelease> GetLatestRelease(string user, string repo)
        {
            var url = $"https://api.github.com/repos/{user}/{repo}/releases";

            string json;
            using (var wc = new WebClient())
            {
                // API requires user-agent string, user name or app name preferred
                wc.Headers.Add(HttpRequestHeader.UserAgent, user);
                json = await wc.DownloadStringTaskAsync(url);
            }

            var releases = JsonConvert.DeserializeObject<List<GithubRelease>>(json);

            return releases.FirstOrDefault();
        }

        // Basic release info for JSON deserialization
        public class GithubRelease
        {
            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }

            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("prerelease")]
            public string Prerelease { get; set; }

            [JsonProperty("published_at")]
            public string PublishedAt { get; set; }
        }
    }
}