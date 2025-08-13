using System.Net;

namespace IntraGrabber.Services;

public interface IIntraAuthenticationService
{
    Task<IEnumerable<Cookie>> GetLoginCookies();

}