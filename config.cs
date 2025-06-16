using Microsoft.Extensions.Configuration;

namespace WebTestSuite.Configuration
{
    public class TestConfiguration
    {
        private static readonly Lazy<TestConfiguration> _instance = new(() => new TestConfiguration());
        public static TestConfiguration Instance => _instance.Value;

        public string BaseUrl { get; private set; }
        public int TimeoutSeconds { get; private set; }
        public int ImplicitWaitSeconds { get; private set; }
        public int MaxRetryAttempts { get; private set; }
        public bool ScreenshotOnFailure { get; private set; }
        public BrowserOptions BrowserOptions { get; private set; }
        public TestData TestData { get; private set; }

        private TestConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var testSettings = configuration.GetSection("TestSettings");
            
            BaseUrl = testSettings["BaseUrl"] ?? "https://example.com";
            TimeoutSeconds = int.Parse(testSettings["TimeoutSeconds"] ?? "10");
            ImplicitWaitSeconds = int.Parse(testSettings["ImplicitWaitSeconds"] ?? "5");
            MaxRetryAttempts = int.Parse(testSettings["MaxRetryAttempts"] ?? "3");
            ScreenshotOnFailure = bool.Parse(testSettings["ScreenshotOnFailure"] ?? "true");

            BrowserOptions = new BrowserOptions
            {
                Headless = bool.Parse(testSettings["BrowserOptions:Headless"] ?? "false"),
                WindowSize = testSettings["BrowserOptions:WindowSize"] ?? "1920x1080",
                DisableNotifications = bool.Parse(testSettings["BrowserOptions:DisableNotifications"] ?? "true"),
                DisablePopupBlocking = bool.Parse(testSettings["BrowserOptions:DisablePopupBlocking"] ?? "true")
            };

            TestData = new TestData
            {
                SearchQuery = testSettings["TestData:SearchQuery"] ?? "test query",
                ValidEmail = testSettings["TestData:ValidEmail"] ?? "test@example.com",
                ValidPassword = testSettings["TestData:ValidPassword"] ?? "TestPassword123!",
                InvalidEmail = testSettings["TestData:InvalidEmail"] ?? "invalid-email",
                InvalidPassword = testSettings["TestData:InvalidPassword"] ?? "123"
            };
        }
    }

    public class BrowserOptions
    {
        public bool Headless { get; set; }
        public string WindowSize { get; set; } = "1920x1080";
        public bool DisableNotifications { get; set; }
        public bool DisablePopupBlocking { get; set; }
    }

    public class TestData
    {
        public string SearchQuery { get; set; } = string.Empty;
        public string ValidEmail { get; set; } = string.Empty;
        public string ValidPassword { get; set; } = string.Empty;
        public string InvalidEmail { get; set; } = string.Empty;
        public string InvalidPassword { get; set; } = string.Empty;
    }
}