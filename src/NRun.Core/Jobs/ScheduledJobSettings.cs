using System;
using System.Reactive.Concurrency;

namespace NRun.Core.Jobs
{
	public class ScheduledJobSettings
	{
		public Func<DateTime, DateTime> GetNextOccurrence { get; set; }
		public IScheduler Scheduler { get; set; }

	}
}
