using System.Net.Http.Headers;
using IntraCalendarGrabber.Services;

namespace IntraCalendarGrabber.Helpers;

public class IntraAuthenticationHandler : DelegatingHandler
{
    private readonly IIntraAuthenticationService _authenticationService;
    private readonly CalendarOptions _calendarOptions;

    public IntraAuthenticationHandler(IIntraAuthenticationService authenticationService, IOptions<CalendarOptions> options)
    {
        _authenticationService = authenticationService;
        _calendarOptions = options.Value;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookieValue = await _authenticationService.GetLoginCookie();
        request.Headers.Add("Cookie", $"{_calendarOptions.CookieName}={cookieValue}");
        return await base.SendAsync(request, cancellationToken);
    }
}