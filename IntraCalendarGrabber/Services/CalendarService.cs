using IntraCalendarGrabber.Models;

namespace IntraCalendarGrabber.Services;

public class CalendarService : ICalendarService
{
    private readonly HttpClient _httpClient;
    private readonly CalendarOptions _options;

    public CalendarService(HttpClient httpClient, IOptions<CalendarOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IReadOnlyCollection<Item>> GetItemsAsync(int daysAhead)
    {
        var s = DateTime.Today.ToSecondsSinceEpoch();
        var e = DateTime.Today.AddDays(daysAhead).ToSecondsSinceEpoch();
        var n = DateTime.Now.ToMillisecondsSinceEpoch();

        var url = string.Format(_options.UrlFormatString, _options.ClassName, s, e, n);

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var some = JsonSerializer.Deserialize<Item[]>(json);
            return some;
        }

        return new Item[0];
    }
}