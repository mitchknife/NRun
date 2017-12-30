using NCrontab;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace NRun.Core
{
	public static class JobExtensions
	{
		/// <summary>
		/// Creates a new job that executes on the supplied crontab schedule.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="crontab">The crontab schedule expression.</param>
		public static IJob WithSchedule(this IJob job, string crontab)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
			if (crontab == null)
				throw new ArgumentNullException(nameof(crontab));

			return job.WithSchedule(crontab, null);
		}

		/// <summary>
		/// Creates a new job that executes on the supplied crontab schedule.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="crontab">The crontab schedule expression.</param>
		/// <param name="scheduler">The scheduler.</param>
		public static IJob WithSchedule(this IJob job, string crontab, IScheduler scheduler)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
			if (crontab == null)
				throw new ArgumentNullException(nameof(crontab));

			var parseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = crontab.Split(' ').Length == 6 };
			var schedule = CrontabSchedule.Parse(crontab, parseOptions);
			return job.WithSchedule(schedule.GetNextOccurrence, scheduler);
		}

		/// <summary>
		/// Creates a new job that executes <paramref name="job"/> for each date <paramref name="getNextOccurrence"/> returns.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="getNextOccurrence">The method that gets the next time to execute the job.</param>
		/// <param name="scheduler">The scheduler.</param>
		public static IJob WithSchedule(this IJob job, Func<DateTime, DateTime> getNextOccurrence, IScheduler scheduler)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
			if (getNextOccurrence == null)
				throw new ArgumentNullException(nameof(getNextOccurrence));

			scheduler = scheduler ?? Scheduler.Default;
			var jobStream = Observable.Generate(
				initialState: 0,
				condition: _ => true,
				iterate: _ => 0,
				resultSelector: _ => job,
				timeSelector: _ => new DateTimeOffset(getNextOccurrence(scheduler.Now.UtcDateTime)),
				scheduler: scheduler);

			return Job.Create(jobStream);
		}
	}
}
