using System;
using System.Threading;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NRun.Core.Jobs;

namespace NRun.Core.UnitTests
{
	public class JobServiceTests : TestsBase
	{
		[Fact]
		public void StartStop_Success()
		{
			using (var startSemaphore = new SemaphoreSlim(0))
			using (var stopSemaphore = new SemaphoreSlim(0))
			{
				var job = CreateStartStopJob(() => startSemaphore.Release(), () => stopSemaphore.Release());
				var service = new JobService(new[] { job });
				service.IsRunning.Should().BeFalse();
				service.Start();
				startSemaphore.ShouldWait(1);
				service.IsRunning.Should().BeTrue();
				service.Stop();
				stopSemaphore.ShouldWait(1);
				service.IsRunning.Should().BeFalse();
			}
		}

		[Fact]
		public void StartStop_MultipleJobs_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var job1 = new Job(async ct =>
				{
					await Task.Yield();
					semaphore.Release();
				});

				var job2 = new Job(async ct =>
				{
					await Task.Yield();
					semaphore.Release();
				});

				var service = new JobService(new[] { job1, job2 });
				service.Start();
				semaphore.ShouldWait(2);
				service.Stop();
			}
		}

		[Fact]
		public void ServiceFaulted_Handled_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var service = new TestJobService(
					jobs: new[] { new Job(ct => { throw new TestException(); }) },
					handleServiceFaulted: ex => { semaphore.Release(); return true; });

				service.Start();
				semaphore.ShouldWait(1);
				service.Stop();
			}
		}

		[Fact]
		public void ServiceFaulted_MultipleJobs_OtherJobsStop()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var service = new JobService(new[]
				{
					CreateStartStopJob(() => { }, () => semaphore.Release()),
					CreateStartStopJob(() => { }, () => semaphore.Release()),
					CreateStartStopJob(() => { }, () => semaphore.Release()),
					CreateStartStopJob(() => throw new TestException(), () => { }),
				});

				service.Start();
				semaphore.ShouldWait(3);
				service.Stop();
			}
		}

		private Job CreateStartStopJob(Action start, Action stop)
		{
			return new Job(async ct =>
		   {
			   start();
			   var taskCompletion = new TaskCompletionSource<bool>();
			   using (ct.Register(() => taskCompletion.SetResult(true)))
				   await taskCompletion.Task.ConfigureAwait(false);
			   stop();
		   });
		}

		private class TestJobService : JobService
		{
			public TestJobService(IReadOnlyList<IJob> jobs, Func<Exception, bool> handleServiceFaulted)
				: base(jobs)
			{
				m_handledServiceFaulted = handleServiceFaulted ?? base.HandleServiceFaulted;
			}

			protected override bool HandleServiceFaulted(Exception exception)
			{
				return m_handledServiceFaulted(exception);
			}

			Func<Exception, bool> m_handledServiceFaulted;
		}
	}
}
