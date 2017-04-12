using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;

namespace SproutTests
{
    [Binding]
    public class ComposeTweetForm
    {
        private readonly IWebDriver _webDriver;
        private readonly IWait<IWebDriver> _wait;

        public ComposeTweetForm(IWebDriver webDriver)
        {
            _webDriver = webDriver;

            _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(5))
            {
                PollingInterval = TimeSpan.FromMilliseconds(100)
            };
        }

        [When(@"I enter the following text")]
        public void EnterMessage(string message)
        {
            IWebElement tweetTextbox =
                _wait.Until(
                    ExpectedConditions.ElementIsVisible(By.CssSelector("form#compose-form article.messagetext textarea")));

            tweetTextbox.SendKeys(message);
        }

        public void ScheduleTweet(DateTime scheduledDate)
        {
            IWebElement calendarButton = _webDriver.FindElement(By.CssSelector("form#compose-form button[data-qa-button=\"Send Later\"]"));
            calendarButton.Click();

            int day = scheduledDate.Day;
            SelectDay(day);

            int hour = scheduledDate.Hour > 12 ? scheduledDate.Hour - 12 : scheduledDate.Hour;
            SelectHour(hour);
            SelectMinute(scheduledDate.Minute);
        }

        [When(@"I send it")]
        [When(@"send it")]
        public void Send()
        {
            IWebElement sendButton = _webDriver.FindElement(By.CssSelector("form#compose-form .primary-action"));
            sendButton.Click();
        }

        private void SelectDay(int day)
        {
            ReadOnlyCollection<IWebElement> dayButtons = _wait.Until(d =>
            {
                ReadOnlyCollection<IWebElement> elements =
                d.FindElements(
                    By.CssSelector("form#compose-form .scheduling table.ui-datepicker-calendar tbody td a"));

                return elements.Count == 0 ? null : elements;
            });

            IWebElement dayButton = dayButtons.FirstOrDefault(b => b.Text == day.ToString());

            if (dayButton == null)
                Assert.Fail("Unable to select the calendar day " + day + ".");

            dayButton.Click();
        }

        private void SelectHour(int hour)
        {
            IWebElement hourInput = _webDriver.FindElement(By.CssSelector("input[data-time-field=hour]"));
            hourInput.Click();

            ReadOnlyCollection<IWebElement> hourOptions = _wait.Until(d =>
            {
                ReadOnlyCollection<IWebElement> elements =
                d.FindElements(
                    By.CssSelector("div#sprouttime-autocomplete ul:nth-child(1) a"));

                return elements.Count == 0 ? null : elements;
            });

            IWebElement hourOption = hourOptions.FirstOrDefault(o => o.Text == hour.ToString());

            if (hourOption == null)
            {
                Assert.Fail("Unable to select the hour option.");
            }

            hourOption.Click();
        }

        private void SelectMinute(int minute)
        {
            IWebElement minuteInput = _webDriver.FindElement(By.CssSelector("input[data-time-field=minute]"));
            minuteInput.Click();

            ReadOnlyCollection<IWebElement> minuteOptions = _wait.Until(d =>
            {
                ReadOnlyCollection<IWebElement> elements =
                d.FindElements(
                    By.CssSelector("div#sprouttime-autocomplete ul:nth-child(2) a"));

                return elements.Count == 0 ? null : elements;
            });

            IWebElement minuteOption = minuteOptions.FirstOrDefault(o => o.Text == minute.ToString());

            if (minuteOption == null)
            {
                Assert.Fail("Unable to select the minute option.");
            }

            minuteOption.Click();
        }
    }
}
