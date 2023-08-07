using System.Globalization;
using HtmlAgilityPack;

namespace IntraGrabber.Services;

public class WeekPlansService : IWeekPlansService
{
    private readonly HttpClient _httpClient;
    private readonly IntraGrabberOptions _options;

    public WeekPlansService(IHttpClientFactory clientFactory, IOptions<IntraGrabberOptions> options)
    {
        _httpClient = clientFactory.CreateClient("IntraGrabber");
        _options = options.Value;
    }

    public async Task<Weekplan?> GetWeekplan(bool nextWeek)
    {
        var dayToLookFor = DateTime.Now;
        // if today is weekend, get next week
        if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            dayToLookFor = dayToLookFor.AddDays(7);

        if (nextWeek) dayToLookFor = dayToLookFor.AddDays(7);

        var weeknumber = ISOWeek.GetWeekOfYear(dayToLookFor);

        var url = string.Format(_options.WeekplanFormatString, _options.ParentId, _options.ChildName, weeknumber,
            dayToLookFor.Year);
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(await response.Content.ReadAsStringAsync());

            var divNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='root']");

            if (divNode != null)
            {
                var jsonData = divNode.GetAttributeValue("data-clientlogic-settings-WeeklyPlansApp", "");

                if (!string.IsNullOrEmpty(jsonData)) return JsonSerializer.Deserialize<Weekplan>(jsonData);
            }
        }

        return new Weekplan();
    }
}