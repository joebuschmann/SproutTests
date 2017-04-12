using System.Configuration;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace SproutTests
{
    [Binding]
    public class LoginForm
    {
        private readonly IWebDriver _webDriver;

        public LoginForm(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        [Given(@"I have logged into my Sprout account")]
        public void Login()
        {
            _webDriver.Navigate().GoToUrl(ConfigurationManager.AppSettings["loginUrl"]);

            IWebElement username = _webDriver.FindElement(By.CssSelector("form input[type=email]"));
            IWebElement password = _webDriver.FindElement(By.CssSelector("form input[type=password]"));

            username.SendKeys(ConfigurationManager.AppSettings["username"]);
            password.SendKeys(ConfigurationManager.AppSettings["password"]);

            IWebElement form = _webDriver.FindElement(By.Id("signin_form"));
            form.Submit();
        }
    }
}
