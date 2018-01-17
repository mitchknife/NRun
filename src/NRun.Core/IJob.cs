using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// Encapulates work that needs to get done.
	/// </summary>
	public interface IJob
	{
		/// <summary>
		/// Executes the job.
		/// </summary>
		Task ExecuteAsync(CancellationToken cancellatinToken);
	}
}
