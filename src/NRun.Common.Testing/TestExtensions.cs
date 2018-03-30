using System.Threading;
using FluentAssertions;

namespace NRun.Common.Testing
{
	public static class TestExtensions
	{
		public static void ShouldWait(this SemaphoreSlim semaphore, int count)
		{
			int actualCount = 0;
			while (semaphore.Wait(actualCount == count ? 100 : 1000))
				actualCount++;

			actualCount.Should().Be(count, "semaphore.Release() should have been called {0} times", count);
		}
	}
}
