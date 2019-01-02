using System.Collections.Generic;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Monitor.Interfaces;

namespace Monitor.Providers
{
    public class Log4netNotifyer : INotifyer
    {
        public static readonly Level monitorLogLevel = new Level(50000, "MONITOR");

        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Log4netNotifyer(Dictionary<string, object> args) { } //Dummy for easy reflection loading

        public void notifyMonitoredEvent(string message)
        {
            log.Monitor(message);
        }
    }

    static class MonitorEventExtention
    {
        public static void Monitor(this ILog log, string message)
        {
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                Log4netNotifyer.monitorLogLevel, message, null);
        }
    }
}