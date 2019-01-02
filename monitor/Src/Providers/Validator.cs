using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Monitor.Interfaces;
using OpenQA.Selenium;

public abstract class Validator : IValidator
{
    protected static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private INotifyer notifyer;
    public void registerNotifyer(INotifyer notifyer)
    {
        this.notifyer = notifyer;
    }

    public void registerNotifyers(IEnumerable<INotifyer> notifyers)
    {
        throw new NotImplementedException("Multiple notifyers aren't supported in this version");
    }

    protected void notify(string message)
    {
        if (notifyer != null)
            notifyer.notifyMonitoredEvent(message);
        else
            log.Error(message);
    }

    protected T getSafeValue<T>(object cell, object fallback)
    {
        return (T)((cell == null || cell is System.DBNull) ?
             fallback :
             cell);
    }

    public abstract void validate(IWebDriver driver, string url, string title = null);
    public abstract void validate(IWebDriver driver, DataRow siteData);
}