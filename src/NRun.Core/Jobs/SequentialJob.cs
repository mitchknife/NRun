using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core.Jobs
{
	public class SequentialJob : IJob
	{
		public SequentialJob(IEnumerable<IJob> jobs)
		{
			m_jobs = jobs ?? throw new ArgumentNullException(nameof(jobs));
		}

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			foreach (var job in m_jobs)
				await job.ExecuteAsync(cancellationToken).ConfigureAwait(false);
		}

		readonly IEnumerable<IJob> m_jobs;
	}
}
