using System.Globalization;
using System.Web;
using HtmlAgilityPack;

namespace IntraGrabber.Services;

public class PinService : IPinService
{
    private readonly HttpClient _httpClient;
    private readonly IntraGrabberOptions _options;

    public PinService(IHttpClientFactory clientFactory, IOptions<IntraGrabberOptions> options)
    {
        _httpClient = clientFactory.CreateClient("IntraGrabber");
        _options = options.Value;
    }

    private static readonly string[] DanishMonths =
        ["jan", "feb", "mar", "apr", "maj", "jun", "jul", "aug", "sep", "okt", "nov", "dec"];

    /// <summary>
    /// Parses ForældreIntra timestamps like "30. okt. 2026 12:31" or "4. maj 2026 08:00".
    /// Uses an explicit month table so it works regardless of the container's ICU/culture data.
    /// </summary>
    internal static DateTime? ParseDanishDate(string text)
    {
        var m = System.Text.RegularExpressions.Regex.Match(text,
            @"(\d{1,2})\.\s*([a-zæøå]{3})[a-zæøå]*\.?\s*(\d{4})(?:\s+(\d{1,2}):(\d{2}))?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        var month = Array.IndexOf(DanishMonths, m.Groups[2].Value.ToLowerInvariant()) + 1;
        if (month == 0) return null;
        var hour = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
        var minute = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
        try
        {
            return new DateTime(int.Parse(m.Groups[3].Value), month, int.Parse(m.Groups[1].Value), hour, minute, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<PinItem>> GetPinItems()
    {
        var response =
            await _httpClient.GetAsync(string.Format(_options.PinFormatString, _options.ParentId, _options.ChildName));
        if (response.IsSuccessStatusCode)
        {
            var htmlDocument = new HtmlDocument();
            var content = await response.Content.ReadAsStringAsync();
            htmlDocument.LoadHtml(content.Replace("<br />", Environment.NewLine));

            var pinNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='sk-news-item']");

            var result = new List<PinItem>();

            if (pinNodes != null)
            {
                foreach (var pinNode in pinNodes)
                {
                    var pinItem = new PinItem();
                    pinItem.CreatedText = HttpUtility.HtmlDecode(pinNode.SelectSingleNode(".//div[contains(@class, 'sk-news-item-timestamp')]")
                        .InnerText).Trim();
                    // convert a text like this "23. aug. 2023 08:19" (Danish month names) to local datetime
                    pinItem.Created = ParseDanishDate(pinItem.CreatedText) ?? DateTime.MinValue;
                    
                    var authorNode = pinNode.SelectSingleNode(".//div[contains(@class, 'sk-news-item-author')]");
                    pinItem.Author = authorNode.SelectSingleNode(".//span").InnerText;
                    // get first span after span with class "sk-news-item-for"
                    var recipients = authorNode
                        .SelectSingleNode(".//span[contains(@class, 'sk-news-item-for')]/following-sibling::span[1]")
                        .InnerText;
                    //get innertext of span with class "sk-news-show-more-container" and add to recipients
                    var moreRecipients =
                        authorNode.SelectSingleNode(".//span[contains(@class, 'sk-news-show-more-container')]");
                    if (moreRecipients != null) recipients += moreRecipients.InnerText.Trim();
                    recipients = recipients.Replace(" og ", ", ");
                    pinItem.Recipients = recipients.Split(',').Select(x => x.Trim()).ToList();

                    // get content of div with class "sk-user-input" from divNode
                    pinItem.Text = HttpUtility.HtmlDecode(pinNode
                        .SelectSingleNode(".//div[contains(@class, 'sk-user-input')]").InnerText.Trim());
                    // pinItem.Text = HttpUtility.HtmlDecode("Hello&aelig;");  // yields Helloæ

                    result.Add(pinItem);
                }

                return result;
            }
        }

        return new List<PinItem>();
    }
}