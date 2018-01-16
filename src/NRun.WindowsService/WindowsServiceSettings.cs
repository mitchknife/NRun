using System;

namespace NRun.WindowsService
{
	/// <summary>
	/// Windows Service settings
	/// </summary>
	public sealed class WindowsServiceSettings
	{
		/// <summary>
		/// The service name.
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// The time to wait for the service to stop before forcibly closing.
		/// </summary>
		public TimeSpan? StopTimeout { get; set; }
 	}
}
