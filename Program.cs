using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;



class Program
{
    static async Task Main()
    {
        IWebDriver driver = new ChromeDriver();
        try
        {


            string searchQuery = "camera"; // Change this to your desired search query
            string baseurl = $"https://www.flipkart.com/search?q={searchQuery}";

            //connecting to the target web page
            driver.Navigate().GoToUrl(baseurl);


            List<CameraData> cameraDataList = await ScrapeFlipkart(baseurl, driver);

            if (cameraDataList.Count > 0)
            {
                SaveToCsv(cameraDataList, "camera_data.csv");
                Console.WriteLine("Data has been scraped and saved to camera_data.csv.");
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        finally
        {
            // Ensure the WebDriver is closed even in case of exceptions
            // driver.Quit();
        }


        static async Task<List<CameraData>> ScrapeFlipkart(string baseurl, IWebDriver driver)
        {
            List<CameraData> cameraDataList = new List<CameraData>();
            var httpClient = new HttpClient();
            int page = 1;

            while (true)
            {
                string url = $"{baseurl}&page={page}";
                var html = await httpClient.GetStringAsync(url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var productNodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, '_2kHMtA')]");

                if (productNodes == null || productNodes.Count == 0)
                {
                    break; // No more pages to scrape
                }

                foreach (var productNode in productNodes)
                {
                    var titleNode = productNode.SelectSingleNode("(//div[contains(@class,'_2kHMtA')]/a/div[2])/div[1]/div[1]");
                    var priceNode = productNode.SelectSingleNode("((//div[contains(@class,'_2kHMtA')]/a/div[2])/div[2]/div[1])/div[1]/div[1]");
                    var ratingNode = productNode.SelectSingleNode("(//div[contains(@class,'_2kHMtA')]/a/div[2])/div[1]/div[2]/span/div[1]");
                    var urlNode = productNode.SelectSingleNode("//div[contains(@class,'_2kHMtA')]/a/@href");



                    if (titleNode != null && priceNode != null && ratingNode != null && urlNode != null)
                    {
                        string title = titleNode.InnerText.Trim();
                        //Console.WriteLine("title : " + title.Trim());
                        string price = priceNode.InnerText.Trim();
                        //Console.WriteLine("price : " + price.Trim());
                        string rating = ratingNode.InnerText.Trim();
                        //Console.WriteLine("Rating : " + rating.Trim());
                        string productUrl = urlNode.GetAttributeValue("href", "");
                        //Console.WriteLine("Link : " + productUrl.Trim());
                        string completeUrl = "https://www.flipkart.com" + productUrl;

                        cameraDataList.Add(new CameraData
                        {
                            Title = title,
                            Price = price,
                            Rating = rating,
                            URL = completeUrl
                        });
                    }



                }
                try
                {
                    var nextButton = driver.FindElement(By.XPath("//a[contains(@class, '_1LKTO3')]"));
                    if (nextButton != null && nextButton.Displayed)
                    {
                        nextButton.Click();
                        page++;
                    }

                }
                catch
                {
                    Console.WriteLine("next button not found");
                }






            }
            return cameraDataList;
        }
    }

    static void SaveToCsv(List<CameraData> cameraDataList, string fileName)
    {

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);

        using (var writer = new StreamWriter(fileName))                                           
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            csv.WriteRecords(cameraDataList);
        }
    }
}



   public class CameraData
   {
    public string Title { get; set; } = " ";
    public string Price { get; set; } = " ";
    public string Rating { get; set; } = " ";

    public string URL { get; set; } = " ";
   }




