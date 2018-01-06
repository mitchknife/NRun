using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NRun.Core.Jobs;
using Xunit;

namespace NRun.Core.UnitTests
{
	public class SequentialJobTests : TestsBase
	{
		[Fact]
		public void ParameterValidation_Success()
		{
			Invoking(() => new SequentialJob(null)).Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public async Task Execute_JobsRunSequentially()
		{
			using (var semaphore1 = new SemaphoreSlim(0))
			using (var semaphore2 = new SemaphoreSlim(0))
			{
				var job = new SequentialJob(new[]
				{
					new Job(async ct =>
					{
						await Task.Yield();
						semaphore1.Release();
					}),
					new Job(async ct =>
					{
						await Task.Yield();
						semaphore1.ShouldWait(1);
						semaphore2.Release();
					}),
				});

				await job.ExecuteAsync(CancellationToken.None);
				semaphore2.ShouldWait(1);
			}
		}
	}
}

