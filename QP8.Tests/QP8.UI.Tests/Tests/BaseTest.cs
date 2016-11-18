﻿using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using Nunit3AllureAdapter;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtension;
using SeleniumExtension.Support.Extensions;
using SeleniumExtension.Support.UI;
using Config = QP8.UI.Tests.Configuration.Config;

namespace QP8.UI.Tests.Tests
{
    [AllureTestFixture]
    public class BaseTest : AllureStepDefinition
    {
        protected RemoteWebDriver Driver;
        protected IWait Wait;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Driver = Setup.GridHub.GetBrowserInstance(DesiredCapabilities.Chrome());
            Driver.Manage().Timeouts().ImplicitlyWait(Config.Environment.ImplicitlyTimeout);
            Driver.Manage().Timeouts().SetPageLoadTimeout(Config.Environment.PageLoadTimeout);
            Driver.Manage().Timeouts().SetScriptTimeout(Config.Environment.JavaScriptTimeout);

            var failedActions = new Action[]
            {
                () =>
                {
                    var screenshot = Driver.GetFullPageScreenshot();
                    MakeAttachment(Image.FromStream(new MemoryStream(screenshot.AsByteArray)),"Exception screenshot", ImageType.imageJpeg);
                }
            };

            NUnit3AllureAdapterConfig.ActionsIfStepFailed = failedActions;
        }

        [SetUp]
        public void SetUp()
        {
            Driver.Manage().Cookies.DeleteAllCookies();
            Wait = ExtensionCore.GetWaiter();
            Wait.Timeout = Config.Environment.ImplicitlyTimeout;
            Wait.IgnoreExceptionTypes(typeof(NotFoundException), typeof(StaleElementReferenceException));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Driver.Dispose();
        }
    }
}