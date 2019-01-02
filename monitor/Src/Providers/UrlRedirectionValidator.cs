using System;
using System.Collections.Generic;
using System.Data;
using Monitor.Interfaces;
using OpenQA.Selenium;

namespace Monitor.Providers
{
    public class UrlRedirectionValidator : Validator
    {
        public UrlRedirectionValidator(Dictionary<string, object> eyesParams)
        {

        }
        public override void validate(IWebDriver driver, string url, string title = null)
        {
            if (driver.Url.ToLower().CompareTo(url.ToLower()) != 0)
                notify($"The urls didn't match for {title} \n Got: {driver.Url} \n Requred: {url}");
        }
        public override void validate(IWebDriver driver, DataRow validationData)
        {
            if (validationData[2] == null || validationData[2] is System.DBNull)
            {
                log.Warn("Skipping url validation, empty desired url");
                return;
            }
            string title = getSafeValue<string>(validationData[1], validationData[2]);

            this.validate(driver, (string)validationData[2], title);
        }
    }
}