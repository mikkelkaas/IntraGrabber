namespace IntraGrabber.Models;

public class Weekplan
{
    public SelectedPlan SelectedPlan { get; set; }
}

public class SelectedPlan
{
    public Plan GeneralPlan { get; set; }
    public List<Plan> DailyPlans { get; set; }
}

public class Plan
{
    public string Date { get; set; }
    public string Day { get; set; }
    public int DayOfWeek { get; set; }
    public string FormattedDate { get; set; }
    public string LongFormattedDate { get; set; }
    public string FeedbackFormattedDate { get; set; }
    public List<LessonPlan> LessonPlans { get; set; }
    public List<Schedule> Schedule { get; set; }
}

public class LessonPlan
{
    public int WeeklyPlanId { get; set; }
    public Subject Subject { get; set; }
    public string Content { get; set; }
}

public class Subject
{
    public string Title { get; set; }
    public bool IsGeneralSubject { get; set; }
    public string FormattedTitle { get; set; }
    public long LessonNumber { get; set; }
}

public class Schedule
{
    public string TimeString { get; set; }
    public string Title { get; set; }
    public string ClassName { get; set; }
    public string ShortSubjectTitle { get; set; }
    public string FullSubjectTitle { get; set; }
    public int LessonNumber { get; set; }
}