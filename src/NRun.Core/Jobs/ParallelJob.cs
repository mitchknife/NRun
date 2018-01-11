using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core.Jobs
{
	public class ParallelJob : IJob
	{
		public ParallelJob(IEnumerable<IJob> jobs)
		{
			m_jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
		}

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			using (var localCancellation = new CancellationTokenSource())
			using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, localCancellation.Token))
			{
				var tasks = m_jobs
					.Select(job => job.ExecuteAsync(linkedCancellation.Token))
					.ToList();

				while (tasks.Count != 0)
				{
					var completedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
					tasks.Remove(completedTask);
					try
					{
						await completedTask.ConfigureAwait(false);
					}
					catch
					{
						localCancellation.Cancel();
						throw;
					}
				}
			}
		}

		readonly IEnumerable<IJob> m_jobs;
	}
}
