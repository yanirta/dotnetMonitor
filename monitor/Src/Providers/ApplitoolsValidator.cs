using Applitools;
using Applitools.Selenium;
using Monitor.Extentions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;

namespace Monitor.Providers
{
    public class ApplitoolsValidator : Validator
    {
        private BatchInfo batch;
        private string serverUrl;
        private readonly string apiKey;
        private readonly string appName;
        private readonly Size viewport = Size.Empty;

        private ThreadLocal<Eyes> LocalEyes = new ThreadLocal<Eyes>(() => new Eyes()
        {
            ForceFullPageScreenshot = true,
            StitchMode = StitchModes.CSS
        });

        public ApplitoolsValidator(Dictionary<string, object> eyesParams)
        {
            this.apiKey = (string)eyesParams.getSafeValue("apiKey");
            this.appName = (string)eyesParams.getSafeValue("appName");
            this.batch = new BatchInfo(
                (string)eyesParams.getSafeValue("batchname", $"Monitor cycle - {DateTime.Now.ToString()}"));
            this.serverUrl = (string)eyesParams.getSafeValue("serverUrl");
            this.viewport = readSafeSize(eyesParams["viewportsize"], viewport);
        }

        public ApplitoolsValidator(string apiKey, string appName, string batchName, Size viewport)
        {
            this.apiKey = apiKey;
            this.appName = appName;
            this.viewport = viewport;
            this.batch = new BatchInfo(batchName);
        }

        private static Size readSafeSize(object vstobe, Size fallback)
        {
            try
            {
                if (vstobe == null || !(vstobe is Dictionary<string, object>))
                    return Size.Empty;
                Dictionary<string, object> sizeDict = (Dictionary<string, object>)vstobe;
                int width = int.Parse((string)sizeDict.getSafeValue("width", fallback.Width.ToString()));
                int height = int.Parse((string)sizeDict.getSafeValue("height", fallback.Height.ToString()));
                return new Size(width, height);
            }
            catch (System.Exception e)
            {
                log.Error("Couldn't read config value, will be using fallback", e);
                return fallback;
            }
        }

        public override void validate(IWebDriver driver, DataRow validationData)
        {

            string title = getSafeValue<string>(validationData[0], validationData[1]);

            this.validate(driver, (string)validationData[0], title);
        }

        public override void validate(IWebDriver driver, string url, string title = null)
        {
            log.Info($"Validating visually {title}");
            Eyes eyes = LocalEyes.Value;
            eyes.ServerUrl = serverUrl ?? eyes.ServerUrl;
            eyes.Batch = batch;
            string app = string.IsNullOrEmpty(appName) ? new Uri(url).DnsSafeHost : appName;
            try
            {
                eyes.Open(driver, app, title, viewport);
                eyes.CheckWindow(url);
                TestResults res = eyes.Close(false);
                if (!res.IsPassed && !res.IsNew)
                    notify($"Visual validation failed for {title} \n see full results at {res.Url}");
            }
            finally
            {
                eyes.AbortIfNotClosed();
            }

        }
    }
}
