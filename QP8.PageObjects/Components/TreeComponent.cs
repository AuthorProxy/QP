﻿using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements.Implementation;

namespace QP8.PageObjects.Components
{
    public class TreeComponent : Component
    {
        public TreeComponent(IWebElement webElement, IWebDriver webDriver) 
            : base(webElement, webDriver)
        {
        }
    }
}