using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace RD.AspNetCore.Components
{
    public sealed class RedirectManager(NavigationManager navigationManager, IHttpContextAccessor? httpContextAccessor = null)
    {
        public const string StatusMessageCookieName = "StatusMessage";

        public void RedirectTo(string? uri)
        {
            uri ??= "";

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                uri = navigationManager.ToBaseRelativePath(uri);
            }

            navigationManager.NavigateTo(uri);
        }

        public void RedirectToWithStatus(string uri, string statusMessage)
        {
            var context = httpContextAccessor?.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is not available for setting status cookies.");

            var expires = DateTimeOffset.UtcNow.AddSeconds(5);
            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Path = "/",
                MaxAge = TimeSpan.FromSeconds(5),
                Expires = expires
            };

            context.Response.Cookies.Append(StatusMessageCookieName, statusMessage, options);
            RedirectTo(uri);
        }

        public void RedirectToWithStatus(string uri, Dictionary<string, object?> queryParameters, string statusMessage)
        {
            var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            RedirectToWithStatus(newUri, statusMessage);
        }

        public bool TryRedirectToWithStatus(string uri, string statusMessage)
        {
            if (httpContextAccessor?.HttpContext is not { } context)
            {
                return false;
            }

            var expires = DateTimeOffset.UtcNow.AddSeconds(5);
            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Path = "/",
                MaxAge = TimeSpan.FromSeconds(5),
                Expires = expires
            };

            context.Response.Cookies.Append(StatusMessageCookieName, statusMessage, options);
            RedirectTo(uri);
            return true;
        }

        public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
        {
            var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            RedirectTo(newUri);
        }

        public void RedirectToExternal(string uri)
        {
            navigationManager.NavigateTo(uri, forceLoad: true);
        }

        public void ClearStatus()
        {
            var context = httpContextAccessor?.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is not available for clearing status cookies.");

            context.Response.Cookies.Delete(StatusMessageCookieName, new CookieOptions { Path = "/" });
        }

        public bool TryClearStatus()
        {
            if (httpContextAccessor?.HttpContext is not { } context)
            {
                return false;
            }

            context.Response.Cookies.Delete(StatusMessageCookieName, new CookieOptions { Path = "/" });
            return true;
        }
    }
}
