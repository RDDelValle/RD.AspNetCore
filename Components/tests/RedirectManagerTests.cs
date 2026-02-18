using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RD.AspNetCore.Components;

namespace RD.AspNetCore.Components.Tests;

public sealed class RedirectManagerTests
{
    [Fact]
    public void RedirectTo_with_relative_uri_navigates_to_same_uri()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager);

        redirectManager.RedirectTo("account/settings");

        Assert.Equal("account/settings", navigationManager.LastUri);
    }

    [Fact]
    public void RedirectTo_with_absolute_uri_uses_base_relative_path()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager);

        redirectManager.RedirectTo("https://example.com/secure/profile");

        Assert.Equal("secure/profile", navigationManager.LastUri);
    }

    [Fact]
    public void RedirectTo_with_query_parameters_appends_query_and_redirects()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager);

        redirectManager.RedirectTo("https://example.com/search", new Dictionary<string, object?> { ["q"] = "test" });

        Assert.Equal("search?q=test", navigationManager.LastUri);
    }

    [Fact]
    public void RedirectToWithStatus_sets_cookie_and_redirects()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        var accessor = new HttpContextAccessor { HttpContext = context };
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, accessor);

        redirectManager.RedirectToWithStatus("https://example.com/account", "Saved!");

        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains($"{RedirectManager.StatusMessageCookieName}=Saved%21", setCookie);
        Assert.Contains("max-age=5", setCookie);
        Assert.Equal("account", navigationManager.LastUri);
    }

    [Fact]
    public void RedirectToWithStatus_with_query_parameters_appends_query_and_redirects()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        var accessor = new HttpContextAccessor { HttpContext = context };
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, accessor);

        redirectManager.RedirectToWithStatus(
            "https://example.com/search",
            new Dictionary<string, object?> { ["q"] = "test" },
            "Saved!");

        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains($"{RedirectManager.StatusMessageCookieName}=Saved%21", setCookie);
        Assert.Contains("max-age=5", setCookie);
        Assert.Equal("search?q=test", navigationManager.LastUri);
    }

    [Fact]
    public void RedirectToWithStatus_throws_when_httpcontext_missing()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, new HttpContextAccessor());

        var exception = Assert.Throws<InvalidOperationException>(
            () => redirectManager.RedirectToWithStatus("account", "Saved!"));

        Assert.Contains("HttpContext is not available", exception.Message);
    }

    [Fact]
    public void TryRedirectToWithStatus_returns_false_when_httpcontext_missing()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, new HttpContextAccessor());

        var result = redirectManager.TryRedirectToWithStatus("account", "Saved!");

        Assert.False(result);
    }

    [Fact]
    public void TryRedirectToWithStatus_returns_true_when_cookie_is_set()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        var accessor = new HttpContextAccessor { HttpContext = context };
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, accessor);

        var result = redirectManager.TryRedirectToWithStatus("account", "Saved!");

        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        Assert.True(result);
        Assert.Contains($"{RedirectManager.StatusMessageCookieName}=Saved%21", setCookie);
        Assert.Contains("max-age=5", setCookie);
        Assert.Equal("account", navigationManager.LastUri);
    }

    [Fact]
    public void RedirectToExternal_allows_absolute_uri_and_force_loads()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager);

        redirectManager.RedirectToExternal("https://contoso.com/welcome");

        Assert.Equal("https://contoso.com/welcome", navigationManager.LastUri);
        Assert.True(navigationManager.LastOptions?.ForceLoad);
    }

    [Fact]
    public void ClearStatus_deletes_cookie()
    {
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, accessor);

        redirectManager.ClearStatus();

        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains($"{RedirectManager.StatusMessageCookieName}=", setCookie);
    }

    [Fact]
    public void TryClearStatus_returns_false_when_httpcontext_missing()
    {
        var navigationManager = new TestNavigationManager("https://example.com/");
        var redirectManager = new RedirectManager(navigationManager, new HttpContextAccessor());

        var result = redirectManager.TryClearStatus();

        Assert.False(result);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public string? LastUri { get; private set; }
        public NavigationOptions? LastOptions { get; private set; }

        public TestNavigationManager(string baseUri)
        {
            Initialize(baseUri, baseUri);
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            LastUri = uri;
            LastOptions = options;
        }
    }
}
