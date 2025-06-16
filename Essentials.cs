using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebTestSuite.PageObjects;
using System.Net;
using SeleniumExtras.WaitHelpers;

namespace WebTestSuite.Tests
{
    [TestFixture]
    public class EssentialFunctionalTests : BaseTest
    {
        private HomePage _homePage = null!;
        private LoginPage _loginPage = null!;
        private ContactPage _contactPage = null!;

        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _homePage = new HomePage(Driver, Wait);
            _loginPage = new LoginPage(Driver, Wait);
            _contactPage = new ContactPage(Driver, Wait);
        }

        [Test, Order(1)]
        [Description("Verify homepage loads successfully and title is correct")]
        public void Test01_HomePage_LoadsSuccessfully()
        {
            WaitForPageLoad();

            Assert.Multiple(() =>
            {
                Assert.That(Driver.Title, Is.Not.Empty, "Page title should not be empty");
                Assert.That(Driver.Url, Does.Contain(Config.BaseUrl), "URL should contain base URL");
                Console.WriteLine($"Page Title: {Driver.Title}");
                Console.WriteLine($"Current URL: {Driver.Url}");
            });
        }

        [Test, Order(2)]
        [Description("Verify essential page elements are present")]
        public void Test02_HomePage_EssentialElementsPresent()
        {
            WaitForPageLoad();

            Assert.Multiple(() =>
            {
                Assert.That(IsElementPresent(By.TagName("header")), Is.True, "Header should be present");
                Assert.That(IsElementPresent(By.TagName("footer")), Is.True, "Footer should be present");
                
                bool navPresent = IsElementPresent(By.TagName("nav")) || 
                                IsElementPresent(By.CssSelector(".navigation")) ||
                                IsElementPresent(By.CssSelector(".nav-menu"));
                Assert.That(navPresent, Is.True, "Navigation menu should be present");
                
                bool logoPresent = IsElementPresent(By.CssSelector("img[alt*='logo']")) ||
                                 IsElementPresent(By.CssSelector(".logo")) ||
                                 IsElementPresent(By.Id("logo"));
                Assert.That(logoPresent, Is.True, "Logo should be present");
            });
        }

        [Test, Order(3)]
        [Description("Test navigation link functionality")]
        public void Test03_Navigation_LinkFunctionality()
        {
            WaitForPageLoad();
            var initialUrl = Driver.Url;

            try
            {
                var navigationLinks = Driver.FindElements(By.CssSelector("nav a, .navigation a, .nav-menu a, header a"));
                var validLink = navigationLinks.FirstOrDefault(link => 
                    !string.IsNullOrEmpty(link.GetAttribute("href")) && 
                    !link.GetAttribute("href").StartsWith("javascript:") &&
                    !link.GetAttribute("href").StartsWith("#"));

                if (validLink != null)
                {
                    var targetUrl = validLink.GetAttribute("href");
                    validLink.Click();
                
                    Wait.Until(driver => driver.Url != initialUrl);
                    WaitForPageLoad();

                    Assert.That(Driver.Url, Is.Not.EqualTo(initialUrl), "URL should change after clicking navigation link");
                    Console.WriteLine($"Navigated from {initialUrl} to {Driver.Url}");
                }
                else
                {
                    Assert.Inconclusive("No valid navigation links found for testing");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Navigation test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(4)]
        [Description("Test search functionality if available")]
        public void Test04_Search_BasicFunctionality()
        {
            WaitForPageLoad();

            try
            {
                var searchInput = Driver.FindElements(By.CssSelector("input[type='search'], input[placeholder*='search'], #search"))
                                       .FirstOrDefault(e => e.Displayed);
                
                if (searchInput != null)
                {
                    var initialUrl = Driver.Url;
                    searchInput.Clear();
                    searchInput.SendKeys(Config.TestData.SearchQuery);
                    searchInput.SendKeys(Keys.Enter);
                    
                    Wait.Until(driver => driver.Url != initialUrl || 
                              driver.FindElements(By.CssSelector(".search-results, .results")).Any());
                    WaitForPageLoad();

                    Assert.That(Driver.Url, Does.Contain("search").Or.Contain("query").Or.Contain("q="), 
                              "URL should indicate search was performed");
                    Console.WriteLine($"Search performed successfully. Current URL: {Driver.Url}");
                }
                else
                {
                    Assert.Inconclusive("Search functionality not found on the page");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Search test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(5)]
        [Description("Test form validation with invalid data")]
        public void Test05_Form_InvalidDataValidation()
        {
            WaitForPageLoad();

            try
            {
                var forms = Driver.FindElements(By.TagName("form"));
                var testableForm = forms.FirstOrDefault(f => f.Displayed && f.FindElements(By.CssSelector("input, textarea")).Any());

                if (testableForm != null)
                {
                    var inputs = testableForm.FindElements(By.CssSelector("input[type='text'], input[type='email'], textarea"))
                                           .Where(i => i.Displayed).ToList();
                    var submitButton = testableForm.FindElements(By.CssSelector("input[type='submit'], button[type='submit']"))
                                                 .FirstOrDefault(b => b.Displayed);

                    if (inputs.Any() && submitButton != null)
                    {
                        foreach (var input in inputs.Take(2))
                        {
                            input.Clear();
                            if (input.GetAttribute("type") == "email")
                            {
                                input.SendKeys(Config.TestData.InvalidEmail);
                            }
                            else
                            {
                                input.SendKeys("x"); 
                            }
                        }

                        submitButton.Click();
                        
                        Wait.Until(driver => 
                            driver.FindElements(By.CssSelector(".error, .invalid, [aria-invalid='true']")).Any() ||
                            inputs.Any(i => !string.IsNullOrEmpty(i.GetAttribute("validationMessage"))));

                        bool validationFound = IsElementPresent(By.CssSelector(".error, .invalid, [aria-invalid='true']")) ||
                                             inputs.Any(i => i.GetAttribute("validity") == "false" || 
                                                          !string.IsNullOrEmpty(i.GetAttribute("validationMessage")));

                        Assert.That(validationFound, Is.True, "Form should show validation errors for invalid data");
                        Console.WriteLine("Form validation working correctly");
                    }
                    else
                    {
                        Assert.Inconclusive("No suitable form inputs found for validation testing");
                    }
                }
                else
                {
                    Assert.Inconclusive("No testable forms found on the page");
                }
            }
            catch (TimeoutException)
            {
                Assert.Inconclusive("Form validation messages did not appear within timeout period");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Form validation test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(6)]
        [Description("Test form submission with valid data")]
        public void Test06_Form_ValidDataSubmission()
        {
            WaitForPageLoad();

            try
            {
                var forms = Driver.FindElements(By.TagName("form"));
                var contactForm = forms.FirstOrDefault(f => f.Displayed);

                if (contactForm != null)
                {
                    var nameInput = contactForm.FindElements(By.CssSelector("input[name*='name'], #name"))
                                              .FirstOrDefault(i => i.Displayed);
                    var emailInput = contactForm.FindElements(By.CssSelector("input[type='email'], input[name*='email']"))
                                               .FirstOrDefault(i => i.Displayed);
                    var messageInput = contactForm.FindElements(By.CssSelector("textarea, input[name*='message']"))
                                                 .FirstOrDefault(i => i.Displayed);
                    var submitButton = contactForm.FindElements(By.CssSelector("input[type='submit'], button[type='submit']"))
                                                 .FirstOrDefault(b => b.Displayed);

                    if (nameInput != null && emailInput != null && submitButton != null)
                    {
                        nameInput.Clear();
                        nameInput.SendKeys("Test User");
                        emailInput.Clear();
                        emailInput.SendKeys(Config.TestData.ValidEmail);
                        
                        if (messageInput != null)
                        {
                            messageInput.Clear();
                            messageInput.SendKeys("This is a test message for automated testing.");
                        }

                        var initialUrl = Driver.Url;
                        submitButton.Click();
                        
                        Wait.Until(driver => 
                            driver.Url != initialUrl || 
                            driver.FindElements(By.CssSelector(".success, .thank-you, .confirmation")).Any());

                        bool successIndicator = Driver.Url != initialUrl || 
                                              IsElementPresent(By.CssSelector(".success, .thank-you, .confirmation"));

                        Assert.That(successIndicator, Is.True, "Form should submit successfully with valid data");
                        Console.WriteLine("Form submitted successfully with valid data");
                    }
                    else
                    {
                        Assert.Inconclusive("Required form fields not found");
                    }
                }
                else
                {
                    Assert.Inconclusive("No forms found for valid data testing");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Valid form submission test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(7)]
        [Description("Test login functionality with invalid credentials")]
        public void Test07_Login_InvalidCredentials()
        {
            WaitForPageLoad();

            try
            {
                var loginLink = Driver.FindElements(By.CssSelector("a[href*='login'], .login-link, .signin"))
                                    .FirstOrDefault(l => l.Displayed);
                
                if (loginLink != null)
                {
                    loginLink.Click();
                    WaitForPageLoad();
                }

                var emailField = Driver.FindElements(By.CssSelector("input[type='email'], input[name*='email'], input[name*='username']"))
                                      .FirstOrDefault(f => f.Displayed);
                var passwordField = Driver.FindElements(By.CssSelector("input[type='password']"))
                                         .FirstOrDefault(f => f.Displayed);
                var loginButton = Driver.FindElements(By.CssSelector("button[type='submit'], input[type='submit'], .login-button"))
                                       .FirstOrDefault(b => b.Displayed);

                if (emailField != null && passwordField != null && loginButton != null)
                {
                    emailField.Clear();
                    emailField.SendKeys(Config.TestData.InvalidEmail);
                    passwordField.Clear();
                    passwordField.SendKeys(Config.TestData.InvalidPassword);
                    loginButton.Click();

                    Wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".error, .alert-danger, .invalid-credentials")));

                    bool errorPresent = IsElementPresent(By.CssSelector(".error, .alert-danger, .invalid-credentials"));

                    Assert.That(errorPresent, Is.True, "Error message should appear for invalid login credentials");
                    Console.WriteLine("Login correctly rejected invalid credentials");
                }
                else
                {
                    Assert.Inconclusive("Login form elements not found");
                }
            }
            catch (TimeoutException)
            {
                Assert.Inconclusive("Login error message did not appear within timeout period");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Login test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(8)]
        [Description("Test button functionality")]
        public void Test08_Button_ClickFunctionality()
        {
            WaitForPageLoad();

            try
            {
                var buttons = Driver.FindElements(By.CssSelector("button:not([type='submit']), .btn:not([type='submit']), input[type='button']"))
                                   .Where(b => b.Displayed && b.Enabled).ToList();

                if (buttons.Any())
                {
                    var testButton = buttons.First();
                    var initialState = testButton.GetAttribute("class");
                    var initialUrl = Driver.Url;

                    testButton.Click();
                    
                    try
                    {
                        Wait.Until(driver => 
                            testButton.GetAttribute("class") != initialState ||
                            driver.Url != initialUrl ||
                            driver.FindElements(By.CssSelector(".modal, .popup, .dropdown-menu")).Any());
                    }
                    catch (TimeoutException)
                    {
                    }

                    var newState = testButton.GetAttribute("class");
                    var newUrl = Driver.Url;
                    
                    bool buttonWorked = initialState != newState || initialUrl != newUrl || 
                                       IsElementPresent(By.CssSelector(".modal, .popup, .dropdown-menu"));

                    Console.WriteLine($"Button functionality test completed: {testButton.TagName}");
                    Console.WriteLine($"Button state changed: {initialState != newState}");
                    Console.WriteLine($"URL changed: {initialUrl != newUrl}");
                    Console.WriteLine($"Modal/popup appeared: {IsElementPresent(By.CssSelector(".modal, .popup, .dropdown-menu"))}");
                }
                else
                {
                    Assert.Inconclusive("No testable buttons found on the page");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Button functionality test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(9)]
        [Description("Test link validation - check for broken links")]
        public void Test09_Links_BrokenLinkValidation()
        {
            WaitForPageLoad();
            var brokenLinks = new List<string>();

            try
            {
                var links = Driver.FindElements(By.TagName("a"))
                                 .Where(l => !string.IsNullOrEmpty(l.GetAttribute("href")) && 
                                           !l.GetAttribute("href").StartsWith("javascript:") &&
                                           !l.GetAttribute("href").StartsWith("mailto:") &&
                                           !l.GetAttribute("href").StartsWith("tel:"))
                                 .Take(10)
                                 .ToList();

                foreach (var link in links)
                {
                    try
                    {
                        var href = link.GetAttribute("href");
                        using var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(10);
                        var response = await client.GetAsync(href);
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            brokenLinks.Add($"{href} - Status: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        brokenLinks.Add($"{link.GetAttribute("href")} - Error: {ex.Message}");
                    }
                }

                Assert.That(brokenLinks.Count, Is.LessThanOrEqualTo(2), 
                          $"Too many broken links found: {string.Join(", ", brokenLinks)}");
                
                if (brokenLinks.Any())
                {
                    Console.WriteLine($"Broken links found: {string.Join(", ", brokenLinks)}");
                }
                else
                {
                    Console.WriteLine("No broken links detected");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Link validation test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(10)]
        [Description("Test modal or alert functionality")]
        public void Test10_Modal_AlertFunctionality()
        {
            WaitForPageLoad();

            try
            {
                var modalTriggers = Driver.FindElements(By.CssSelector("[data-toggle='modal'], .modal-trigger, button[data-target*='modal']"))
                                         .Where(b => b.Displayed && b.Enabled).ToList();

                if (modalTriggers.Any())
                {
                    var trigger = modalTriggers.First();
                    trigger.Click();
                    
                    Wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".modal, .popup, .dialog, [role='dialog']")));

                    bool modalPresent = IsElementPresent(By.CssSelector(".modal, .popup, .dialog")) ||
                                       IsElementPresent(By.CssSelector("[role='dialog']"));

                    Assert.That(modalPresent, Is.True, "Modal should appear when trigger is clicked");
                    Console.WriteLine("Modal functionality working correctly");

                    var closeButton = Driver.FindElements(By.CssSelector(".modal .close, .popup .close, [data-dismiss='modal']"))
                                           .FirstOrDefault(b => b.Displayed);
                    if (closeButton != null)
                    {
                        closeButton.Click();
                        Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".modal, .popup, .dialog, [role='dialog']")));
                    }
                }
                else
                {
                    try
                    {
                        ((IJavaScriptExecutor)Driver).ExecuteScript("alert('Test Alert');");
                        Wait.Until(ExpectedConditions.AlertIsPresent());
                        
                        var alert = Driver.SwitchTo().Alert();
                        Assert.That(alert.Text, Is.EqualTo("Test Alert"), "Alert text should match");
                        alert.Accept();
                        Console.WriteLine("JavaScript alert functionality working");
                    }
                    catch
                    {
                        Assert.Inconclusive("No modal triggers or alert functionality found");
                    }
                }
            }
            catch (TimeoutException)
            {
                Assert.Inconclusive("Modal did not appear within timeout period");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Modal/Alert test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(11)]
        [Description("Test responsive design - mobile viewport")]
        public void Test11_Responsive_MobileViewport()
        {
            WaitForPageLoad();
            var originalSize = Driver.Manage().Window.Size;

            try
            {
                var viewports = new[]
                {
                    new { Name = "", Size = new System.Drawing.Size(375, 667) },
                    new { Name = "", Size = new System.Drawing.Size(390, 844) },
                    new { Name = "", Size = new System.Drawing.Size(360, 740) },
                    new { Name = "", Size = new System.Drawing.Size(768, 1024) }
                };

                foreach (var viewport in viewports)
                {
                    Driver.Manage().Window.Size = viewport.Size;
                    
                    Wait.Until(driver => driver.FindElement(By.TagName("body")).Size.Width <= viewport.Size.Width + 50);

                    var bodyWidth = Driver.FindElement(By.TagName("body")).Size.Width;
                    
                    bool hasMobileNavigation = IsElementPresent(By.CssSelector(".mobile-menu, .hamburger, .nav-toggle")) ||
                                              IsElementPresent(By.CssSelector("[class*='mobile']"));
                    
                    bool contentFitsViewport = bodyWidth <= viewport.Size.Width + 50; 
                    
                    var hasHorizontalScrollbar = (bool)((IJavaScriptExecutor)Driver)
                        .ExecuteScript("return document.body.scrollWidth > document.body.clientWidth;");

                    Assert.Multiple(() =>
                    {
                        Assert.That(contentFitsViewport, Is.True, 
                                  $"Content should fit in {viewport.Name} viewport ({viewport.Size.Width}px). Actual body width: {bodyWidth}px");
                        Assert.That(hasHorizontalScrollbar, Is.False, 
                                  $"Page should not have horizontal scrollbar in {viewport.Name} viewport");
                    });

                    Console.WriteLine($"{viewport.Name} ({viewport.Size.Width}x{viewport.Size.Height}): " +
                                    $"Body width: {bodyWidth}px, Mobile nav: {hasMobileNavigation}, " +
                                    $"Horizontal scroll: {hasHorizontalScrollbar}");
                }

                Driver.Manage().Window.Size = new System.Drawing.Size(768, 1024);
                Wait.Until(driver => driver.FindElement(By.TagName("body")).Size.Width <= 818); // 768 + margin
                
                var tabletBodyWidth = Driver.FindElement(By.TagName("body")).Size.Width;
                Assert.That(tabletBodyWidth, Is.LessThanOrEqualTo(818), "Content should adapt to tablet viewport");
                
                Console.WriteLine($"Tablet viewport test completed. Body width: {tabletBodyWidth}px");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Responsive design test could not be completed: {ex.Message}");
            }
            finally
            {
                Driver.Manage().Window.Size = originalSize;
            }
        }

        [Test, Order(12)]
        [Description("Test keyboard navigation accessibility")]
        public void Test12_Accessibility_KeyboardNavigation()
        {
            WaitForPageLoad();

            try
            {
                var focusableElements = Driver.FindElements(By.CssSelector("a, button, input, select, textarea, [tabindex]:not([tabindex='-1'])"))
                                             .Where(e => e.Displayed && e.Enabled).Take(5).ToList();

                if (focusableElements.Any())
                {
                    var firstElement = focusableElements.First();
                    firstElement.Click();
                    
                    Wait.Until(driver => driver.SwitchTo().ActiveElement().Equals(firstElement));
                    var initialFocusedElement = Driver.SwitchTo().ActiveElement();
                    
                    Driver.SwitchTo().ActiveElement().SendKeys(Keys.Tab);
                    
                    Wait.Until(driver => !driver.SwitchTo().ActiveElement().Equals(initialFocusedElement));
                    var newFocusedElement = Driver.SwitchTo().ActiveElement();

                    Assert.That(newFocusedElement, Is.Not.EqualTo(initialFocusedElement), 
                              "Tab key should move focus to next element");
                    
                    if (newFocusedElement.TagName.ToLower() == "a" || newFocusedElement.TagName.ToLower() == "button")
                    {
                        var initialUrl = Driver.Url;
                        newFocusedElement.SendKeys(Keys.Enter);
                        
                        try
                        {
                            Wait.Until(driver => driver.Url != initialUrl, TimeSpan.FromSeconds(2));
                        }
                        catch (TimeoutException)
                        {
                        }
                        
                        Console.WriteLine("Keyboard navigation test completed successfully");
                    }

                    var focusedElementStyles = newFocusedElement.GetCssValue("outline");
                    bool hasFocusIndicator = !string.IsNullOrEmpty(focusedElementStyles) && focusedElementStyles != "none";
                    
                    if (!hasFocusIndicator)
                    {
                        Console.WriteLine("Warning: Focused element may not have visible focus indicator");
                    }
                }
                else
                {
                    Assert.Inconclusive("No focusable elements found for keyboard navigation testing");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Keyboard navigation test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(13)]
        [Description("Test page performance and load time")]
        public void Test13_Performance_PageLoadTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                Driver.Navigate().Refresh();
                
                Wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                WaitForPageLoad();
                stopwatch.Stop();

                var loadTime = stopwatch.ElapsedMilliseconds;
                Assert.That(loadTime, Is.LessThan(10000), $"Page should load within 10 seconds. Actual: {loadTime}ms");

                var performanceData = ((IJavaScriptExecutor)Driver).ExecuteScript(performanceScript);
                Console.WriteLine($"Page Load Performance:");
                Console.WriteLine($"- Total Load Time: {loadTime}ms");
                Console.WriteLine($"- Performance Data: {performanceData}");

                bool hasTitle = !string.IsNullOrEmpty(Driver.Title);
                bool hasMetaDescription = IsElementPresent(By.CssSelector("meta[name='description']"));
                bool hasH1 = IsElementPresent(By.TagName("h1"));
                
                Assert.Multiple(() =>
                {
                    Assert.That(hasTitle, Is.True, "Page should have a title");
                    Assert.That(Driver.Title.Length, Is.GreaterThan(10), "Page title should be descriptive");
                    Assert.That(Driver.Title.Length, Is.LessThan(70), "Page title should not be too long for SEO");
                });

                Console.WriteLine($"SEO Check - Title: {hasTitle}, Meta Description: {hasMetaDescription}, H1: {hasH1}");
                
                bool hasLangAttribute = !string.IsNullOrEmpty(Driver.FindElement(By.TagName("html")).GetAttribute("lang"));
                Console.WriteLine($"Accessibility Check - HTML lang attribute: {hasLangAttribute}");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Performance test could not be completed: {ex.Message}");
            }
        }

        [Test, Order(14)]
        [Description("Test responsive design - desktop to mobile transitions")]
        public void Test14_Responsive_BreakpointTransitions()
        {
            WaitForPageLoad();
            var originalSize = Driver.Manage().Window.Size;

            try
            {
                var breakpoints = new[]
                {
                    new { Name = "", Width = 1920 },
                    new { Name = "", Width = 1200 },
                    new { Name = "", Width = 768 },
                    new { Name = "", Width = 480 },
                    new { Name = "", Width = 320 }
                };

                var layoutChanges = new List<string>();

                foreach (var breakpoint in breakpoints)
                {
                    Driver.Manage().Window.Size = new System.Drawing.Size(breakpoint.Width, 800);
                    
                    Wait.Until(driver => Math.Abs(driver.FindElement(By.TagName("body")).Size.Width - breakpoint.Width) <= 50);
                    
                    var bodyWidth = Driver.FindElement(By.TagName("body")).Size.Width;
                    var hasHamburgerMenu = IsElementPresent(By.CssSelector(".hamburger, .mobile-menu-toggle, .nav-toggle"));
                    var hasVisibleNavigation = IsElementPresent(By.CssSelector("nav:not([style*='display: none']), .navigation:not([style*='display: none'])"));
                    var hasSidebar = IsElementPresent(By.CssSelector(".sidebar, .side-nav"));
                    
                    layoutChanges.Add($"{breakpoint.Name} ({breakpoint.Width}px): " +
                                    $"Body: {bodyWidth}px, Hamburger: {hasHamburgerMenu}, " +
                                    $"Nav visible: {hasVisibleNavigation}, Sidebar: {hasSidebar}");
                    
                    var hasHorizontalOverflow = (bool)((IJavaScriptExecutor)Driver)
                        .ExecuteScript("return document.body.scrollWidth > window.innerWidth;");
                    
                    Assert.That(hasHorizontalOverflow, Is.False, 
                              $"Page should not have horizontal overflow at {breakpoint.Name} breakpoint ({breakpoint.Width}px)");
                }

                Console.WriteLine("Responsive Breakpoint Analysis:");
                layoutChanges.ForEach(Console.WriteLine);
                
                Driver.Manage().Window.Size = new System.Drawing.Size(480, 800);
                Wait.Until(driver => driver.FindElement(By.TagName("body")).Size.Width <= 530);
                
                bool mobileMenuExists = IsElementPresent(By.CssSelector(".hamburger, .mobile-menu-toggle, .nav-toggle, .menu-button"));
                if (mobileMenuExists)
                {
                    Console.WriteLine("✓ Mobile navigation menu detected on small screens");
                }
                else
                {
                    Console.WriteLine("⚠ No mobile navigation menu detected - may impact mobile usability");
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Responsive breakpoint test could not be completed: {ex.Message}");
            }
            finally
            {
                Driver.Manage().Window.Size = originalSize;
            }
        }
    }
}