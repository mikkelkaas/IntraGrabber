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
    [HttpGet("json")]
    // [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new []{"nextWeek"})]
    public async Task<IActionResult> Get(bool nextWeek = false)
    {
        var plan = await _weekPlansService.GetWeekplan(nextWeek);
        return Ok(plan);
    }
}