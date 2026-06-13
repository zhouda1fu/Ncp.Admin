using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Ncp.Admin.Web.Services.SystemLogs;

public sealed class SystemLogLoggerProvider(
    SystemLogChannel channel,
    IOptionsMonitor<SystemLogOptions> options,
    IHttpContextAccessor httpContextAccessor) : ILoggerProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, SystemLogLogger> _loggers = new(StringComparer.Ordinal);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new SystemLogLogger(name, channel, options, httpContextAccessor));

    public void Dispose()
    {
        _loggers.Clear();
    }

    private sealed class SystemLogLogger(
        string categoryName,
        SystemLogChannel channel,
        IOptionsMonitor<SystemLogOptions> options,
        IHttpContextAccessor httpContextAccessor) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            var opt = options.CurrentValue;
            return opt.Enabled
                   && logLevel != LogLevel.None
                   && logLevel >= opt.MinimumLevel
                   && !categoryName.StartsWith("Ncp.Admin.Web.Services.SystemLogs", StringComparison.Ordinal);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message) && exception == null)
                return;

            var httpContext = httpContextAccessor.HttpContext;
            var activity = Activity.Current;
            var entry = new SystemLogEntry(
                DateTimeOffset.UtcNow,
                logLevel.ToString(),
                categoryName,
                eventId.Id == 0 ? null : eventId.Id,
                message,
                exception?.ToString(),
                SerializeProperties(state),
                activity?.TraceId.ToString() ?? httpContext?.TraceIdentifier,
                httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
                httpContext?.Request.Path.Value,
                httpContext?.Connection.RemoteIpAddress?.ToString());

            channel.Write(entry);
        }

        private static string? SerializeProperties<TState>(TState state)
        {
            if (state is not IEnumerable<KeyValuePair<string, object?>> values)
                return null;

            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var pair in values)
            {
                if (pair.Key == "{OriginalFormat}")
                    continue;

                dict[pair.Key] = ShouldMask(pair.Key) ? "***" : ToSafeValue(pair.Value);
            }

            return dict.Count == 0 ? null : JsonSerializer.Serialize(dict, JsonOptions);
        }

        private static object? ToSafeValue(object? value) => value switch
        {
            null => null,
            string or bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => value,
            DateTime dateTime => dateTime,
            DateTimeOffset dateTimeOffset => dateTimeOffset,
            Guid guid => guid,
            _ => value.ToString()
        };

        private static bool ShouldMask(string key)
        {
            return key.Contains("password", StringComparison.OrdinalIgnoreCase)
                   || key.Contains("token", StringComparison.OrdinalIgnoreCase)
                   || key.Contains("secret", StringComparison.OrdinalIgnoreCase)
                   || key.Contains("authorization", StringComparison.OrdinalIgnoreCase)
                   || key.Contains("cookie", StringComparison.OrdinalIgnoreCase)
                   || key.Contains("密钥", StringComparison.OrdinalIgnoreCase)
                   || key.Contains("密码", StringComparison.OrdinalIgnoreCase);
        }
    }
}
