using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace NRun.Core.UnitTests
{

	public class JobTests : TestsBase
	{
		[Fact]
		public void ParameterValidation_Success()
		{
			Invoking(() => Job.Create(executeAsync: null)).Should().Throw<ArgumentNullException>();
		}


		[Fact]
		public async Task Execute_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var executeAsync = CreateExecuteAsync(ct => semaphore.Release());
				var job = Job.Create(executeAsync);
				var task = job.ExecuteAsync(CancellationToken.None);
				semaphore.ShouldWait(1);
				await task;
			}
		}

		[Fact]
		public void Execute_UnhandledException_Rethrows()
		{
			var executeAsync = CreateExecuteAsync(ct => throw new TestException());
			var job = Job.Create(executeAsync);
			Awaiting(() => job.ExecuteAsync(CancellationToken.None)).Should().Throw<TestException>();
		}

		[Fact]
		public async Task StreamingJob_Execute_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var scheduler = new TestScheduler();
				const int total = 3;

				var executeAsyncStream = Observable.Interval(TimeSpan.FromTicks(2), scheduler)
					.Take(total)
					.Select(x => CreateExecuteAsync(ct => semaphore.Release()));

				var job = Job.Create(executeAsyncStream);

				var task = job.ExecuteAsync(CancellationToken.None);

				foreach (int _ in Enumerable.Range(0, total))
				{
					scheduler.AdvanceBy(1);
					semaphore.ShouldWait(0);
					scheduler.AdvanceBy(1);
					semaphore.ShouldWait(1);
				}

				await task;
				semaphore.ShouldWait(0);
			}
		}

		[Fact]
		public void StreamingJob_UnhandledException_Rethrows()
		{
			var scheduler = new TestScheduler();
			var executeAsyncStream = Observable.Interval(TimeSpan.FromTicks(1), scheduler)
				.Select(x => CreateExecuteAsync(ct => throw new TestException()));

			var job = Job.Create(executeAsyncStream);
			var task = job.ExecuteAsync(CancellationToken.None);
			scheduler.AdvanceBy(1);
			Awaiting(() => task).Should().Throw<TestException>();
		}

		[Fact]
		public void ScheduledJob_Execute_Success()
		{
			using (var semaphore = new SemaphoreSlim(0, 3))
			using (var cancellation = new CancellationTokenSource())
			{
				var scheduler = new TestScheduler();
				var schedule = JobSchedule.CreateFromCrontab("*/5 * * * * *", new JobScheduleSettings { Scheduler = scheduler });

				var executeAsync = CreateExecuteAsync(ct => semaphore.Release());
				var job = Job.Create(executeAsync, schedule);
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

