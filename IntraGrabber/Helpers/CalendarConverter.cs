using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using IntraGrabber.Models;

namespace IntraGrabber.Helpers;

public static class CalendarConverter
{
    private static CalendarEvent ConvertToCalendarEvent(CalendarItem calendarItem)
    {
        var staff = string.IsNullOrEmpty(calendarItem.StaffName) ?
            string.Empty :
            $" ({calendarItem.StaffName?.Split(' ')[0]})";

        var summary = $"{calendarItem.Title}{staff}";

        return new CalendarEvent
        {
            Uid = calendarItem.Id,
            Summary = summary,
            Location = calendarItem.Location.FirstOrDefault(),
            Start = calendarItem.AllDay ? new CalDateTime(calendarItem.Start.Date) : new CalDateTime(calendarItem.Start, "Europe/Copenhagen"),
            End = calendarItem.AllDay ? new CalDateTime(calendarItem.End.Date) : new CalDateTime(calendarItem.End, "Europe/Copenhagen")
        };
    }

    public static Calendar ConvertToCalendar(IEnumerable<CalendarItem> items)
    {
        var calendar = new Calendar();

        foreach (var item in items.Select(ConvertToCalendarEvent))
        {
            calendar.Events.Add(item);
        }

        return calendar;
    }

    public static IEnumerable<CalendarItem> GroupByTitleAndTime(IEnumerable<CalendarItem> items)
    {
        var groups = items.GroupBy(i => new { i.Start, i.End });
        foreach (var g in groups)
        {
            CalendarItem f = g.First();
            IEnumerable<string> t = g.Where(h => !string.IsNullOrWhiteSpace(h.Title)).OrderBy(h => h.Title).Select(h => h.Title).Distinct();
            string combinedTitle = string.Join("/", t);

            IEnumerable<string> s = g.Where(h => !string.IsNullOrWhiteSpace(h.StaffName)).OrderBy(h => h.StaffName).Select(h => h.StaffName.Split(' ')[0]).Distinct().Reverse();
            yield return new CalendarItem
            {
                Id = f.Id,
                Title = combinedTitle,
                Start = f.Start,
                End = f.End,
                StaffName = string.Join("/", s),
                Location = f.Location,
                AllDay = f.AllDay
            };
        }
    }
    
    public static IEnumerable<CalendarItem> GroupByTitleAndStaff(IEnumerable<CalendarItem> items)
    {
        var groupedItems = new List<CalendarItem>();
        CalendarItem previousItem = null;

        foreach (var item in items.OrderBy(i => i.Start))
        {
            if (previousItem == null)
            {
                groupedItems.Add(item);
                previousItem = item;
                continue;
            }

            if (previousItem.Title == item.Title &&
                previousItem.StaffName == item.StaffName &&
                previousItem.End == item.Start)
            {
                previousItem.End = item.End;
            }
            else
            {
                groupedItems.Add(item);
                previousItem = item;
            }
        }

        return groupedItems;
    }
    
    public static string Serialize(Calendar calendar)
    {
        var serializer = new CalendarSerializer(new SerializationContext());
    
        return serializer.SerializeToString(calendar);
    }
}
