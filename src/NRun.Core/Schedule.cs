using NCrontab;
using System;
using System.Reactive.Concurrency;

namespace NRun.Core
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
		/// <param name="settings">The schedule settings.</param>
		public static Schedule CreateFromCrontab(string crontab, ScheduleSettings settings)
		{
			var parseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = crontab.Split(' ').Length == 6 };
			var crontabSchedule = CrontabSchedule.Parse(crontab, parseOptions);
			return Create(crontabSchedule.GetNextOccurrence, settings);
		}

		/// <summary>
		/// Creates a schedule.
		/// </summary>
		/// <param name="getNextScheduledTime">A method that gets the next scheduled time after the supplied time.</param>
		public static Schedule Create(Func<DateTime, DateTime> getNextScheduledTime)
		{
			return Create(getNextScheduledTime, null);
		}

		/// <summary>
		/// Creates a schedule.
		/// </summary>
		/// <param name="getNextScheduledTime">A method that gets the next scheduled time after the supplied time.</param>
		/// <param name="settings">The schedule settings.</param>
		public static Schedule Create(Func<DateTime, DateTime> getNextScheduledTime, ScheduleSettings settings)
		{
			return new Schedule(getNextScheduledTime, settings);
		}

		/// <summary>
		/// Gets the scheduler for the schedule.
		/// </summary>
		public IScheduler Scheduler { get; }

		/// <summary>
		/// Gets the upper bound time for the schedule.
		/// </summary>
		public DateTime EndTime { get; }

		/// <summary>
		/// Gets the next scheduled time.
		/// </summary>
		public DateTime GetNextScheduledTime()
		{
			var nextTime = m_getNextScheduledTime(Scheduler.Now.UtcDateTime);
			return nextTime < EndTime ? nextTime : EndTime;
		}

		public Schedule Clone()
		{
			return new Schedule(m_getNextScheduledTime, new ScheduleSettings { EndTime = EndTime, Scheduler = Scheduler });
		}

		private Schedule(Func<DateTime, DateTime> getNextScheduledTime, ScheduleSettings settings)
		{
			m_getNextScheduledTime = getNextScheduledTime ?? throw new ArgumentNullException(nameof(getNextScheduledTime));
			Scheduler = settings?.Scheduler ?? System.Reactive.Concurrency.Scheduler.Default;
			EndTime = settings?.EndTime ?? DateTime.MaxValue.ToUniversalTime();
		}

		readonly Func<DateTime, DateTime> m_getNextScheduledTime;
	}
}
