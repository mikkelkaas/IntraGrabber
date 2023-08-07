using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using IntraCalendarGrabber.Models;

namespace IntraCalendarGrabber.Helpers;

public static class CalendarConverter
{
    private static CalendarEvent ConvertToCalendarEvent(Item item)
    {
        var staff = string.IsNullOrEmpty(item.StaffName) ?
            string.Empty :
            $" ({item.StaffName?.Split(' ')[0]})";

        var summary = $"{item.Title}{staff}";

        return new CalendarEvent
        {
            Uid = item.Id,
            Summary = summary,
            Location = item.Location.FirstOrDefault(),
            Start = new CalDateTime(item.Start, "Europe/Copenhagen"),
            End = new CalDateTime(item.End, "Europe/Copenhagen"),
            IsAllDay = item.AllDay
        };
    }

    public static Calendar ConvertToCalendar(IEnumerable<Item> items)
    {
        var calendar = new Calendar();

        foreach (var item in items.Select(ConvertToCalendarEvent))
        {
            calendar.Events.Add(item);
        }

        return calendar;
    }

    public static IEnumerable<Item> GroupByTitleAndTime(IEnumerable<Item> items)
    {
        return from i in items
               group i by new { i.Title, i.Start, i.End } into g
               let f = g.First()
               let s = g.Where(h => !string.IsNullOrWhiteSpace(h.StaffName)).OrderBy(h => h.StaffName).Select(h => h.StaffName.Split(' ')[0]).Distinct()
               select new Item
               {
                   Id = f.Id,
                   Title = f.Title,
                   Start = f.Start,
                   End = f.End,
                   StaffName = string.Join("/", s),
                   Location = f.Location,
                   AllDay = f.AllDay
               };
    }
    
    public static IEnumerable<Item> GroupByTitleAndStaff(IEnumerable<Item> items)
    {
        var current = default(Item);
    
        foreach (var item in items.OrderBy(i => i.Start))
        {
            if (current == null)
            {
                current = item;
                continue;
            }
    
            if (current.Start.Date != item.Start.Date)
            {
                yield return current;
                current = item;
                continue;
            }
    
            if (current.Title != item.Title ||
                current.StaffName != item.StaffName)
            {
                yield return current;
                current = item;
                continue;
            }
    
            current.End = item.End;
        }
    
        yield return current;
    }
    
    public static string Serialize(Calendar calendar)
    {
        var serializer = new CalendarSerializer(new SerializationContext());
    
        return serializer.SerializeToString(calendar);
    }
}
