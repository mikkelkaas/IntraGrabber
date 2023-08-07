using IntraCalendarGrabber.Models;

namespace IntraCalendarGrabber.Services;

public interface ICalendarService
{
    Task<IReadOnlyCollection<Item>> GetItemsAsync(int daysAhead);
}