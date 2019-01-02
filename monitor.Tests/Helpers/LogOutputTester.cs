using System;
using log4net;
using log4net.Config;
using log4net.Core;
using Xunit.Abstractions;

public abstract class LogOutputTester : IDisposable
{
    private readonly IAppenderAttachable _attachable;
    protected readonly ITestOutputHelper output;
    private TestOutputAppender _appender;

    protected LogOutputTester(ITestOutputHelper output)
    {
        var repo = ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository(this.GetType().Assembly));
        BasicConfigurator.Configure(repo);

        _attachable = repo.Root;
        _appender = new TestOutputAppender(output);
        _attachable?.AddAppender(_appender);
        this.output = output;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _attachable.RemoveAppender(_appender);
    }
}