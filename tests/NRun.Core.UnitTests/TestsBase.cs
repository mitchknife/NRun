using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;

namespace NRun.Core.UnitTests
{
	public abstract class TestsBase
	{
		protected Action Invoking(Action action)
		{
			return this.Invoking(_ => action());
		}

		protected Func<Task> Awaiting(Func<Task> funcAsync)
		{
			return this.Awaiting(async _ => await funcAsync());
		}
	}

	internal static class TestExtensions
	{
		public static void ShouldWait(this SemaphoreSlim semaphore, int count)
		{
			int actualCount = 0;
			while (semaphore.Wait(100))
				actualCount++;

			actualCount.Should().Be(count, "semaphore.Release() should have been called {0} times", count);
		}
	}

	internal sealed class TestException : Exception
	{
	}
}
