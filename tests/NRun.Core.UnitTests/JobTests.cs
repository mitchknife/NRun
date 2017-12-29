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
					startSemaphore.Wait(1000).Should().BeTrue();
					cancellation.Cancel();
					stopSemaphore.Wait(1000).Should().BeTrue();
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
			using (var semaphore = new SemaphoreSlim(1))
			{
				var scheduler = new TestScheduler();
				const int total = 3;
				int count = 0;

				var job = Job.Create(Observable.Interval(TimeSpan.FromTicks(1), scheduler)
					.TakeWhile(_ => count < total)
					.Select(x => Job.Create(async ct =>
					{
						await Task.Yield();
						count++;
						semaphore.Release();
					})));

				var task = job.ExecuteAsync(CancellationToken.None);

				foreach (int value in Enumerable.Range(0, total + 1))
				{
					semaphore.ShouldWait();
					count.Should().Be(value);
					scheduler.AdvanceBy(1);
				}

				await task;
				count.Should().Be(total);
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
				semaphore.ShouldNotWait();

				scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
				semaphore.ShouldNotWait();

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
			using (var semaphore = new SemaphoreSlim(0))
			{
				var scheduler = new TestScheduler();
				var job = Job.Create(Observable.Interval(TimeSpan.FromTicks(1), scheduler)
					.Select(x => Job.Create(async ct =>
					{
						await Task.Yield();
						try
						{
							throw new TestException();
						}
						finally
						{
							semaphore.Release();
						}
						
					})));

				var task = job.ExecuteAsync(CancellationToken.None);
				scheduler.AdvanceBy(1);
				semaphore.ShouldWait();
				Awaiting(() => task).Should().Throw<TestException>();
			}
		}
	}
}
