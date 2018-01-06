using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NRun.Core.Jobs;
using Xunit;

namespace NRun.Core.UnitTests
{
	public class StreamingJobTests : TestsBase
	{
		[Fact]
		public void ParameterValidation_Success()
		{
			Invoking(() => new StreamingJob(null)).Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public async Task Execute_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var scheduler = new TestScheduler();
				const int total = 3;

				var job = new StreamingJob(Observable.Interval(TimeSpan.FromTicks(2), scheduler)
					.Take(total)
					.Select(x => new Job(async ct =>
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
		public void Execute_UnhandledException_Rethrows()
		{
			var scheduler = new TestScheduler();
			var job = new StreamingJob(Observable.Interval(TimeSpan.FromTicks(1), scheduler)
				.Select(x => new Job(async ct =>
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

