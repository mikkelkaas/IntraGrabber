using IntraGrabber.Models;

namespace IntraGrabber.Services;

public interface ICalendarService
{
    Task<IReadOnlyCollection<CalendarItem>> GetItemsAsync(int daysAhead);
}