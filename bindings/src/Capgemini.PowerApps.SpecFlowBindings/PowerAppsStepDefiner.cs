﻿namespace Capgemini.PowerApps.SpecFlowBindings
{
    using System;
    using System.IO;
    using System.Reflection;
    using Capgemini.PowerApps.SpecFlowBindings.Configuration;
    using Microsoft.Dynamics365.UIAutomation.Api.UCI;
    using Microsoft.Dynamics365.UIAutomation.Browser;
    using OpenQA.Selenium;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    /// <summary>
    /// Base class for defining step bindings.
    /// </summary>
    public abstract class PowerAppsStepDefiner
    {
        private static TestConfiguration testConfig;

        [ThreadStatic]
        private static ITestDriver testDriver;

        [ThreadStatic]
        private static ITestDataRepository testDataRepository;

        [ThreadStatic]
        private static WebClient client;

        [ThreadStatic]
        private static XrmApp xrmApp;

        /// <summary>
        /// Gets the configuration for the test project.
        /// </summary>
        protected static TestConfiguration TestConfig
        {
            get
            {
                if (testConfig == null)
                {
                    var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), TestConfiguration.FileName);

                    testConfig = new DeserializerBuilder()
                        .WithTypeConverter(new BrowserCredentialsYamlTypeConverter())
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build()
                        .Deserialize<TestConfiguration>(File.ReadAllText(configPath));

                    if (testConfig.BrowserOptions.DriversPath == Path.Combine(Directory.GetCurrentDirectory()))
                    {
                        if (Directory.GetFiles(testConfig.BrowserOptions.DriversPath, "*driver.exe").Length == 0)
                        {
                            testConfig.BrowserOptions.DriversPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        }
                    }
                }

                return testConfig;
            }
        }

        /// <summary>
        /// Gets a repository provides access to test data.
        /// </summary>
        protected static ITestDataRepository TestDataRepository => testDataRepository ?? (testDataRepository = new FileDataRepository());

        /// <summary>
        /// Gets the EasyRepro WebClient.
        /// </summary>
        protected static WebClient Client => client ?? (client = new WebClient(TestConfig.BrowserOptions));

        /// <summary>
        /// Gets the EasyRepro XrmApp.
        /// </summary>
        protected static XrmApp XrmApp => xrmApp ?? (xrmApp = new XrmApp(Client));

        /// <summary>
        /// Gets the Selenium WebDriver.
        /// </summary>
        protected static IWebDriver Driver => Client.Browser.Driver;

        /// <summary>
        /// Gets provides utilities for test setup/teardown.
        /// </summary>
        protected static ITestDriver TestDriver => testDriver ?? (testDriver = new TestDriver((IJavaScriptExecutor)Driver));

        /// <summary>
        /// Performs any cleanup necessary when quitting the WebBrowser.
        /// </summary>
        protected static void Quit()
        {
            if (xrmApp == null)
            {
                return;
            }

            xrmApp.Dispose();
            xrmApp = null;
            client = null;
            testDriver = null;
        }
    }
}
