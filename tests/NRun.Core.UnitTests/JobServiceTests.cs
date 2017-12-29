using System;
using System.Threading;
using FluentAssertions;
using Xunit;

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
				var job = Job.Create(
					startJob: () => startSemaphore.Release(),
					stopJob: () => stopSemaphore.Release());

				var service = new JobService(job);
				service.IsRunning.Should().BeFalse();
				service.Start();
				startSemaphore.ShouldWait();
				service.IsRunning.Should().BeTrue();
				service.Stop();
				stopSemaphore.ShouldWait();
				service.IsRunning.Should().BeFalse();
			}
		}

		[Fact]
		public void OnException_ServiceFaultedIsCalled()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var service = new TestJobService(
					job: Job.Create(ct => { throw new TestException(); }),
					handleServiceFaulted: ex => { semaphore.Release(); return true; });

				service.Start();
				semaphore.ShouldWait();
				service.Stop();
			}
		}

		private class TestJobService : JobService
		{
			public TestJobService(IJob job, Func<Exception, bool> handleServiceFaulted)
				: base(job)
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
