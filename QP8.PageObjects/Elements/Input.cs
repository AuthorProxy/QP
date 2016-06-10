﻿using System.Linq;
using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects.Elements;

namespace QP8.PageObjects.Elements
{
    /// <summary>
    /// Объектная модель поля ввода<br/>
    /// <code>
    /// Примеры вёрстки:
    /// <br/>
    /// 1.  &lt;dd class=&quot;field&quot;&gt; 
    ///         &lt;input class=&quot;input-validation-error textbox&quot; type=&quot;text&quot;&gt; 
    ///         &lt;em class=&quot;validators&quot;&gt;
    ///             &lt;span class=&quot;field-validation-error&quot; &gt;
    ///         &lt;/em&gt; 
    ///      &lt;/dd&gt;
    /// </code>
    /// </summary>
    public class Input : InputBasedElement, IEditable, IEnabled, IFocused
    {
        /// <summary>
        /// Проверка доступности поля ввода<br/>
        /// Выполняется путём проверки отсутствия атрибута 'disabled'<br/>
        /// и вызова свойства 'IWebElement.Enabled'
        /// </summary>
        public bool Enabled
        {
            get
            {
                var disabled = ProxyWebElement.FindElement(By.CssSelector("input")).GetAttribute("disabled");
                return string.IsNullOrEmpty(disabled) && ProxyWebElement.Enabled;
            }
        }

        /// <summary>
        /// Проверка наличия фокуса на поле ввода<br/>
        /// Выполняется путём получения активного элемента на странице и его сравнения с полем ввода
        /// </summary>
        public bool Focused
        {
            get { return ProxyWebElement.Equals(WebDriver.SwitchTo().ActiveElement()); }
        }

        /// <summary>
        /// Проверка валидации поля<br/>
        /// Выполняется путём проверки значения атрибута 'class' дочернего элемента 'input'<br/>
        /// на отсутствие параметра 'input-validation-error' или отсутствия текста валидации
        /// </summary>
        public bool Valid
        {
            get
            {
                var cssClass = ProxyWebElement.FindElement(By.CssSelector("input")).GetAttribute("class");

                return !string.IsNullOrEmpty(cssClass)
                    ? !cssClass.Contains("input-validation-error")
                    : string.IsNullOrEmpty(ValidationText);
            }
        }

        /// <summary>
        /// Получение текста валидации<br/>
        /// Выполняется путём поиска дочернего элемента 'span' с атрибутом 'class="field-validation-error"'<br/>
        /// и получения его текста
        /// </summary>
        public string ValidationText
        {
            get
            {
                var errors = ProxyWebElement.FindElements(By.CssSelector("span.field-validation-error"));
                return errors.Any() && errors.First().Displayed ? errors.First().Text : string.Empty;
            }
        }

        public Input(IWebElement webElement, IWebDriver webDriver)
            : base(webElement, webDriver)
        {
        }

        /// <summary>
        /// Удаление текста из поля ввода<br/>
        /// Выполяется путём вызова метода 'IWebElement.Clear()' дочернего элемента 'input'
        /// </summary>
        public void Clear()
        {
            ProxyWebElement.FindElement(By.CssSelector("input")).Clear();
        }

        /// <summary>
        /// Передача текста полю ввода<br/>
        /// Выполяется путём вызова метода 'IWebElement.SendKeys(string)' дочергено элемента 'input'
        /// </summary>
        /// <param name="text">Текст для передачи</param>
        public void SendKeys(string text)
        {
            ProxyWebElement.FindElement(By.CssSelector("input")).SendKeys(text);
        }
    }
}