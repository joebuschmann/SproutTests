using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;

namespace SproutTests
{
    [Binding]
    public class SproutAppSteps
    {
        private readonly IWebDriver _webDriver;
        private readonly ComposeTweetForm _composeTweetForm;
        private readonly IWait<IWebDriver> _shortWait;
        private readonly IWait<IWebDriver> _longWait;
        private DateTime _scheduledDate = DateTime.MinValue;

        public SproutAppSteps(IWebDriver webDriver, ComposeTweetForm composeTweetForm)
        {
            _webDriver = webDriver;
            _composeTweetForm = composeTweetForm;

            _shortWait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(5))
            {
                PollingInterval = TimeSpan.FromMilliseconds(100)
            };

            _longWait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(8))
            {
                PollingInterval = TimeSpan.FromSeconds(1)
            };
        }

        [Given(@"I compose a Twitter message")]
        public void ComposeTwitterMessage()
        {
            IWebElement composeButton = _longWait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("a[href=\"#compose\"]")));
            composeButton.Click();
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
            _composeTweetForm.EnterMessage(tweet);
            _composeTweetForm.Send();
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

            _composeTweetForm.EnterMessage(tweet);
        }

        [When(@"schedule the post for one hour from now")]
        public void ScheduleTweet()
        {
            _scheduledDate = DateTime.Now.AddHours(1);
            _composeTweetForm.ScheduleTweet(_scheduledDate);
        }

        [Then(@"the following message should appear on the calendar")]
        public void PostAppearsOnCalendar(string expectedTweet)
        {
            // Get the calendar item with the scheduled tweet
            string dayOfWeekMarker = "_" + _scheduledDate.DayOfWeek.ToString().ToLower();
            string expectedTime = _scheduledDate.ToString("h:mm tt").ToLower();

            // There is a data marker attribute "data-hour". Take advantage of it to select the correct list of calendar items.
            string hour = _scheduledDate.Hour < 10 ? "0" + _scheduledDate.Hour : _scheduledDate.Hour.ToString();

            IWebElement actualCalendarItem = _longWait.Until(d =>
            {
                ReadOnlyCollection<IWebElement> calendarItems =
                    d.FindElements(
                        By.CssSelector(string.Format(
                            "section#publishing_calendar td.WeeklyCalendar-data-day-cell.{0} div.WeeklyCalendar-timebucket div[data-hour=\"{1}\"]",
                            dayOfWeekMarker, hour)));

                if (calendarItems.Count == 0)
                {
                    // Item hasn't loaded yet. Return and try again.
                    return null;
                }

                // We have calendar items. Try to find the right one.
                foreach (var calendarItem in calendarItems)
                {
                    string time = calendarItem.FindElement(By.CssSelector("div.WeeklyCalendar-data-day-byline")).Text;

                    if (time == expectedTime)
                    {
                        return calendarItem;
                    }
                }

                return null;
            });

            Assert.IsNotNull(actualCalendarItem);

            string actualTime = actualCalendarItem.FindElement(By.CssSelector("div.WeeklyCalendar-data-day-byline")).Text;
            string actualTweet = actualCalendarItem.FindElement(By.CssSelector("div.calendar-data-message-text")).Text;

            Assert.AreEqual(_scheduledDate.ToString("h:mm tt").ToLower(), actualTime);
            Assert.AreEqual(expectedTweet, actualTweet);
        }
    }
}
