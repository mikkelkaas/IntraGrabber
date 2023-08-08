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
                    // convert a text like this 23. aug. 2023 08:19 to local datetime
                    pinItem.Created = DateTime.ParseExact(pinItem.CreatedText, "d. MMM. yyyy HH:mm",
                        CultureInfo.InvariantCulture);
                    
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