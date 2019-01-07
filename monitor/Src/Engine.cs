using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using log4net;
using Monitor.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Monitor
{
    public class Engine
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISiteDataProvider urlProvider;
        private List<Thread> threads;
        private List<IValidator> validators = new List<IValidator>();
        private string browser = "chrome";
        private string gridAddress;
        private bool headless = false;
        public Engine(ISiteDataProvider urlProvider, int threadCount)
        {
            this.urlProvider = urlProvider;
            //TODO threads cap and validation
            threads = new List<Thread>(threadCount);
            for (int i = 0; i < threadCount; ++i)
                threads.Add(
                    new Thread(new ThreadStart(worker))
                    { Name = $"Monitor thread #{i}" });
        }

        public Engine(ISiteDataProvider urlProvider, Dictionary<string, object> args)
        : this(urlProvider, int.Parse((string)args["threads"]))
        {
            browser = (string)args.GetValueOrDefault("browser", browser);
            headless = bool.Parse((string)args.GetValueOrDefault("headless", "false"));
            gridAddress = (string)args.GetValueOrDefault("gridAddress", null);
        }

        internal void registerNotifyer(INotifyer notifyer)
        {
            validators.ForEach(v => v.registerNotifyer(notifyer));
        }

        internal void registerNotifyers(IEnumerable<INotifyer> notifyers)
        {
            throw new NotImplementedException("Multipe notifyers aren't supported in this version");
        }

        public void registerValidators(List<IValidator> validators)
        {
            this.validators.AddRange(validators);
        }

        public void registerValidator(IValidator validator)
        {
            validators.Add(validator);
        }

        public void run()
        {
            log.Debug("Starting Engine");
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
            log.Debug("Closing Engine");
        }

        private void worker()
        {
            IWebDriver driver = createDriver();
            try
            {
                DataRow siteData = urlProvider.nextRow();
                while (siteData != null)
                {
                    object url2be = siteData[0];
                    if (url2be is System.DBNull)
                    {
                        log.Warn($"Error reading url {url2be}, skipping...");
                        siteData = urlProvider.nextRow();
                        continue;
                    }
                    string url = (string)url2be;
                    log.Info($"Checking url {url}");
                    TimeSpan ttl = safeNavigate(driver, url);
                    addMetrics("ttl", siteData, ttl);
                    validators.ForEach((v) => safeValidate(v, driver, siteData));
                    siteData = urlProvider.nextRow();
                    log.Debug("Check done");
                }
            }
            finally
            {
                driver.Close();
            }
        }

        private void addMetrics(string colName, DataRow siteData, object metric)
        {
            if (siteData.Table.Columns.Contains(colName))
                siteData[colName] = metric;
        }

        private IWebDriver createDriver()
        {
            ChromeOptions options = new ChromeOptions();
            if (headless) options.AddArguments("headless");
            if (string.IsNullOrEmpty(gridAddress))
                //local
                return new ChromeDriver(options);
            else
                //remote
                return new RemoteWebDriver(new Uri(gridAddress), options);
        }

        private static TimeSpan safeNavigate(IWebDriver driver, string url)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                driver.Navigate().GoToUrl(url);
                stopwatch.Stop();
                return stopwatch.Elapsed;
            }
            catch (Exception e)
            {
                log.Error("Error while navigating", e);
            }

            return TimeSpan.Zero;
        }

        private static void safeValidate(IValidator v, IWebDriver driver, DataRow urlData)
        {
            try
            {
                v.validate(driver, urlData);
            }
            catch (Exception e)
            {
                log.Error("Error while validating", e);
            }
        }
    }
}
