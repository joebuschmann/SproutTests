
# Sprout Tests

This .NET solution validates the behavior of the Sprout Social web app at https://app.sproutsocial.com/login using Specflow and Selenium. To run the tests, update the credentials in App.config, build the solution, and run with your favorite NUnit test runner. Alternatively, you can run runtests.bat which will open the NUnit GUI and execute the tests.

The Gherkin scenarios are in the SproutApp.feature file and describe three scenarios for sending tweets.

## SproutAppSteps

This class defines bindings for its corresponding feature file. Common steps have been separated out into `LoginForm` and `ComposeTweetForm`.

## LoadDriver

The LoadDriver class is responsible for putting the correct `WebDriver` instance into the Specflow container. Currently, it just supports Chrome, but it could be enhanced to include other browsers such as Firefox and IE. It could also be updated to support `RemoveWebDriver` if this test suite were to be integrated into a CI build.

## LoginForm

This class encapsulates methods for logging into the Sprout app with a username and password.

## ComposeTweetForm

`ComposeTweetForm` handles the modal form used to compose and schedule tweets and is invoked by all three test scenarios.
