using System.Globalization;
using IntraGrabber.Helpers;
using IntraGrabber.Models;
using IntraGrabber.Services;

namespace IntraGrabber.Controllers;

[ApiController]
[Route("[controller]")]
public class WeekplanController : ControllerBase
{
    private readonly IWeekPlansService _weekPlansService;

    public WeekplanController(IWeekPlansService  weekPlansService)
    {
        _weekPlansService = weekPlansService;
    }

    // GET api/calendar/json
    [HttpGet()]
    public async Task<IActionResult> Get(bool nextWeek = false)
    {

        
        var plan = await _weekPlansService.GetWeekplan(nextWeek);
        return Ok(plan);
    }
}