using System;
using System.Reactive.Concurrency;

namespace NRun.Core
{
	/// <summary>
	/// The schedule settings.
	/// </summary>
	public sealed class JobScheduleSettings
	{
		/// <summary>
		/// The scheduler for the schedule.
		/// </summary>
		public IScheduler Scheduler { get; set; }

		/// <summary>
		/// The upper bound time for the schedule.
		/// </summary>
		public DateTime? EndTime { get; set; }
	}
}
