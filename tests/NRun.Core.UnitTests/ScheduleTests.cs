using System;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace NRun.Core.UnitTests
{
	public class ScheduleTests : TestsBase
	{
		[Fact]
		public void Clone_Success()
		{
			var scheduler = new TestScheduler();
			var schedule = Schedule.Create(d => d, new ScheduleSettings { Scheduler = scheduler, EndTime = DateTime.UtcNow });

			var clonedSchedule = schedule.Clone();
			clonedSchedule.Should().NotBeNull();
			clonedSchedule.Scheduler.Should().Be(schedule.Scheduler);
			clonedSchedule.EndTime.Should().Be(schedule.EndTime);
		}

		[Theory]
		[InlineData("* * * * * *", 1, 1)]
		[InlineData("*/2 * * * * *", 1, 2)]
		[InlineData("0 */3 * * * *", 60, 3)]
		[InlineData("0 0 */4 * * *", 60 * 60, 4)]
		[InlineData("0 0 0 */5 * *", 60 * 60 * 24, 5)]
		public void SimpleCrontab_Success(string crontab, int secondsToAdvance, int iterationsUntilFirstScheduledTime)
		{
			long ticksToAdvance = TimeSpan.FromSeconds(secondsToAdvance).Ticks;
			var scheduler = new TestScheduler();
			var schedule = Schedule.CreateFromCrontab(crontab, new ScheduleSettings { Scheduler = scheduler });
			for (int i = 0; i < iterationsUntilFirstScheduledTime; i++)
			{
				scheduler.AdvanceTo(ticksToAdvance * i);
				schedule.GetNextScheduledTime().Should().Be(scheduler.Now.UtcDateTime.AddTicks(ticksToAdvance * (iterationsUntilFirstScheduledTime - i)));
			}
		}

		[Fact]
		public void EndTime_Success()
		{
			var scheduler = new TestScheduler();
			var settings = new ScheduleSettings
			{
				Scheduler = scheduler,
				EndTime = scheduler.Now.UtcDateTime.AddMinutes(10)
			};

			var startTime = scheduler.Now.UtcDateTime;
			var schedule = Schedule.CreateFromCrontab("* * * * *", settings);
			scheduler.AdvanceBy(TimeSpan.FromMinutes(12).Ticks);
			schedule.GetNextScheduledTime().Should().Be(startTime.AddMinutes(10));
		}
	}
}
