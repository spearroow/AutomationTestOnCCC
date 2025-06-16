using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace WebTestSuite.PageObjects
{
    public class HomePage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public HomePage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public IWebElement Logo => _driver.FindElement(By.CssSelector("img[alt*='logo'], .logo, #logo"));
        public IWebElement Header => _driver.FindElement(By.TagName("header"));
        public IWebElement Footer => _driver.FindElement(By.TagName("footer"));
        public IWebElement NavigationMenu => _driver.FindElement(By.CssSelector("nav, .navigation, .nav-menu"));
        public IWebElement SearchBox => _driver.FindElement(By.CssSelector("input[type='search'], input[placeholder*='search'], #search"));
        public IWebElement SearchButton => _driver.FindElement(By.CssSelector("button[type='submit'], .search-button, input[type='submit']"));
    
        public IWebElement LoginLink => _driver.FindElement(By.CssSelector("a[href*='login'], .login-link"));
        public IWebElement ContactLink => _driver.FindElement(By.CssSelector("a[href*='contact'], .contact-link"));
        public IWebElement AboutLink => _driver.FindElement(By.CssSelector("a[href*='about'], .about-link"));

        public void PerformSearch(string query)
        {
            SearchBox.Clear();
            SearchBox.SendKeys(query);
            SearchButton.Click();
        }

        public void ClickLoginLink()
        {
            LoginLink.Click();
        }
    }

    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public IWebElement EmailField => _driver.FindElement(By.CssSelector("input[type='email'], input[name*='email'], #email"));
        public IWebElement PasswordField => _driver.FindElement(By.CssSelector("input[type='password'], input[name*='password'], #password"));
        public IWebElement LoginButton => _driver.FindElement(By.CssSelector("button[type='submit'], .login-button, input[type='submit']"));
        public IWebElement ErrorMessage => _driver.FindElement(By.CssSelector(".error, .error-message, .alert-danger"));

        public void Login(string email, string password)
        {
            EmailField.Clear();
            EmailField.SendKeys(email);
            PasswordField.Clear();
            PasswordField.SendKeys(password);
            LoginButton.Click();
        }
    }

    public class ContactPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public ContactPage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public IWebElement NameField => _driver.FindElement(By.CssSelector("input[name*='name'], #name"));
        public IWebElement EmailField => _driver.FindElement(By.CssSelector("input[type='email'], input[name*='email'], #email"));
        public IWebElement MessageField => _driver.FindElement(By.CssSelector("textarea[name*='message'], #message"));
        public IWebElement SubmitButton => _driver.FindElement(By.CssSelector("button[type='submit'], .submit-button, input[type='submit']"));
        public IWebElement SuccessMessage => _driver.FindElement(By.CssSelector(".success, .success-message, .alert-success"));
        public IWebElement ErrorMessage => _driver.FindElement(By.CssSelector(".error, .error-message, .alert-danger"));

        public void FillContactForm(string name, string email, string message)
        {
            NameField.Clear();
            NameField.SendKeys(name);
            EmailField.Clear();
            EmailField.SendKeys(email);
            MessageField.Clear();
            MessageField.SendKeys(message);
        }

        public void SubmitForm()
        {
            SubmitButton.Click();
        }
    }
}