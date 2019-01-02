using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Monitor;
using Monitor.Providers;


namespace monitor
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            createLogger("log4net.config");

            if (args.Length > 0 && args.Length != 1)
                throw new InvalidOperationException("Invalid command line syntax");
            Builder builder = args.Length == 1 ? new Builder(args[0]) : new Builder();
            Engine engine = builder.build();
            engine.run();
        }

        private static void createLogger(string file)
        {
            LogManager.GetRepository(Assembly.GetEntryAssembly()).LevelMap.Add(Log4netNotifyer.monitorLogLevel);
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(file));
        }
    }
}
