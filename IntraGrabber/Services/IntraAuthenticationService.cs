using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

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

    public async Task<string?> GetLoginCookie()
    {
        if (!_memoryCache.TryGetValue("AspAuth", out string? cookieValue))
        {
            // First, do a request to the login page to get some needed cookies and extract the request verification token
            var initialResponse = await _httpClient.GetAsync(
                "/Account/IdpLogin?partnerSp=urn%3Aitslearning%3Ansi%3Asaml%3A2.0%3Astenpriv.m.skoleintra.dk");
            var initialCookies = initialResponse.ExtractCookies();
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

            samlResponseRequestMessage.Content = samlResponseContent;

            // Now we get the final page with the cookie we need
            var samlResponseResponse = await _httpClient.SendAsync(samlResponseRequestMessage);
            var finalCookies = samlResponseResponse.ExtractCookies();
            // get the value of the cookie with name _calendarOptions.CookieName
            cookieValue = finalCookies.Where(x => x.Name == _intraGrabberOptions.CookieName).Select(x => x.Value)
                .FirstOrDefault();

            _memoryCache.Set("AspAuth", cookieValue, new DateTimeOffset(DateTime.Now.AddHours(1)));
        }

        return cookieValue;
    }
}