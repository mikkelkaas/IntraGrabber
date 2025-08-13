using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using IntraGrabber.Helpers;

namespace IntraGrabber.Services;

internal class IntraAuthenticationService : IIntraAuthenticationService
{
    private readonly IntraGrabberOptions _intraGrabberOptions;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public IntraAuthenticationService(HttpClient httpClient, IMemoryCache memoryCache,
        IOptions<IntraGrabberOptions> intraGrabberOptions)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _intraGrabberOptions = intraGrabberOptions.Value;
    }

    public async Task<IEnumerable<Cookie>> GetLoginCookies()
    {
        if (!_memoryCache.TryGetValue("IntraGrabberAuthCookies", out List<Cookie>? cachedCookies))
        {
            // First, do a request to the login page to get some needed cookies and extract the request verification token
            var initialResponse = await _httpClient.GetAsync(
                "/Account/IdpLogin?partnerSp=urn%3Aitslearning%3Ansi%3Asaml%3A2.0%3Astenpriv.m.skoleintra.dk");
            var initialCookies = initialResponse.ExtractCookies().ToList();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(await initialResponse.Content.ReadAsStringAsync());
            var requestVerificationToken = htmlDocument.DocumentNode
                .SelectSingleNode("//input[@name='__RequestVerificationToken']")
                ?.Attributes["value"].Value;

            // Setup the login form
            var loginParams = new KeyValuePair<string?, string?>[]
            {
                new("__RequestVerificationToken", requestVerificationToken),
                new("RoleType", "Parent"),
                new("UserName", _intraGrabberOptions.LoginUsername),
                new("Password", _intraGrabberOptions.LoginPassword)
            };
            using var contentForLogin = new FormUrlEncodedContent(loginParams);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                "Account/IdpLogin?partnerSp=urn%3Aitslearning%3Ansi%3Asaml%3A2.0%3Astenpriv.m.skoleintra.dk");
            httpRequestMessage.PopulateCookies(initialCookies);
            httpRequestMessage.Content = contentForLogin;
            var loginResponse = await _httpClient.SendAsync(httpRequestMessage);
            var loginCookies = loginResponse.ExtractCookies().ToList();

            // We get an intermediate page with a form that we need to submit
            var intermediateHtmlDocument = new HtmlDocument();
            intermediateHtmlDocument.LoadHtml(await loginResponse.Content.ReadAsStringAsync());
            var samlResponse = intermediateHtmlDocument.DocumentNode.SelectSingleNode("//input[@name='SAMLResponse']")
                ?.Attributes["value"].Value;
            var samlResponseParams = new KeyValuePair<string?, string?>[]
            {
                new("SAMLResponse", samlResponse)
            };
            using var samlResponseContent = new FormUrlEncodedContent(samlResponseParams);
            var samlResponseRequestMessage = new HttpRequestMessage(HttpMethod.Post, "sso/assertionconsumerservice");

            // include cookies from previous steps
            var accumulatedCookies = new List<Cookie>();
            accumulatedCookies.AddRange(initialCookies);
            accumulatedCookies.AddRange(loginCookies);
            samlResponseRequestMessage.PopulateCookies(accumulatedCookies);
            samlResponseRequestMessage.Content = samlResponseContent;

            // Now we get the final page with the cookie we need
            var samlResponseResponse = await _httpClient.SendAsync(samlResponseRequestMessage);
            var finalCookies = samlResponseResponse.ExtractCookies().ToList();

            // Combine and de-duplicate cookies by name
            var allCookies = new List<Cookie>();
            foreach (var c in initialCookies.Concat(loginCookies).Concat(finalCookies))
            {
                var existing = allCookies.FirstOrDefault(x => x.Name == c.Name);
                if (existing != null)
                {
                    // replace with the latest value
                    allCookies.Remove(existing);
                }
                allCookies.Add(c);
            }

            // Cache the full set for reuse
            _memoryCache.Set("IntraGrabberAuthCookies", allCookies, new DateTimeOffset(DateTime.Now.AddHours(1)));
            cachedCookies = allCookies;
        }

        return cachedCookies ?? Enumerable.Empty<Cookie>();
    }
}