using System.Net;
using System.Net.Http.Headers;
using IntraGrabber.Services;
using IntraGrabber.Helpers;

namespace IntraGrabber.Helpers;

public class IntraAuthenticationHandler : DelegatingHandler
{
    private readonly IIntraAuthenticationService _authenticationService;
    private readonly IntraGrabberOptions _intraGrabberOptions;

    public IntraAuthenticationHandler(IIntraAuthenticationService authenticationService, IOptions<IntraGrabberOptions> options)
    {
        _authenticationService = authenticationService;
        _intraGrabberOptions = options.Value;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookies = (await _authenticationService.GetLoginCookies()).ToList();

        // Always include these cookies
        var alwaysCookies = new List<Cookie>
        {
            new("UniUserRole", "Parent"),
            new("User", _intraGrabberOptions.LoginUsername),
            new("UserRole", "Parent")
        };

        // De-duplicate by name: prefer the forced values above
        var nameSet = new HashSet<string>(alwaysCookies.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        cookies.RemoveAll(c => nameSet.Contains(c.Name));
        cookies.AddRange(alwaysCookies);

        request.PopulateCookies(cookies);
        return await base.SendAsync(request, cancellationToken);
    }
}