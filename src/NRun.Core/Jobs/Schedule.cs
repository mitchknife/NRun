using NCrontab;
using System;
using System.Reactive.Concurrency;

namespace NRun.Core.Jobs
{
	/// <summary>
	/// Represents a schedule.
	/// </summary>
	public sealed class Schedule
	{
		/// <summary>
		/// Creates a schedule from the supplied crontab expression.
		/// </summary>
		/// <param name="crontab">The crontab expression.</param>
		public static Schedule CreateFromCrontab(string crontab)
		{
			return CreateFromCrontab(crontab, null);
		}

		/// <summary>
		/// Creates a schedule from a crontab expression.
		/// </summary>
		/// <param name="crontab">The crontab expression.</param>
		/// <param name="scheduler">The scheduler.</param>
		public static Schedule CreateFromCrontab(string crontab, IScheduler scheduler)
		{
			var parseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = crontab.Split(' ').Length == 6 };
			var crontabSchedule = CrontabSchedule.Parse(crontab, parseOptions);
			return Create(crontabSchedule.GetNextOccurrence, scheduler);
		}

		/// <summary>
		/// Creates a schedule.
		/// </summary>
		/// <param name="getNextScheduledTime">A method that returns the next scheduled time after the supplied time.</param>
		public static Schedule Create(Func<DateTime, DateTime> getNextScheduledTime)
		{
			return Create(getNextScheduledTime, null);
		}

		/// <summary>
		/// Creates a schedule.
		/// </summary>
		/// <param name="getNextScheduledTime">A method that returns the next scheduled time after the supplied time.</param>
		/// <param name="scheduler">The scheduler.</param>
		public static Schedule Create(Func<DateTime, DateTime> getNextScheduledTime, IScheduler scheduler)
		{
			return new Schedule(getNextScheduledTime, scheduler);
		}

		/// <summary>
		/// Gets the scheduler.
		/// </summary>
		public IScheduler Scheduler { get; }

		/// <summary>
		/// Gets the next scheduled time.
		/// </summary>
		public DateTime GetNextScheduledTime()
		{
			return m_getNextScheduledTime(Scheduler.Now.UtcDateTime);
		}

		public Schedule Clone()
		{
			return new Schedule(m_getNextScheduledTime, Scheduler);
		}

		private Schedule(Func<DateTime, DateTime> getNextScheduledTime, IScheduler scheduler)
		{
			m_getNextScheduledTime = getNextScheduledTime ?? throw new ArgumentNullException(nameof(getNextScheduledTime));
			Scheduler = scheduler ?? System.Reactive.Concurrency.Scheduler.Default;
		}

		readonly Func<DateTime, DateTime> m_getNextScheduledTime;
	}
}
