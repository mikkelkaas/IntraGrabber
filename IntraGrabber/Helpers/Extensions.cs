using System.Net;
using System.Text.RegularExpressions;

namespace IntraGrabber.Helpers;

public static class Extensions
{
    private static readonly DateTimeOffset Epoch = new(new DateTime(1970, 1, 1), TimeSpan.FromHours(0));
    
    public static IEnumerable<Cookie> ExtractCookies(this HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookieEntries))
        {
            return Enumerable.Empty<Cookie>();
        }
        var uri = response.RequestMessage.RequestUri;
        var cookieContainer = new CookieContainer();

        foreach (var cookieEntry in cookieEntries)
        {
            cookieContainer.SetCookies(uri, cookieEntry);
        }

        return cookieContainer.GetCookies(uri).Cast<Cookie>();
    }

    public static void PopulateCookies(this HttpRequestMessage request, IEnumerable<Cookie> cookies)
    {
        request.Headers.Remove("Cookie");
        if (cookies.Any())
        {
            request.Headers.Add("Cookie", cookies.ToHeaderFormat());
        }
    }

    private static string ToHeaderFormat(this IEnumerable<Cookie> cookies)
    {
        var cookieString = string.Empty;
        var delimiter = string.Empty;

        foreach (var cookie in cookies)
        {
            cookieString += delimiter + cookie;
            delimiter = "; ";
        }

        return cookieString;
    }


    public static long ToMillisecondsSinceEpoch(this DateTime dateTime)
    {
        var utc = dateTime.ToUniversalTime();
        var timeSpan = utc - Epoch;

        return (long) timeSpan.TotalMilliseconds;
    }

    public static int ToSecondsSinceEpoch(this DateTime dateTime)
    {
        var utc = dateTime.ToUniversalTime();
        var timeSpan = utc - Epoch;

        return (int) timeSpan.TotalSeconds;
    }

    public static DateTime ToDateTime(this long millisecondsSinceEpoch)
    {
        var dateTime = Epoch + TimeSpan.FromMilliseconds(millisecondsSinceEpoch);

        return dateTime.ToLocalTime().DateTime;
    }

    public static DateTime ToDateTime(this int secondsSinceEpoch)
    {
        var dateTime = Epoch + TimeSpan.FromSeconds(secondsSinceEpoch);

        return dateTime.ToLocalTime().DateTime;
    }
}