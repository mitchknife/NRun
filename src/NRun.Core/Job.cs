using System;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// A simple job that wraps a function.
	/// </summary>
	public class Job : IJob
	{
		/// <summary>
		/// Gets a new job that executes the supplied fuction.
		/// </summary>
		public Job(Func<CancellationToken, Task> executeAsync)
		{
			m_executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
		}

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await m_executeAsync(cancellationToken).ConfigureAwait(false);
		}

		readonly Func<CancellationToken, Task> m_executeAsync;
	}
}
