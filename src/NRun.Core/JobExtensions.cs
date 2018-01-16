using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace NRun.Core
{
	public static class JobExtensions
	{
		/// <summary>
		/// Converts an observable sequence of jobs to a streaming job.
		/// </summary>
		/// <param name="jobStream">The observable sequence of jobs to stream.</param>
		public static IJob ToStreamingJob(this IObservable<IJob> jobStream)
		{
			if (jobStream == null)
				throw new ArgumentNullException(nameof(jobStream));

			return new Job(async cancellationToken =>
			{
				await jobStream
					.Select(job => Observable.FromAsync(job.ExecuteAsync))
					.Concat()
					.ToTask(cancellationToken);
			});
		}

		/// <summary>
		/// Converts a job to a scheduled job.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="schedule">The schedule.</param>
		public static IJob ToScheduledJob(this IJob job, JobSchedule schedule)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
			if (schedule == null)
				throw new ArgumentNullException(nameof(schedule));

			schedule = schedule.Clone();
			return Observable.Generate(
				initialState: 0,
				condition: _ => true,
				iterate: _ => 0,
				resultSelector: _ => job,
				timeSelector: _ => new DateTimeOffset(schedule.GetNextScheduledTime()),
				scheduler: schedule.Scheduler
			).ToStreamingJob();
		}
	}
}
