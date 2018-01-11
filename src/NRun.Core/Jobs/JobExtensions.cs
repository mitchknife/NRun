using System;
using System.Collections.Generic;

namespace NRun.Core.Jobs
{
	public static class JobExtensions
	{
		/// <summary>
		/// Converts a sequence of jobs into a parallel job.
		/// </summary>
		/// <param name="jobs">The jobs to execute in parallel.</param>
		/// <returns></returns>
		public static ParallelJob ToParallelJob(this IEnumerable<IJob> jobs)
		{
			return new ParallelJob(jobs);
		}
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
		/// Converts a job to a scheduled job.
		/// </summary>
		/// <param name="job">The job to schedule.</param>
		/// <param name="schedule">The schedule.</param>
		public static ScheduledJob ToScheduledJob(this IJob job, Schedule schedule)
		{
			return new ScheduledJob(job, schedule);
		}
	}
}
