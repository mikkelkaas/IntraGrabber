using IntraGrabber.Models;

namespace IntraGrabber.Services;

public interface ICalendarService
{
    Task<IReadOnlyCollection<CalendarItem>> GetCalenderItems(int daysAhead);
}