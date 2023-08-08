namespace IntraGrabber.Services;

public interface IPinService
{
    Task<IEnumerable<PinItem>> GetPinItems();
}