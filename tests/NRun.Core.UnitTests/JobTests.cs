using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NRun.Core.Jobs;
using Xunit;

namespace NRun.Core.UnitTests
{

	public class JobTests : TestsBase
	{
		[Fact]
		public void ParameterValidation_Success()
		{
			Invoking(() => new Job(null)).Should().Throw<ArgumentNullException>();
		}


		[Fact]
		public async Task Execute_Success()
		{
			using (var semaphore = new SemaphoreSlim(0))
			{
				var job = CreateTestJob(ct => semaphore.Release());
				var task = job.ExecuteAsync(CancellationToken.None);
				semaphore.ShouldWait(1);
				await task;
			}
		}

		[Fact]
		public void Execute_UnhandledException_Rethrows()
		{
			var job = CreateTestJob(ct => throw new TestException());
			Awaiting(() => job.ExecuteAsync(CancellationToken.None)).Should().Throw<TestException>();
		}


	}
}

