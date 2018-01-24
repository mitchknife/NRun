using NRun.Core;

namespace NRun.WindowsService
{
	/// <summary>
	/// Windows Service settings
	/// </summary>
	public sealed class WindowsServiceSettings
	{
		/// <summary>
		/// The job service.
		/// </summary>
		public JobService JobService { get; set; }

		/// <summary>
		/// The service name.
		/// </summary>
		public string ServiceName { get; set; }
 	}
}
