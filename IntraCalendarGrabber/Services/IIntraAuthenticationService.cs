namespace IntraCalendarGrabber.Services;

public interface IIntraAuthenticationService
{
    Task<string?> GetLoginCookie();

}