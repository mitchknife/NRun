using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using NRun.Common.Testing;
using NRun.Core;
using Xunit;

namespace NRun.ConsoleApp.UnitTests
{
    public class ConsoleAppTestss : TestsBase
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
