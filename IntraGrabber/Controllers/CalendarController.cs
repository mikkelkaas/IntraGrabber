
namespace IntraGrabber.Controllers;

[ApiController]
[Route("[controller]")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    // GET api/calendar/json
    [HttpGet("json")]
    // cache response for half a day for both server and client and vary by all query parameters
    [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new []{"daysAhead"})]
    public async Task<IActionResult> GetJson(int daysAhead = 7)
    {
        IEnumerable<CalendarItem> items = await _calendarService.GetCalenderItems(daysAhead);

        items = CalendarConverter.GroupByTitleAndTime(items);
        items = CalendarConverter.GroupByTitleAndStaff(items);

        return Ok(items);
    }
    
    // GET api/calendar/ical
    [HttpGet("ical")]
    [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new []{"daysAhead"})]
    public async Task<IActionResult> GetICal(int daysAhead = 7)
    {
        IEnumerable<CalendarItem> items = await _calendarService.GetCalenderItems(daysAhead);
        items = CalendarConverter.GroupByTitleAndTime(items);
        items = CalendarConverter.GroupByTitleAndStaff(items);
        var calendar = CalendarConverter.ConvertToCalendar(items);
        var ical = CalendarConverter.Serialize(calendar);
        return File(Encoding.UTF8.GetBytes(ical), "text/calendar", "calendar.ics");
    }
}