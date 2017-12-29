using System.Threading;
using System.Threading.Tasks;

namespace NRun.Core
{
	/// <summary>
	/// Encapsulates a unit of work.
	/// </summary>
	public interface IJob
	{
		/// <summary>
		/// Executes the job.
		/// </summary>
		Task ExecuteAsync(CancellationToken cancellatinToken);
	}
}
