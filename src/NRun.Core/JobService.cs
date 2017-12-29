using System;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// This class wraps a job with start/stop semantics, executing the job on the threadpool.
	/// The main purpose of this class is to make more of the Windows Service pipeline testable and should not need to used directly from your code.
	/// </summary>
    public class JobService
    {
		public JobService(IJob job)
		{
			m_job = job ?? throw new ArgumentNullException(nameof(job));
		}

		public bool IsRunning => m_cancellation != null;

		public void Start()
		{
			lock (m_lock)
			{
				if (IsRunning)
					throw new InvalidOperationException("Service is already running.");

				m_cancellation = new CancellationTokenSource();
				m_jobTask = Task.Run(() => m_job.ExecuteAsync(m_cancellation.Token));
				m_jobTask.ContinueWith(_ => Stop(), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach);
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
					m_jobTask.GetAwaiter().GetResult();
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
					m_jobTask = null;
				}
			}
		}

		protected virtual bool HandleServiceFaulted(Exception exception)
		{
			return false;
		}

		readonly object m_lock = new object();
		readonly IJob m_job;

		Task m_jobTask;
		CancellationTokenSource m_cancellation;
	}
}
