using System;
using System.Reactive.Linq;
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
				var job = new Job(async ct =>
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
		public void Execute_UnhandledException_Rethrows()
		{
			var job = new Job(ct => throw new TestException());
			Awaiting(() => job.ExecuteAsync(CancellationToken.None)).Should().Throw<TestException>();
		}


	}
}

