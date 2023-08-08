namespace IntraGrabber.Controllers;

[ApiController]
[Route("[controller]")]
public class PinController : ControllerBase
{
    private readonly IPinService _pinService;
    
    public PinController(IPinService pinService)
    {
        _pinService = pinService;
    }

    // create get endpoint for pins. Use caching
    [HttpGet("json")]
    [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new []{"nextWeek"})]
    public async Task<IEnumerable<PinItem>> Get()
    {
        return await _pinService.GetPinItems();
    }
}