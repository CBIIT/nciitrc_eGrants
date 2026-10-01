using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;
using Xunit.Abstractions;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Auto-discovers static image/asset references from the application's Razor views and
    /// CSS files, then requests each one over HTTP against the running app and asserts a
    /// 200 OK. This confirms the earlier 404 remediation (missing images such as the
    /// database-driven db_*.jpg dashboard icons and previously broken ~/images paths) and
    /// catches any future broken static asset reference automatically.
    /// </summary>
    [Collection(SmokeTestCollection.Name)]
    public class StaticAssetSmokeTests
    {
        private readonly SmokeTestWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;

        public StaticAssetSmokeTests(ITestOutputHelper output)
        {
            // Reuse the single process-wide host. AddSystemWebAdapters() registers a
            // process-global hosting environment that only permits one host per process,
            // so every smoke test must share the same factory instance.
            _factory = SmokeTestHost.Factory;
            _output = output;
        }

        [Theory]
        [MemberData(nameof(DiscoverAssetReferences))]
        public async Task Referenced_Static_Asset_Returns_Ok(string url)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            HttpResponseMessage response = await client.GetAsync(url);

            _output.WriteLine($"GET {url} -> {(int)response.StatusCode} {response.StatusCode}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static IEnumerable<object[]> DiscoverAssetReferences()
        {
            var projectDir = FindProjectDirectory();
            if (projectDir is null)
            {
                // No project directory found: yield nothing so the theory reports no cases
                // rather than throwing during discovery.
                yield break;
            }

            var urls = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            // Scan Razor views for image references such as src="~/images/foo.jpg".
            var viewsDir = Path.Combine(projectDir, "Views");
            if (Directory.Exists(viewsDir))
            {
                foreach (var file in Directory.EnumerateFiles(viewsDir, "*.cshtml", SearchOption.AllDirectories))
                {
                    foreach (var url in ExtractMarkupAssetReferences(StripComments(File.ReadAllText(file))))
                    {
                        urls.Add(url);
                    }
                }
            }

            // Scan CSS files (excluding third-party lib) for url(...) references.
            var cssDir = Path.Combine(projectDir, "wwwroot", "css");
            if (Directory.Exists(cssDir))
            {
                foreach (var file in Directory.EnumerateFiles(cssDir, "*.css", SearchOption.AllDirectories))
                {
                    var content = File.ReadAllText(file);
                    var fileDirRelative = GetUrlDirectory(projectDir, file);
                    foreach (var url in ExtractCssAssetReferences(StripComments(content), fileDirRelative))
                    {
                        urls.Add(url);
                    }
                }
            }

            foreach (var url in urls)
            {
                yield return new object[] { url };
            }
        }

        // Removes commented-out markup/script so that dead references (which do not
        // execute at runtime) are not treated as live static assets to verify. Handles
        // Razor (@*...*@), HTML (<!--...-->), CSS/JS block (/*...*/), and JS line (//...)
        // comments.
        private static readonly Regex RazorCommentRegex = new(
            "@\\*.*?\\*@", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex HtmlCommentRegex = new(
            "<!--.*?-->", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex BlockCommentRegex = new(
            "/\\*.*?\\*/", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex LineCommentRegex = new(
            "(?<!:)//[^\r\n]*", RegexOptions.Compiled);

        private static string StripComments(string content)
        {
            content = RazorCommentRegex.Replace(content, string.Empty);
            content = HtmlCommentRegex.Replace(content, string.Empty);
            content = BlockCommentRegex.Replace(content, string.Empty);
            content = LineCommentRegex.Replace(content, string.Empty);
            return content;
        }

        // Matches src/href attribute values that reference images, e.g.
        //   src="~/images/db_gpmats.jpg"  or  href="/images/logo.png"
        private static readonly Regex MarkupAssetRegex = new(
            "(?:src|href)\\s*=\\s*[\"']([^\"']+?\\.(?:jpg|jpeg|png|gif|svg|ico|bmp|tif|tiff|webp))[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Matches CSS url(...) references, e.g. url('images/tabrightJ.gif').
        private static readonly Regex CssUrlRegex = new(
            "url\\(\\s*['\"]?([^'\")]+?\\.(?:jpg|jpeg|png|gif|svg|ico|bmp|tif|tiff|webp))['\"]?\\s*\\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static IEnumerable<string> ExtractMarkupAssetReferences(string content)
        {
            foreach (Match match in MarkupAssetRegex.Matches(content))
            {
                var raw = match.Groups[1].Value.Trim();
                var url = NormalizeToRootRelative(raw);
                if (url is not null)
                {
                    yield return url;
                }
            }
        }

        private static IEnumerable<string> ExtractCssAssetReferences(string content, string cssUrlDirectory)
        {
            foreach (Match match in CssUrlRegex.Matches(content))
            {
                var raw = match.Groups[1].Value.Trim();
                var url = NormalizeToRootRelative(raw, cssUrlDirectory);
                if (url is not null)
                {
                    yield return url;
                }
            }
        }

        /// <summary>
        /// Converts a discovered reference into a root-relative URL that the static file
        /// middleware can serve, or returns null for references that cannot be verified
        /// (absolute external URLs, data URIs, Razor-tokenized paths, etc.).
        /// </summary>
        private static string? NormalizeToRootRelative(string raw, string? cssUrlDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            // Skip external, protocol-relative, and inline data references.
            if (raw.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                raw.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
                raw.StartsWith("//") ||
                raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Skip references that contain Razor expressions we cannot resolve statically,
            // e.g. src="~/images/@link.icon_name".
            if (raw.Contains('@') || raw.Contains('{'))
            {
                return null;
            }

            // Strip query strings / fragments (e.g. cache-busting ?v=123).
            var cut = raw.IndexOfAny(new[] { '?', '#' });
            if (cut >= 0)
            {
                raw = raw.Substring(0, cut);
            }

            // Tilde-rooted app path: ~/images/foo.jpg -> /images/foo.jpg
            if (raw.StartsWith("~/"))
            {
                return "/" + raw.Substring(2).TrimStart('/');
            }

            // Already root-relative.
            if (raw.StartsWith("/"))
            {
                return raw;
            }

            // Relative reference (typical inside CSS url(...)): resolve against the file's
            // directory URL so ../images/foo.gif and images/foo.gif both resolve correctly.
            if (cssUrlDirectory is not null)
            {
                var combined = "/" + CombineUrl(cssUrlDirectory, raw);
                return combined;
            }

            // Relative markup reference without a known base is ambiguous; skip it.
            return null;
        }

        private static string GetUrlDirectory(string projectDir, string filePath)
        {
            var wwwroot = Path.Combine(projectDir, "wwwroot");
            var relative = Path.GetRelativePath(wwwroot, Path.GetDirectoryName(filePath)!);
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static string CombineUrl(string baseDir, string relative)
        {
            var segments = new List<string>();

            foreach (var part in baseDir.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                segments.Add(part);
            }

            foreach (var part in relative.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (part == ".")
                {
                    continue;
                }

                if (part == "..")
                {
                    if (segments.Count > 0)
                    {
                        segments.RemoveAt(segments.Count - 1);
                    }

                    continue;
                }

                segments.Add(part);
            }

            return string.Join('/', segments);
        }

        /// <summary>
        /// Walks up from the test assembly location to locate the eGrants web project
        /// directory (the folder containing eGrants.csproj and wwwroot).
        /// </summary>
        private static string? FindProjectDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir is not null)
            {
                // Look for a sibling/parent "eGrants" project folder with a wwwroot.
                var candidate = Path.Combine(dir.FullName, "eGrants");
                if (File.Exists(Path.Combine(candidate, "eGrants.csproj")) &&
                    Directory.Exists(Path.Combine(candidate, "wwwroot")))
                {
                    return candidate;
                }

                // Also handle the case where BaseDirectory is already inside the project.
                if (File.Exists(Path.Combine(dir.FullName, "eGrants.csproj")) &&
                    Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }
    }
}
