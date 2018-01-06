using NCrontab;
using NRun.Core.Jobs;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace NRun.Core.Jobs
{
	public static class JobExtensions
	{
		/// <summary>
		/// Converts a sequence of jobs to a sequential job.
		/// </summary>
		/// <param name="jobs">The jobs to execute sequentially.</param>
		public static SequentialJob ToSequentialJob(this IEnumerable<IJob> jobs)
		{
			return new SequentialJob(jobs);
		}

		/// <summary>
		/// Converts an observable sequence of jobs to a streaming job.
		/// </summary>
		/// <param name="jobStream">The observable sequence of jobs to stream.</param>
		public static StreamingJob ToStreamingJob(this IObservable<IJob> jobStream)
		{
			return new StreamingJob(jobStream);
		}

		/// <summary>
		/// Converts a job to a scheduled job using a crontab expression.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="crontab">The crontab expression.</param>
		public static ScheduledJob ToScheduledJob(this IJob job, string crontab)
		{
			return job.ToScheduledJob(crontab, null);
		}

		/// <summary>
		/// Converts a job to a scheduled job using a crontab expression and scheduler.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="crontab">The crontab expression.</param>
		/// <param name="scheduler">The scheduler.</param>
		public static ScheduledJob ToScheduledJob(this IJob job, string crontab, IScheduler scheduler)
		{
			if (crontab == null)
				throw new ArgumentNullException(nameof(crontab));

			var parseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = crontab.Split(' ').Length == 6 };
			var schedule = CrontabSchedule.Parse(crontab, parseOptions);
			return job.ToScheduledJob(new ScheduledJobSettings { GetNextOccurrence = schedule.GetNextOccurrence, Scheduler = scheduler });
		}

		/// <summary>
		/// Converts a job to a scheduled job.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="settings">The scheduled job settings.</param>
		public static ScheduledJob ToScheduledJob(this IJob job, ScheduledJobSettings settings)
		{
			return new ScheduledJob(job, settings);
		}
	}
}
