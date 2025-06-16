using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebTestSuite.Configuration;
using WebTestSuite.Utilities;

namespace WebTestSuite.Tests
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WebDriverWait Wait { get; private set; } = null!;
        protected TestConfiguration Config { get; private set; } = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Config = TestConfiguration.Instance;
        }

        [SetUp]
        public void SetUp()
        {
            InitializeDriver();
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(Config.TimeoutSeconds));
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Config.ImplicitWaitSeconds);
            Driver.Navigate().GoToUrl(Config.BaseUrl);
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                if (Config.ScreenshotOnFailure)
                {
                    ScreenshotHelper.TakeScreenshot(Driver, TestContext.CurrentContext.Test.Name);
                }
            }

            Driver?.Quit();
            Driver?.Dispose();
        }

        private void InitializeDriver()
        {
            var options = new ChromeOptions();
            
            if (Config.BrowserOptions.Headless)
            {
                options.AddArgument("--headless");
            }

            options.AddArgument($"--window-size={Config.BrowserOptions.WindowSize}");
            
            if (Config.BrowserOptions.DisableNotifications)
            {
                options.AddArgument("--disable-notifications");
            }

            if (Config.BrowserOptions.DisablePopupBlocking)
            {
                options.AddArgument("--disable-popup-blocking");
            }

            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");

            Driver = new ChromeDriver(options);
            Driver.Manage().Window.Maximize();
        }

        protected void NavigateToUrl(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        protected IWebElement WaitForElement(By locator, int timeoutSeconds = 0)
        {
            var timeout = timeoutSeconds > 0 ? timeoutSeconds : Config.TimeoutSeconds;
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
        }

        protected IWebElement WaitForClickableElement(By locator, int timeoutSeconds = 0)
        {
            var timeout = timeoutSeconds > 0 ? timeoutSeconds : Config.TimeoutSeconds;
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
        }

        protected void WaitForUrlToContain(string urlPart, int timeoutSeconds = 0)
        {
            var timeout = timeoutSeconds > 0 ? timeoutSeconds : Config.TimeoutSeconds;
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeout));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains(urlPart));
        }

        protected bool IsElementPresent(By locator)
        {
            try
            {
                Driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected void ScrollToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500); // Wait for scroll animation
        }

        protected void WaitForPageLoad()
        {
            Wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}