using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core.Jobs
{
	public class ScheduledJob : IJob
	{
		public ScheduledJob(IJob job, ScheduledJobSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			m_job = job ?? throw new ArgumentNullException(nameof(job));
			m_getNextOccurrence = settings.GetNextOccurrence ?? throw new ArgumentException("'GetNextOccurrence' is required.", nameof(settings));
			m_scheduler = settings.Scheduler ?? Scheduler.Default;
		}

		public async Task ExecuteAsync(CancellationToken cancellatinToken)
		{
			await Observable.Generate(
				initialState: 0,
				condition: _ => true,
				iterate: _ => 0,
				resultSelector: _ => m_job,
				timeSelector: _ => new DateTimeOffset(m_getNextOccurrence(m_scheduler.Now.UtcDateTime)),
				scheduler: m_scheduler
			)
			.ToStreamingJob()
			.ExecuteAsync(cancellatinToken)
			.ConfigureAwait(false);
		}

		readonly IJob m_job;
		readonly Func<DateTime, DateTime> m_getNextOccurrence;
		readonly IScheduler m_scheduler;
	}
}
