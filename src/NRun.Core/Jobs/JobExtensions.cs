using System;

namespace NRun.Core.Jobs
{
	public static class JobExtensions
	{
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
