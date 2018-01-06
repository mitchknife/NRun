using System;
using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core.Jobs
{
	/// <summary>
	/// A simple job that wraps a function.
	/// </summary>
	public class Job : IJob
	{
		/// <summary>
		/// Gets a new job that executes the supplied fuction.
		/// </summary>
		public Job(Func<CancellationToken, Task> function)
		{
			m_function = function ?? throw new ArgumentNullException(nameof(function));
		}

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await m_function(cancellationToken).ConfigureAwait(false);
		}

		readonly Func<CancellationToken, Task> m_function;
	}
}
