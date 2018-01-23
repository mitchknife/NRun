using System;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// This class wraps a job with start/stop semantics, executing the job on the thread pool.
	/// </summary>
	/// <remarks>
	/// The main purpose of this class is to make more of the Windows Service pipeline testable using NetStandard.
	/// </remarks>
	public sealed class JobService
	{
		public JobService(IJob job, JobServiceSettings settings)
		{
			m_job = job ?? throw new ArgumentNullException(nameof(job));
			m_stopTimeout = settings?.StopTimeout ?? TimeSpan.FromSeconds(3);
		}

		public bool IsRunning => m_serviceTask != null;

		public event EventHandler<Exception> ServiceFaulted;

		public void Start()
		{
			lock (m_lock)
			{
				if (IsRunning)
					throw new InvalidOperationException("Service is already running.");

				m_cancellation = new CancellationTokenSource();
				m_serviceTask = Task.Run(async () =>
				{
					try
					{
						await m_job.ExecuteAsync(m_cancellation.Token).ConfigureAwait(false);
					}
					catch (Exception ex) when (IsCancellationException(ex))
					{
						throw;
					}
					catch (Exception exception)
					{
						ServiceFaulted?.Invoke(this, exception);
						throw;
					}
				});
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
					Task.WhenAny(Task.Delay(m_stopTimeout), m_serviceTask).GetAwaiter().GetResult().GetAwaiter().GetResult();
				}
				catch (Exception ex) when (IsCancellationException(ex))
				{
				}
				finally
				{
					m_cancellation.Dispose();
					m_cancellation = null;
					m_serviceTask = null;
				}
			}
		}

		private static bool IsCancellationException(Exception exception)
		{
			return exception is OperationCanceledException ||
				(exception is AggregateException agg && agg.InnerException is OperationCanceledException);
		}

		readonly object m_lock = new object();
		readonly IJob m_job;
		readonly TimeSpan m_stopTimeout;

		Task m_serviceTask;
		CancellationTokenSource m_cancellation;
	}
}
