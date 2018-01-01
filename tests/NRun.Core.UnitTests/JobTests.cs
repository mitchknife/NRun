using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;
using System.Collections.Generic;

namespace NRun.Core.UnitTests
{
	public class JobTests : TestsBase
	{
		[Fact]
		public void ParameterValidation_Success()
		{
			Invoking(() => Job.Create(default(Func<CancellationToken, Task>))).Should().Throw<ArgumentNullException>();
			Invoking(() => Job.Create(default(IEnumerable<IJob>))).Should().Throw<ArgumentNullException>();
			Invoking(() => Job.Create(default(IObservable<IJob>))).Should().Throw<ArgumentNullException>();
			Invoking(() => Job.Create(startJob: null, stopJob: null)).Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public async Task Func_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var job = Job.Create(async ct =>
				{
					await Task.Yield();
					semaphore.Release();
				});

				var task = job.ExecuteAsync(CancellationToken.None);
				(await semaphore.WaitAsync(1000)).Should().BeTrue();
				await task;
			}
		}

		[Fact]
		public void Func_OnException_Rethrows()
		{
			var job = Job.Create(ct => throw new TestException());
			Awaiting(() => job.ExecuteAsync(CancellationToken.None)).Should().Throw<TestException>();
		}

		[Fact]
		public void StartStop_Success()
		{
			using (var startSemaphore = new SemaphoreSlim(0))
			using (var stopSemaphore = new SemaphoreSlim(0))
			{
				var job = Job.Create(
					startJob: () => startSemaphore.Release(),
					stopJob: () => stopSemaphore.Release());

				using (var cancellation = new CancellationTokenSource())
				{
					var task = job.ExecuteAsync(cancellation.Token);
					startSemaphore.ShouldWait(1);
					cancellation.Cancel();
					stopSemaphore.ShouldWait(1);
				}
			}
		}

		[Fact]
		public void StartStop_WithNonCancellableToken_Throws()
		{
			var job = Job.Create(startJob: () => { }, stopJob: () => { });
			Awaiting(() => job.ExecuteAsync(CancellationToken.None)).Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public async Task Observable_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var scheduler = new TestScheduler();
				const int total = 3;

				var job = Job.Create(Observable.Interval(TimeSpan.FromTicks(2), scheduler)
					.Take(total)
					.Select(x => Job.Create(async ct =>
					{
						await Task.Yield();
						semaphore.Release();
					})));

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
		public void WithSchedule_Success()
		{
			using (var semaphore = new SemaphoreSlim(0, 3))
			using (var cancellation = new CancellationTokenSource())
			{
				var scheduler = new TestScheduler();
				var job = Job.Create(async ct =>
				{
					await Task.Yield();
					semaphore.Release();
				})
				.WithSchedule("*/5 * * * * *", scheduler);

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

		[Fact]
		public void Observable_OnException_Rethrows()
		{
			var scheduler = new TestScheduler();
			var job = Job.Create(Observable.Interval(TimeSpan.FromTicks(1), scheduler)
				.Select(x => Job.Create(async ct =>
				{
					await Task.Yield();
					throw new TestException();
				})));

			var task = job.ExecuteAsync(CancellationToken.None);
			scheduler.AdvanceBy(1);
			Awaiting(() => task).Should().Throw<TestException>();
		}
	}
}
