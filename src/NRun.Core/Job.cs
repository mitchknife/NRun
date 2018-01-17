using System;
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
		/// Creates a job that executes an async function.
		/// </summary>
		/// <param name="executeAsync">The function.</param>
		public static IJob Create(Func<CancellationToken, Task> executeAsync)
		{
			if (executeAsync == null)
				throw new ArgumentNullException(nameof(executeAsync));

			return Create(Observable.Return(executeAsync));
		}

		/// <summary>
		/// Creates a job that executes an async function on a schedule.
		/// </summary>
		/// <param name="executeAsync">The function.</param>
		/// <param name="schedule">The schedule.</param>
		public static IJob Create(Func<CancellationToken, Task> executeAsync, JobSchedule schedule)
		{
			if (executeAsync == null)
				throw new ArgumentNullException(nameof(executeAsync));
			if (schedule == null)
				throw new ArgumentNullException(nameof(schedule));
			schedule = schedule.Clone();

			return Create(Observable.Generate(
				initialState: 0,
				condition: _ => true,
				iterate: _ => 0,
				resultSelector: _ => executeAsync,
				timeSelector: _ => new DateTimeOffset(schedule.GetNextScheduledTime()),
				scheduler: schedule.Scheduler));
		}

		/// <summary>
		/// Creates a job that executes an observable sequence of async functions.
		/// </summary>
		/// <param name="executeAsyncStream">The observable sequence of async functions.</param>
		public static IJob Create(IObservable<Func<CancellationToken, Task>> executeAsyncStream)
		{
			if (executeAsyncStream == null)
				throw new ArgumentNullException(nameof(executeAsyncStream));

			return new JobImpl(executeAsyncStream);
		}

		private sealed class JobImpl : IJob
		{
			public JobImpl(IObservable<Func<CancellationToken, Task>> executeAsyncStream)
			{
				m_executeAsyncStream = executeAsyncStream;
			}

			public async Task ExecuteAsync(CancellationToken cancellationToken)
			{
				await m_executeAsyncStream
					.Select(executeAsync => Observable.FromAsync(executeAsync))
					.Concat()
					.ToTask(cancellationToken);
			}

			readonly IObservable<Func<CancellationToken, Task>> m_executeAsyncStream;
		}
	}
}
