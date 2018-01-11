using System;
using System.Threading;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NRun.Core.Jobs;
using Xunit;

namespace NRun.Core.UnitTests
{
	public class ScheduledJobTests : TestsBase
	{
		[Fact]
		public void Execute_Success()
		{
			using (var semaphore = new SemaphoreSlim(0, 3))
			using (var cancellation = new CancellationTokenSource())
			{
				var scheduler = new TestScheduler();
				var job = CreateTestJob(ct => semaphore.Release())
					.ToScheduledJob("*/5 * * * * *", scheduler);

				var task = job.ExecuteAsync(cancellation.Token);

				scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
				semaphore.ShouldWait(0);

				scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
				semaphore.ShouldWait(0);

				scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);
				semaphore.ShouldWait(1);

				scheduler.AdvanceBy(TimeSpan.FromSeconds(11).Ticks);
				semaphore.ShouldWait(2);

				cancellation.Cancel();
				Awaiting(() => task).Should().Throw<OperationCanceledException>();
			}
		}
	}
}

