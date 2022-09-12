using FluentAssertions;
using NRun.Core;
using NRun.Testing;
using Xunit;

namespace NRun.ConsoleApp.UnitTests
{
    public class ConsoleAppTests : TestsBase
    {
		[Fact]
		public void Run_UnhandledException_Rethrows()
		{
			var executeAsync = CreateExecuteAsync(ct => throw new TestException());
			var job = Job.Create(executeAsync);
			var jobService = new JobService(job);

			Invoking(() => ConsoleApp.Run(new ConsoleAppSettings { JobService = jobService }))
				.Should().Throw<TestException>();
		}
	}
}
