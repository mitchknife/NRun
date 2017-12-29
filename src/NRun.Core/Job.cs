using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// A factory class for creating jobs.
	/// </summary>
	public static class Job
	{
		/// <summary>
		/// Creates a new job that executes the supplied fuction.
		/// </summary>
		public static IJob Create(Func<CancellationToken, Task> func)
		{
			if (func == null)
				throw new ArgumentNullException(nameof(func));

			return new SimpleJob(func);
		}

		/// <summary>
		/// Create a new job that executes <paramref name="startJob"/> when run and <paramref name="stopJob"/> when cancelled.
		/// </summary>
		/// <param name="startJob">Starts the job, returning in a timely fashion.</param>
		/// <param name="stopJob">Stops the job, returning when resources have been cleaned up.</param>
		/// <remarks>Use this method when creating a job from existing code that already uses start and stop semantics (e.g. a Windows Service).</remarks>
		public static IJob Create(Action startJob, Action stopJob)
		{
			if (startJob == null)
				throw new ArgumentNullException(nameof(startJob));
			if (stopJob == null)
				throw new ArgumentNullException(nameof(stopJob));

			return Create(async cancellationToken =>
			{
				if (!cancellationToken.CanBeCanceled)
					throw new InvalidOperationException("The cancellationToken must support cancellation.");

				startJob();
				var taskCompletion = new TaskCompletionSource<bool>();
				using (cancellationToken.Register(() => taskCompletion.SetResult(true)))
					await taskCompletion.Task.ConfigureAwait(false);
				stopJob();
			});
		}

		/// <summary>
		/// Creates a new job that executes the collection of jobs consecutively.
		/// </summary>
		public static IJob Create(IEnumerable<IJob> jobs)
		{
			if (jobs == null)
				throw new ArgumentNullException(nameof(jobs));

			return Create(jobs.ToObservable());
		}

		/// <summary>
		/// Creates a new job that subscribes to the observable and executes each job when observed."/>
		/// </summary>
		public static IJob Create(IObservable<IJob> jobStream)
		{
			if (jobStream == null)
				throw new ArgumentNullException(nameof(jobStream));

			return new StreamingJob(jobStream);
		}

		private sealed class SimpleJob : IJob
		{
			public SimpleJob(Func<CancellationToken, Task> func)
			{
				m_func = func;
			}

			public async Task ExecuteAsync(CancellationToken cancellationToken)
			{
				await m_func(cancellationToken).ConfigureAwait(false);
			}

			readonly Func<CancellationToken, Task> m_func;
		}

		private sealed class StreamingJob : IJob
		{
			public StreamingJob(IObservable<IJob> jobStream)
			{
				m_jobStream = jobStream;
			}

			public async Task ExecuteAsync(CancellationToken cancellationToken)
			{
				await m_jobStream
					.Select(job => Observable.FromAsync(job.ExecuteAsync))
					.Concat()
					.ToTask(cancellationToken);
			}

			readonly IObservable<IJob> m_jobStream;
		}
	}
}
