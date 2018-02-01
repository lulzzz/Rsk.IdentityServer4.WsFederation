using System;
using System.Text.Encodings.Web;

namespace IdentityServer4.Extensions
{
    public static class StringExtensions
    {
        public static string EnsureLeadingSlash(this string url)
        {
            if (!url.StartsWith("/"))
            {
                return "/" + url;
            }

            return url;
        }

        public static string RemoveLeadingSlash(this string url)
        {
            if (url != null && url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            return url;
        }

        public static string EnsureTrailingSlash(this string url)
        {
            if (!url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }

        public static string RemoveTrailingSlash(this string url)
        {
            if (url != null && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static string AddQueryString(this string url, string name, string value)
        {
            return url.AddQueryString(name + "=" + UrlEncoder.Default.Encode(value));
        }

        public static string AddQueryString(this string url, string query)
        {
            if (!url.Contains("?"))
            {
                url += "?";
            }
            else if (!url.EndsWith("&"))
            {
                url += "&";
            }

            return url + query;
        }

        public static bool IsLocalUrl(this string url)
        {
            return
                !string.IsNullOrEmpty(url) &&

                // Allows "/" or "/foo" but not "//" or "/\".
                (url[0] == '/' && (url.Length == 1 || url[1] != '/' && url[1] != '\\') ||

                 // Allows "~/" or "~/foo".
                 url.Length > 1 && url[0] == '~' && url[1] == '/');
        }

        public static string GetOrigin(this string url)
        {
            if (url != null && (url.StartsWith("http://") || url.StartsWith("https://")))
            {
                var idx = url.IndexOf("//", StringComparison.Ordinal);
                if (idx > 0)
                {
                    idx = url.IndexOf("/", idx + 2, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        url = url.Substring(0, idx);
                    }
                    return url;
                }
            }

            return null;
        }

        public static string ToAssertionConsumerUrl(this string entityId, string acsPath)
        {
            return $"{entityId.RemoveTrailingSlash()}{acsPath.EnsureLeadingSlash()}";
        }
    }
}