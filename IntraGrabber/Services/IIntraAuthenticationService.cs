namespace IntraGrabber.Services;

public interface IIntraAuthenticationService
{
    Task<string?> GetLoginCookie();

}