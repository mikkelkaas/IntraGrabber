using IntraGrabber.Models;

namespace IntraGrabber.Services;

public class CalendarService : ICalendarService
{
    private readonly HttpClient _httpClient;
    private readonly IntraGrabberOptions _options;

    public CalendarService(IHttpClientFactory clientFactory, IOptions<IntraGrabberOptions> options)
    {
        _httpClient = clientFactory.CreateClient("IntraGrabber");
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IReadOnlyCollection<CalendarItem>> GetItemsAsync(int daysAhead)
    {
        var s = DateTime.Today.ToSecondsSinceEpoch();
        var e = DateTime.Today.AddDays(daysAhead).ToSecondsSinceEpoch();
        var n = DateTime.Now.ToMillisecondsSinceEpoch();

        var url = string.Format(_options.LessonFormatString, _options.ClassName, s, e, n, _options.ParentId, _options.ChildName);

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var some = JsonSerializer.Deserialize<CalendarItem[]>(json);
            return some;
        }

        return new CalendarItem[0];
    }
}