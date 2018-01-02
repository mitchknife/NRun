using System;

namespace NRun.Core
{
	/// <summary>
	/// The settings for a <see cref="JobService"/>.
	/// </summary>
	public sealed class JobServiceSettings
	{
		/// <summary>
		/// The time to wait for job completion during <see cref="JobService.Stop"/>.
		/// </summary>
		public TimeSpan? StopTimeout { get; set; }
	}
}
