using System;
using System.Collections.Generic;
using System.Linq;
using BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;

namespace SproutTests
{
    [Binding]
    public class LoadDriver
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly FeatureContext _featureContext;
        private readonly IObjectContainer _container;
        private IWebDriver _webDriver;

        public LoadDriver(ScenarioContext scenarioContext, FeatureContext featureContext, IObjectContainer container)
        {
            _scenarioContext = scenarioContext;
            _featureContext = featureContext;
            _container = container;
        }

        [BeforeScenario]
        public void Load()
        {
            List<string> driverTags =
                _scenarioContext.ScenarioInfo.Tags.Where(
                    t => t.IndexOf("driver", StringComparison.OrdinalIgnoreCase) != -1).ToList();

            driverTags.AddRange(
                _featureContext.FeatureInfo.Tags.Where(
                    t => t.IndexOf("driver", StringComparison.OrdinalIgnoreCase) != -1));

            if (driverTags.Count == 0)
            {
                throw new Exception("No driver was specified. Please specify which driver to load with a tag.");
            }

            if (driverTags.Count > 1)
            {
                throw new Exception("More than one driver was specified.");
            }

            var driverTag = driverTags[0];
            _webDriver = GetWebDriver(driverTag);

            if (_webDriver == null)
            {
                throw new Exception("Unable to initialize the driver specified by the tag " + driverTag + ".");
            }

            _container.RegisterInstanceAs(_webDriver, typeof (IWebDriver));
        }

        private IWebDriver GetWebDriver(string tag)
        {
            if (tag.IndexOf("chrome", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return new ChromeDriver();
            }

            return null;
        }

        [AfterScenario]
        public void Dispose()
        {
            if (_webDriver != null)
            {
                _webDriver.Quit();
            }
        }
    }
}
