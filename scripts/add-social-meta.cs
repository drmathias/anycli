#:property TargetFramework=net10.0

// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

const string baseUrl = "https://drmathias.github.io/anycli/";
const string imageUrl = baseUrl + "branding/anycli-social.png";
const string description = "A source-generated, AOT-friendly command-line framework for .NET.";
const string siteName = "AnyCli";
const string marker = "<!-- anycli:social-meta -->";

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/add-social-meta.cs -- <output-dir>");
    return 1;
}

var outputDirectory = Path.GetFullPath(args[0]);
if (!Directory.Exists(outputDirectory))
{
    Console.Error.WriteLine($"Output directory not found: {outputDirectory}");
    return 1;
}

var updated = 0;
foreach (var filePath in Directory.EnumerateFiles(outputDirectory, "*.html", SearchOption.AllDirectories))
{
    var document = File.ReadAllText(filePath, Encoding.UTF8);
    if (document.Contains(marker, StringComparison.Ordinal)) continue;

    var headCloseIndex = document.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
    if (headCloseIndex < 0) continue;

    var title = GetPageTitle(document, siteName);
    var pageDescription = GetPageDescription(document, description);
    var url = GetCanonicalUrl(outputDirectory, filePath, baseUrl);
    var meta = GetSocialMeta(title, pageDescription, url, imageUrl, siteName);

    document = document[..headCloseIndex] + meta + document[headCloseIndex..];
    File.WriteAllText(filePath, document, new UTF8Encoding(false));
    updated++;
}

Console.WriteLine($"Added social metadata to {updated} HTML file(s).");
return 0;

static string GetCanonicalUrl(string outputDirectory, string filePath, string baseUrl)
{
    var relativePath = Path.GetRelativePath(outputDirectory, filePath).Replace('\\', '/');
    return relativePath == "index.html"
        ? baseUrl
        : relativePath.EndsWith("/index.html", StringComparison.Ordinal)
            ? baseUrl + relativePath[..^"index.html".Length]
            : baseUrl + relativePath;
}

static string GetPageTitle(string document, string siteName)
{
    var match = Regex.Match(document, "<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    return !match.Success ? siteName : WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
}

static string GetPageDescription(string document, string fallback)
{
    var match = Regex.Match(document, "<meta\\s+name=\"description\"\\s+content=\"([^\"]*)\"", RegexOptions.IgnoreCase);
    if (!match.Success) return fallback;

    var value = WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
    return string.IsNullOrWhiteSpace(value) ? fallback : value;
}

static string GetSocialMeta(string title, string description, string url, string imageUrl, string siteName)
{
    var encodedTitle = HtmlEncoder.Default.Encode(title);
    var encodedDescription = HtmlEncoder.Default.Encode(description);
    var encodedUrl = HtmlEncoder.Default.Encode(url);
    var encodedImage = HtmlEncoder.Default.Encode(imageUrl);
    var encodedSite = HtmlEncoder.Default.Encode(siteName);

    return $"""
    {marker}
    <meta property="og:type" content="website">
    <meta property="og:site_name" content="{encodedSite}">
    <meta property="og:title" content="{encodedTitle}">
    <meta property="og:description" content="{encodedDescription}">
    <meta property="og:url" content="{encodedUrl}">
    <meta property="og:image" content="{encodedImage}">
    <meta property="og:image:type" content="image/png">
    <meta property="og:image:width" content="1280">
    <meta property="og:image:height" content="640">
    <meta property="og:image:alt" content="{encodedSite} terminal logo">
    <meta name="twitter:card" content="summary_large_image">
    <meta name="twitter:title" content="{encodedTitle}">
    <meta name="twitter:description" content="{encodedDescription}">
    <meta name="twitter:image" content="{encodedImage}">
""";
}
