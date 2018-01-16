using NCrontab;
using System;
using System.Reactive.Concurrency;

namespace NRun.Core
{
	/// <summary>
	/// Represents a schedule.
	/// </summary>
	public sealed class JobSchedule
	{
		/// <summary>
		/// Creates a schedule from the supplied crontab expression.
		/// </summary>
		/// <param name="crontab">The crontab expression.</param>
		public static JobSchedule CreateFromCrontab(string crontab)
		{
			return CreateFromCrontab(crontab, null);
		}

		/// <summary>
		/// Creates a schedule from a crontab expression.
		/// </summary>
		/// <param name="crontab">The crontab expression.</param>
		/// <param name="settings">The schedule settings.</param>
		public static JobSchedule CreateFromCrontab(string crontab, JobScheduleSettings settings)
		{
			var parseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = crontab.Split(' ').Length == 6 };
			var crontabSchedule = CrontabSchedule.Parse(crontab, parseOptions);
			return Create(crontabSchedule.GetNextOccurrence, settings);
		}

		/// <summary>
		/// Creates a schedule.
		/// </summary>
		/// <param name="getNextScheduledTime">A method that gets the next scheduled time after the supplied time.</param>
		public static JobSchedule Create(Func<DateTime, DateTime> getNextScheduledTime)
		{
			return Create(getNextScheduledTime, null);
		}

		/// <summary>
		/// Creates a schedule.
		/// </summary>
		/// <param name="getNextScheduledTime">A method that gets the next scheduled time after the supplied time.</param>
		/// <param name="settings">The schedule settings.</param>
		public static JobSchedule Create(Func<DateTime, DateTime> getNextScheduledTime, JobScheduleSettings settings)
		{
			return new JobSchedule(getNextScheduledTime, settings);
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

		public JobSchedule Clone()
		{
			return new JobSchedule(m_getNextScheduledTime, new JobScheduleSettings { EndTime = EndTime, Scheduler = Scheduler });
		}

		private JobSchedule(Func<DateTime, DateTime> getNextScheduledTime, JobScheduleSettings settings)
		{
			m_getNextScheduledTime = getNextScheduledTime ?? throw new ArgumentNullException(nameof(getNextScheduledTime));
			Scheduler = settings?.Scheduler ?? System.Reactive.Concurrency.Scheduler.Default;
			EndTime = settings?.EndTime ?? DateTime.MaxValue.ToUniversalTime();
		}

		readonly Func<DateTime, DateTime> m_getNextScheduledTime;
	}
}
