using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core.Jobs
{
	public class StreamingJob : IJob
	{
		public StreamingJob(IObservable<IJob> jobStream)
		{
			m_jobStream = jobStream ?? throw new ArgumentNullException(nameof(jobStream));
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
