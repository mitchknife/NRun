using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NRun.Core.Jobs;
using Xunit;

namespace NRun.Core.UnitTests
{
	public class ParallelJobTests : TestsBase
	{
		[Fact]
		public async Task Execute_JobsExecuteInParalllel()
		{
			using (var semaphore1 = new SemaphoreSlim(0))
			using (var semaphore2 = new SemaphoreSlim(0))
			using (var semaphore3 = new SemaphoreSlim(0))
			{
				var job = new ParallelJob(new[]
				{
					CreateTestJob(ct => semaphore1.Release()),
					CreateTestJob(ct => semaphore2.Release()),
					CreateTestJob(ct => semaphore3.Release()),
				});

				await job.ExecuteAsync(CancellationToken.None);
				semaphore3.ShouldWait(1);
				semaphore2.ShouldWait(1);
				semaphore1.ShouldWait(1);
			}
		}

		[Fact]
		public void Execute_UnhandledException_RethrowsAndCancellsOtherJobs()
		{
			using (var semaphore = new SemaphoreSlim(0))
			using (var disposable = new CompositeDisposable())
			{
				var job = new ParallelJob(new[]
				{
					CreateTestJob(ct => disposable.Add(ct.Register(() => semaphore.Release()))),
					CreateTestJob(ct => disposable.Add(ct.Register(() => semaphore.Release()))),
					CreateTestJob(ct => disposable.Add(ct.Register(() => semaphore.Release()))),
					CreateTestJob(ct => throw new TestException()),
				});

				var task = job.ExecuteAsync(CancellationToken.None);
				Awaiting(() => task).Should().Throw<TestException>();
				semaphore.ShouldWait(3);
			}
		}
	}
}
