namespace IntraGrabber.Models;

public class PinItem
{
    public string Author { get; set; }
    public List<string> Recipients { get; set; }
    public string Text { get; set; }
    public string CreatedText { get; set; }
    public DateTime Created { get; set; }
    public string Expires { get; set; }
    public string Attachment { get; set; }
}