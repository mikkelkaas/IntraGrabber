using IntraCalendarGrabber.Helpers;
using IntraCalendarGrabber.Models;
using IntraCalendarGrabber.Services;

namespace IntraCalendarGrabber.Controllers;

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
    [HttpGet("/json")]
    public async Task<IActionResult> GetJson(int daysAhead = 7)
    {
        IEnumerable<Item> items = await _calendarService.GetItemsAsync(daysAhead);

        items = CalendarConverter.GroupByTitleAndTime(items);
        items = CalendarConverter.GroupByTitleAndStaff(items);

        return Ok(items);
    }
    
    // GET api/calendar/ical
    [HttpGet("/ical")]
    public async Task<IActionResult> GetICal(int daysAhead = 7)
    {
        IEnumerable<Item> items = await _calendarService.GetItemsAsync(daysAhead);
        items = CalendarConverter.GroupByTitleAndTime(items);
        items = CalendarConverter.GroupByTitleAndStaff(items);
        var calendar = CalendarConverter.ConvertToCalendar(items);
        var ical = CalendarConverter.Serialize(calendar);
        return File(Encoding.ASCII.GetBytes(ical), "text/calendar", "calendar.ics");
    }
}