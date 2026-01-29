using Finitech.Modules.Scheduler.Contracts.DTOs;

namespace Finitech.Modules.Scheduler.Contracts;

public interface ISchedulerService
{
    Task<IReadOnlyList<ScheduledJobDto>> GetScheduledJobsAsync(CancellationToken cancellationToken = default);
    Task<ScheduledJobDto?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task TriggerJobAsync(TriggerJobRequest request, CancellationToken cancellationToken = default);
    Task PauseJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task ResumeJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobExecutionHistoryDto>> GetJobHistoryAsync(string jobId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
}
