using System.Collections.Generic;
using System.Data;
using OpenQA.Selenium;

namespace Monitor.Interfaces
{
    public interface IValidator
    {
        void validate(IWebDriver driver, string url, string title = null);
        void validate(IWebDriver driver, DataRow siteData);
        void registerNotifyer(INotifyer notifyer);
        void registerNotifyers(IEnumerable<INotifyer> notifyers);
    }
}