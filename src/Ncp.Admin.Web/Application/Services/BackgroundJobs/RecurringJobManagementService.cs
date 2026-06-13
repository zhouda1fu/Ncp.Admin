using Hangfire;
using Hangfire.Storage;

namespace Ncp.Admin.Web.Application.Services.BackgroundJobs;

public sealed class RecurringJobManagementService(IRecurringJobManager recurringJobManager)
{
    public IReadOnlyList<RecurringJobInfoDto> GetRecurringJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs();
        var known = KnownJobs.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

        return jobs
            .Select(job =>
            {
                known.TryGetValue(job.Id, out var definition);
                return new RecurringJobInfoDto(
                    job.Id,
                    definition?.DisplayName ?? job.Id,
                    definition?.Description ?? string.Empty,
                    job.Cron ?? string.Empty,
                    job.Queue ?? "default",
                    job.TimeZoneId ?? TimeZoneInfo.Local.Id,
                    job.LastExecution,
                    job.NextExecution,
                    job.LastJobId,
                    job.LastJobState,
                    job.Error,
                    definition is not null,
                    definition?.SettingsPath);
            })
            .OrderByDescending(x => x.IsKnown)
            .ThenBy(x => x.DisplayName)
            .ToList();
    }

    public bool Trigger(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        recurringJobManager.Trigger(id);
        return true;
    }

    public bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        recurringJobManager.RemoveIfExists(id);
        return true;
    }

    public bool UpsertKnownJob(string id, string cron) => false;

    public IReadOnlyList<KnownRecurringJobDto> GetKnownJobs() => [];

    private static readonly IReadOnlyList<KnownRecurringJobDefinition> KnownJobs = [];

    private sealed record KnownRecurringJobDefinition(
        string Id,
        string DisplayName,
        string Description,
        string? SettingsPath = null);
}

public sealed record RecurringJobInfoDto(
    string Id,
    string DisplayName,
    string Description,
    string Cron,
    string Queue,
    string TimeZoneId,
    DateTime? LastExecution,
    DateTime? NextExecution,
    string? LastJobId,
    string? LastJobState,
    string? Error,
    bool IsKnown,
    string? SettingsPath = null);

public sealed record KnownRecurringJobDto(
    string Id,
    string DisplayName,
    string Description,
    string ConfiguredCron,
    string? SettingsPath = null);
