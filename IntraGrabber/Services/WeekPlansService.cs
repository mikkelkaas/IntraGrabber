using System.Globalization;
using HtmlAgilityPack;

namespace IntraGrabber.Services;

public class WeekPlansService(IHttpClientFactory clientFactory, IOptions<IntraGrabberOptions> options, ILogger<WeekPlansService> logger)
    : IWeekPlansService
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient("IntraGrabber");
    private readonly IntraGrabberOptions _options = options.Value;

    public async Task<Weekplan?> GetWeekplan(bool nextWeek)
    {
        var dayToLookFor = DateTime.Now;
        // if today is weekend, get next week
        if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            dayToLookFor = dayToLookFor.AddDays(7);
        

        if (nextWeek) dayToLookFor = dayToLookFor.AddDays(7);
        logger.LogWarning("Day to look for: {dayToLookFor}", dayToLookFor);
        var weeknumber = ISOWeek.GetWeekOfYear(dayToLookFor);
        logger.LogWarning("Week number: {weeknumber}", weeknumber);
        
        var url = string.Format(_options.WeekplanFormatString, _options.ParentId, _options.ChildName, weeknumber,
            dayToLookFor.Year);
        logger.LogWarning("URL: {url}", url);
        var response = await _httpClient.GetAsync(url);
        logger.LogWarning("Status code: {statusCode}", response.StatusCode);
        if (response.IsSuccessStatusCode)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(await response.Content.ReadAsStringAsync());

            var divNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='root']");

            if (divNode != null)
            {
                var jsonData = divNode.GetAttributeValue("data-clientlogic-settings-WeeklyPlansApp", "");

                if (!string.IsNullOrEmpty(jsonData)) return FormatWeekplanContent(JsonSerializer.Deserialize<Weekplan>(jsonData));
            }
        }

        return new Weekplan();
    }
    
    public async Task<Weekplan?> GetSimplifiedWeekplanForToday()
    {
        var fullWeekplan = await GetWeekplanForToday();
        if (fullWeekplan?.SelectedPlan == null)
            return null;

        // Get target date considering the special Friday case
        var targetDate = GetTargetDateWithFridayHandling();

        // Find the daily plan for our target date
        var dailyPlan = fullWeekplan.SelectedPlan.DailyPlans?.FirstOrDefault(p =>
            DateTime.TryParse(p.Date, out var planDate) &&
            planDate.Date == targetDate.Date);

        // Create a new simplified weekplan
        return FormatWeekplanContent(new Weekplan
        {
            SelectedPlan = new SelectedPlan
            {
                GeneralPlan = fullWeekplan.SelectedPlan.GeneralPlan,
                DailyPlans = dailyPlan != null ? new List<Plan> { dailyPlan } : new List<Plan>()
            }
        });
    }

    private async Task<Weekplan?> GetWeekplanForToday()
    {
        var targetDate = GetTargetDateWithFridayHandling();

        // If target date is in next week, get next week's plan
        var currentDate = DateTime.Now;
        var isNextWeek = ISOWeek.GetWeekOfYear(targetDate) > ISOWeek.GetWeekOfYear(currentDate);

        return await GetWeekplan(isNextWeek);
    }

    private DateTime GetTargetDateWithFridayHandling()
    {
        var now = DateTime.Now;
    
        // If it's after 16:00
        if (now.Hour >= 16)
        {
            // If it's Friday, get next Monday (add 3 days)
            if (now.DayOfWeek == DayOfWeek.Friday)
                return now.AddDays(3);
            // Otherwise just get next day as before
            return now.AddDays(1);
        }
    
        return now;
    }
    
    private Weekplan? FormatWeekplanContent(Weekplan? weekplan)
    {
        if (weekplan?.SelectedPlan == null)
            return weekplan;

        // Format GeneralPlan if it exists
        if (weekplan.SelectedPlan.GeneralPlan?.LessonPlans != null)
        {
            foreach (var lessonPlan in weekplan.SelectedPlan.GeneralPlan.LessonPlans)
            {
                lessonPlan.Content = CleanHtmlContent(lessonPlan.Content);
            }
        }

        // Format all DailyPlans
        foreach (var dailyPlan in weekplan.SelectedPlan.DailyPlans)
        {
            foreach (var lessonPlan in dailyPlan.LessonPlans)
            {
                lessonPlan.Content = CleanHtmlContent(lessonPlan.Content);
            }
        }

        return weekplan;
    }

    private string CleanHtmlContent(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
            return string.Empty;

        // Remove HTML tags and convert line breaks
        var text = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);
    
        // Replace HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
    
        // Normalize whitespace and clean up the text
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
    
        return text.Trim();
    }
}