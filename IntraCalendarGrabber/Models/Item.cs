namespace IntraCalendarGrabber.Models;

public class Item : IEquatable<Item>
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string Title { get; set; }

    private string[] _location;
    
    [System.Text.Json.Serialization.JsonPropertyName("location")]
    public string[] Location
    {
        get => _location ?? new string[0];
        set => _location = value;
    }

    [System.Text.Json.Serialization.JsonPropertyName("start")]
    public DateTime Start { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("end")]
    public DateTime End { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("staffName")]
    public string StaffName { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("allDay")]
    public bool AllDay { get; set; }

    public bool Equals(Item other)
    {
        if (other == null)
        {
            return false;
        }

        var location = new HashSet<string>(Location);

        return
            Id == other.Id &&
            Title == other.Title &&
            location.SetEquals(other.Location) &&
            Start == other.Start &&
            End == other.End &&
            StaffName == other.StaffName &&
            AllDay == other.AllDay;
    }
}
