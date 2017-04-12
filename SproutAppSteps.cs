using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;

namespace SproutTests
{
    [Binding]
    public class SproutAppSteps : IDisposable
    {
        private readonly IWebDriver _webDriver;
        private readonly IWait<IWebDriver> _shortWait;
        private readonly IWait<IWebDriver> _longWait;
        private DateTime _scheduledDate = DateTime.MinValue;

        public SproutAppSteps(IWebDriver webDriver)
        {
            _webDriver = webDriver;

            _shortWait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(5))
            {
                PollingInterval = TimeSpan.FromMilliseconds(100)
            };

            _longWait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(8))
            {
                PollingInterval = TimeSpan.FromSeconds(1)
            };
        }

        [Given(@"I have logged into my Sprout account")]
        public void Login()
        {
            _webDriver.Navigate().GoToUrl("https://app.sproutsocial.com/login");

            IWebElement username = _webDriver.FindElement(By.CssSelector("form input[type=email]"));
            IWebElement password = _webDriver.FindElement(By.CssSelector("form input[type=password]"));

            username.SendKeys(ConfigurationManager.AppSettings["username"]);
            password.SendKeys(ConfigurationManager.AppSettings["password"]);

            IWebElement form = _webDriver.FindElement(By.Id("signin_form"));
            form.Submit();
        }

        [Given(@"I compose a Twitter message")]
        public void ComposeTwitterMessage()
        {
            IWebElement composeButton = _longWait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("a[href=\"#compose\"]")));
            composeButton.Click();
        }

        [When(@"I enter the following text")]
        public void EnterTweet(string tweet)
        {
            IWebElement tweetTextbox =
                _shortWait.Until(
                    ExpectedConditions.ElementIsVisible(By.CssSelector("form#compose-form article.messagetext textarea")));

            tweetTextbox.SendKeys(tweet);
        }

        [When(@"I send it")]
        public void Send()
        {
            IWebElement sendButton = _webDriver.FindElement(By.CssSelector("form#compose-form .primary-action"));
            sendButton.Click();
        }

        [Then(@"the message will be sent successfully")]
        public void ValidateSuccessfulTwitterSend()
        {
            // The alert message will popup and fade.
            IWebElement sentMessage =
                _shortWait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("header div.passive_alert")));

            Assert.AreEqual("Message has been sent!", sentMessage.Text);
        }

        [When(@"I navigate to the Messages tab")]
        public void NavigateToMessages()
        {
            IWebElement messagesButton =
                _longWait.Until(d => d.FindElement(By.CssSelector("a[href=\"/messages/smart/\"]")));

            messagesButton.Click();
        }

        [Then(@"I should have messages in my smart inbox")]
        public void MessagesShouldExist()
        {
            ReadOnlyCollection<IWebElement> messages = _longWait.Until(d =>
            {
                var msgs =
                    d.FindElements(
                        By.CssSelector("div#recent_msgs article[data-qa-message-network=\"twitter\"]"));

                return msgs.Count == 0 ? null : msgs;
            });

            Assert.Greater(messages.Count, 0);
        }

        [When(@"I reply to the first tweet with the following text")]
        public void ReplyToFirstTweet(string tweet)
        {
            IWebElement message =
                _webDriver.FindElement(By.CssSelector("div#recent_msgs article[data-qa-message-network=\"twitter\"]"));

            IWebElement replyButton = message.FindElement(By.CssSelector("a[title=\"Reply\"]"));

            // Make the action buttons on the first tweet appear so it can be clicked
            Actions actions = new Actions(_webDriver);
            actions.MoveToElement(replyButton).Perform();
            _webDriver.FindElement(By.CssSelector("div#recent_msgs article[data-qa-message-network=\"twitter\"]"))
                    .FindElement(By.CssSelector("a[title=\"Reply\"]")).Click();

            // Enter and send the tweet
            EnterTweet(tweet);
            Send();
        }

        [Given(@"I navigate to the Publishing tab")]
        public void NavigateToPublishing()
        {
            IWebElement publishingButton =
                _longWait.Until(d => d.FindElement(By.CssSelector("a[href=\"/publishing/\"]")));

            publishingButton.Click();
        }

        [Given(@"choose the Calendar option")]
        public void ChooseCalendarOption()
        {
            IWebElement calendarOption =
                _shortWait.Until(
                    d =>
                        d.FindElement(
                            By.CssSelector("section#app-container nav#actions a[href=\"/publishing/calendar/\"]")));

            calendarOption.Click();
        }

        [When(@"I compose the message")]
        public void ComposeTheMessage(string tweet)
        {
            IWebElement composeButton = _shortWait.Until(d => d.FindElement(By.CssSelector("a[href=\"#compose\"]")));
            composeButton.Click();

            EnterTweet(tweet);
        }

        [When(@"schedule the post for one hour from now")]
        public void ScheduleTweet()
        {
            IWebElement calendarButton = _webDriver.FindElement(By.CssSelector("form#compose-form button[data-qa-button=\"Send Later\"]"));
            calendarButton.Click();

            _scheduledDate = DateTime.Now.AddHours(1);
            int day = _scheduledDate.Day;

            SelectDay(day);

            int hour = _scheduledDate.Hour > 12 ? _scheduledDate.Hour - 12 : _scheduledDate.Hour;
            SelectHour(hour);
            SelectMinute(_scheduledDate.Minute);

            // Schedule the tweet.
            Send();
        }

        private void SelectDay(int day)
        {
            ReadOnlyCollection<IWebElement> dayButtons = _shortWait.Until(d =>
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

            ReadOnlyCollection<IWebElement> hourOptions = _shortWait.Until(d =>
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

            ReadOnlyCollection<IWebElement> minuteOptions = _shortWait.Until(d =>
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

        [Then(@"the following message should appear on the calendar")]
        public void PostAppearsOnCalendar(string expectedTweet)
        {
            string dayOfWeekMarker = "_" + _scheduledDate.DayOfWeek.ToString().ToLower();

            // Get the cell with the scheduled tweet
            // TODO: Simplify by figuring out how to convert from the value in data-bucket-date attribute.

            ReadOnlyCollection<IWebElement> calendarItems = _longWait.Until(d =>
            {
                ReadOnlyCollection<IWebElement> elements =
                    d.FindElements(
                        By.CssSelector(string.Format(
                            "section#publishing_calendar td.WeeklyCalendar-data-day-cell.{0} div.WeeklyCalendar-timebucket div[data-hour=\"{1}\"]",
                            dayOfWeekMarker, _scheduledDate.Hour)));

                return elements.Count == 0 ? null : elements;
            });

            string expectedTime = _scheduledDate.ToString("h:mm tt").ToLower();
            string actualTime = null;
            string actualTweet = null;

            foreach (var calendarItem in calendarItems)
            {
                string time = calendarItem.FindElement(By.CssSelector("div.WeeklyCalendar-data-day-byline")).Text;

                if (time == expectedTime)
                {
                    actualTime = time;
                    actualTweet = calendarItem.FindElement(By.CssSelector("div.calendar-data-message-text")).Text;
                }
            }

            Assert.AreEqual(_scheduledDate.ToString("h:mm tt").ToLower(), actualTime);
            Assert.AreEqual(expectedTweet, actualTweet);
        }

        [AfterScenario]
        public void Dispose()
        {
            _webDriver.Quit();
        }
    }
}
