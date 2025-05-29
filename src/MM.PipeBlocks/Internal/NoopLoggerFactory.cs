using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MM.PipeBlocks.Internal;
internal class NoopLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

    public void Dispose() { }
}