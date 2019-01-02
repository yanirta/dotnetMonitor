using log4net.Appender;
using log4net.Config;
using Monitor.Interfaces;
using Monitor.Providers;
using Xunit;
using Xunit.Abstractions;

public class Log4netNotifyerTests : LogOutputTester
{
    public Log4netNotifyerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    void test()
    {
        this.output.WriteLine("This is output direct message");
        INotifyer n = new Log4netNotifyer(null);
        n.notifyMonitoredEvent("This is monitor notifyer message");
    }
}