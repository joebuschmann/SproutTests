using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using TechTalk.SpecFlow;

namespace SproutTests
{
    [Binding]
    public class SproutAppSteps : IDisposable
    {
        private readonly IWebDriver _webDriver;
        private DateTime _scheduledDate = DateTime.MinValue;

        public SproutAppSteps()
        {
            _webDriver = new ChromeDriver();
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
            WaitForLoad.Execute(() =>
            {
                IWebElement composeButton = _webDriver.FindElement(By.CssSelector("a[href=\"#compose\"]"));
                composeButton.Click();
            });
        }

        [When(@"I enter the following text")]
        public void EnterTweet(string tweet)
        {
            WaitForLoad.Execute(() =>
            {
                IWebElement tweetTextbox = _webDriver.FindElement(By.CssSelector("form#compose-form article.messagetext textarea"));
                tweetTextbox.SendKeys(tweet);
            });
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
            IWebElement sentMessage = null;
            var messageText = "";

            // Wait with a shorter interval but more reps for the alert message.
            WaitForLoad.Execute(() =>
            {
                sentMessage = _webDriver.FindElement(By.CssSelector("header div.passive_alert"));
                messageText = sentMessage.Text;
            }, 10, 100);

            Assert.AreEqual("Message has been sent!", messageText);
        }

        [When(@"I navigate to the Messages tab")]
        public void NavigateToMessages()
        {
            WaitForLoad.Execute(() =>
            {
                IWebElement messagesButton = _webDriver.FindElement(By.CssSelector("a[href=\"/messages/smart/\"]"));
                messagesButton.Click();
            });
        }

        [Then(@"I should have messages in my smart inbox")]
        public void MessagesShouldExist()
        {
            Thread.Sleep(6000);
            var messages = _webDriver.FindElements(By.CssSelector("div#recent_msgs article[data-qa-message-network=\"twitter\"]"));
            Assert.Greater(messages.Count, 0);
        }

        [When(@"I reply to the first tweet with the following text")]
        public void ReplyToFirstTweet(string tweet)
        {
            IWebElement message =
                _webDriver.FindElement(By.CssSelector("div#recent_msgs article[data-qa-message-network=\"twitter\"]"));

            IWebElement replyButton = message.FindElement(By.CssSelector("a[title=\"Reply\"]"));

            Actions actions = new Actions(_webDriver);
            actions.MoveToElement(replyButton).Perform();
            _webDriver.FindElement(By.CssSelector("div#recent_msgs article[data-qa-message-network=\"twitter\"]"))
                    .FindElement(By.CssSelector("a[title=\"Reply\"]")).Click();

            WaitForLoad.Execute(() =>
            {
                IWebElement tweetTextbox = _webDriver.FindElement(By.CssSelector("form#compose-form article.messagetext textarea"));
                tweetTextbox.SendKeys(tweet);

                IWebElement sendButton = _webDriver.FindElement(By.CssSelector("form#compose-form .primary-action"));
                sendButton.Click();
            });
        }

        [Given(@"I navigate to the Publishing tab")]
        public void NavigateToPublishing()
        {
            WaitForLoad.Execute(() =>
            {
                IWebElement publishingButton = _webDriver.FindElement(By.CssSelector("a[href=\"/publishing/\"]"));
                publishingButton.Click();
            });
        }

        [Given(@"choose the Calendar option")]
        public void ChooseCalendarOption()
        {
            WaitForLoad.Execute(() =>
            {
                IWebElement calendarOption = _webDriver.FindElement(By.CssSelector("section#app-container nav#actions a[href=\"/publishing/calendar/\"]"));
                calendarOption.Click();
            });
        }

        [When(@"I compose the message")]
        public void ComposeTheMessage(string tweet)
        {
            WaitForLoad.Execute(() =>
            {
                IWebElement composeButton = _webDriver.FindElement(By.CssSelector("a[href=\"#compose\"]"));
                composeButton.Click();
            });

            EnterTweet(tweet);
        }

        [When(@"schedule the post for one hour from now")]
        public void ScheduleTweet()
        {
            IWebElement calendarButton = _webDriver.FindElement(By.CssSelector("form#compose-form button[data-qa-button=\"Send Later\"]"));
            calendarButton.Click();

            // Give the calendar time to render.
            // ToDo: Get rid of sleep. Replace with wait.
            Thread.Sleep(1000);

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
            ReadOnlyCollection<IWebElement> dayButtons =
                _webDriver.FindElements(
                    By.CssSelector("form#compose-form .scheduling table.ui-datepicker-calendar tbody td a"));

            IWebElement dayButton = dayButtons.FirstOrDefault(b => b.Text == day.ToString());

            if (dayButton == null)
                Assert.Fail("Unable to select the calendate day " + day + ".");

            dayButton.Click();
        }

        private void SelectHour(int hour)
        {
            IWebElement hourInput = _webDriver.FindElement(By.CssSelector("input[data-time-field=hour]"));
            hourInput.Click();

            var hourOptions = _webDriver.FindElements(By.CssSelector("div#sprouttime-autocomplete ul:nth-child(1) a"));

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

            var minuteOptions = _webDriver.FindElements(By.CssSelector("div#sprouttime-autocomplete ul:nth-child(2) a"));

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

            Thread.Sleep(1000);

            var calendarItems =
                _webDriver.FindElements(
                    By.CssSelector(
                        string.Format(
                            "section#publishing_calendar td.WeeklyCalendar-data-day-cell.{0} div.WeeklyCalendar-timebucket div[data-hour=\"{1}\"]",
                            dayOfWeekMarker, _scheduledDate.Hour)));

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

            // Selector to get the day headers of the calendar
            // section#publishing_calendar table.WeeklyCalendar table.WeeklyCalendar-thead-titles th.WeeklyCalendar-data-day-header

            // Selector to get the calendar item
            // section#publishing_calendar table.WeeklyCalendar table.WeeklyCalendar-data td.WeeklyCalendar-data-day-cell div.WeeklyCalendar-timebucket div[data-hour="15"]

            // Selectors to get the data
            // div.WeeklyCalendar-data-day-byline
            // 3:55 pm
            // div.calendar-data-message-text
            // Tweet text

        }

        [AfterScenario]
        public void Dispose()
        {
            _webDriver.Quit();
        }
    }
}
