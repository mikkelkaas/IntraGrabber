using System.Net.Http.Headers;
using IntraGrabber.Services;

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
        var cookieValue = await _authenticationService.GetLoginCookie();
        request.Headers.Add("Cookie", $"{_intraGrabberOptions.CookieName}={cookieValue}");
        return await base.SendAsync(request, cancellationToken);
    }
}