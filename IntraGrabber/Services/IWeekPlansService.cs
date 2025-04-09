namespace IntraGrabber.Services;

public interface IWeekPlansService
{
    Task<Weekplan?> GetWeekplan(bool nextWeek);
    Task<Weekplan?> GetSimplifiedWeekplanForToday();
}