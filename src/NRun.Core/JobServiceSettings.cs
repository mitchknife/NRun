using System;

namespace NRun.Core
{
	/// <summary>
	/// The job service settings.
	/// </summary>
	public sealed class JobServiceSettings
	{
		/// <summary>
		/// The time to wait for job completion when stop is called.
		/// </summary>
		public TimeSpan? StopTimeout { get; set; }
	}
}
