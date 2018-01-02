using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// This class wraps a collection of jobs with start/stop semantics, executing the jobs concurrently on the threadpool.
	/// The main purpose of this class is to make more of the Windows Service pipeline testable and should not need to used directly from your code.
	/// </summary>
	public class JobService
    {
		public JobService(IReadOnlyList<IJob> jobs)
			: this(jobs, null)
		{
		}

		public JobService(IReadOnlyList<IJob> jobs, JobServiceSettings settings)
		{
			m_jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
			m_stopTimeout = settings?.StopTimeout ?? TimeSpan.FromSeconds(3);
		}

		public bool IsRunning => m_cancellation != null;

		public void Start()
		{
			lock (m_lock)
			{
				if (IsRunning)
					throw new InvalidOperationException("Service is already running.");

				m_cancellation = new CancellationTokenSource();
				m_jobTasks = m_jobs
					.Select(job => Task.Run(() => job.ExecuteAsync(m_cancellation.Token)))
					.ToList();

				foreach (var jobTask in m_jobTasks)
					jobTask.ContinueWith(_ => Stop(), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach);
			}
		}

		public void Stop()
		{
			lock (m_lock)
			{
				if (!IsRunning)
					return;

				m_cancellation.Cancel();

				try
				{
					Task.WhenAny(Task.Delay(m_stopTimeout), Task.WhenAll(m_jobTasks)).GetAwaiter().GetResult().GetAwaiter().GetResult();
				}
				catch (Exception ex) when (
					ex is OperationCanceledException ||
					(ex is AggregateException agg && agg.InnerException is OperationCanceledException))
				{
				}
				catch (Exception ex)
				{
					if (!HandleServiceFaulted(ex))
						throw;
				}
				finally
				{
					m_cancellation.Dispose();
					m_cancellation = null;
					m_jobTasks = null;
				}
			}
		}

		protected virtual bool HandleServiceFaulted(Exception exception)
		{
			return false;
		}

		readonly object m_lock = new object();
		readonly IReadOnlyList<IJob> m_jobs;
		readonly TimeSpan m_stopTimeout;

		IReadOnlyList<Task> m_jobTasks;
		CancellationTokenSource m_cancellation;
	}
}
