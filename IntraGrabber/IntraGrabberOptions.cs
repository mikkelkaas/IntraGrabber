namespace IntraGrabber;

public class IntraGrabberOptions
{
    public Uri BaseAddress { get; set; }
    public string ChildName {get;set;}
    public int ParentId {get;set;}
    public string LessonFormatString { get; set; }
    public string WeekplanFormatString { get; set; }
    public string PinFormatString { get; set; }
    public string ClassName { get; set; }
    public string LoginPassword { get; set; }
    public string LoginUsername { get; set; }
    public string CookieName { get; set; }
}
