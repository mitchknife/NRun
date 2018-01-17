using FluentAssertions;
using System;
using System.Threading;
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
				var executeAsync = CreateExecuteAsync(() => startSemaphore.Release(), () => stopSemaphore.Release());
				var job = Job.Create(executeAsync);
				var service = new JobService(job, null);
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
		public void UnhandledException_IsInvoked()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var service = new JobService(Job.Create(ct => { throw new TestException(); }), null);
				service.UnhandledException += (_, ex) => semaphore.Release();

				service.Start();
				semaphore.ShouldWait(1);
				service.Stop();
			}
		}

		[Fact]
		public void StopTimeout_FastJob_ShouldFinish()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var executeAsync = CreateExecuteAsync(
					start: () => { },
					stop: () =>
					{
						Thread.Sleep(100);
						semaphore.Release();
					});

				var job = Job.Create(executeAsync);
				var service = new JobService(job, new JobServiceSettings
				{
					StopTimeout = TimeSpan.FromMilliseconds(1000),
				});

				service.Start();
				service.Stop();
				semaphore.ShouldWait(1);
			}
		}

		[Fact]
		public void StopTimeout_SlowJob_ShouldNotFinish()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var executeAsync = CreateExecuteAsync(
					start: () => { },
					stop: () =>
					{
						Thread.Sleep(1000);
						semaphore.Release();
					});

				var job = Job.Create(executeAsync);
				var service = new JobService(job, new JobServiceSettings
				{
					StopTimeout = TimeSpan.FromMilliseconds(100),
				});

				service.Start();
				service.Stop();
				semaphore.ShouldWait(0);
			}
		}
	}
}
