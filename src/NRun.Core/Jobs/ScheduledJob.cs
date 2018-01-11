using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core.Jobs
{
	public class ScheduledJob : IJob
	{
		public ScheduledJob(IJob job, Schedule schedule)
		{
			m_job = job ?? throw new ArgumentNullException(nameof(job));
			Schedule = schedule?.Clone() ?? throw new ArgumentNullException(nameof(schedule));
		}

		public Schedule Schedule { get; }

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await Observable.Generate(
				initialState: 0,
				condition: _ => true,
				iterate: _ => 0,
				resultSelector: _ => m_job,
				timeSelector: _ => new DateTimeOffset(Schedule.GetNextScheduledTime()),
				scheduler: Schedule.Scheduler
			)
			.ToStreamingJob()
			.ExecuteAsync(cancellationToken)
			.ConfigureAwait(false);
		}

		readonly IJob m_job;
	}
}
