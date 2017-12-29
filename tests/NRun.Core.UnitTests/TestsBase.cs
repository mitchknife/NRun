using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using System.Linq;

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
		public static void ShouldWait(this SemaphoreSlim semaphore)
		{
			semaphore.ShouldWait(1);
		}

		public static void ShouldWait(this SemaphoreSlim semaphore, int count)
		{
			foreach (var _ in Enumerable.Range(0, count))
				semaphore.Wait(1000).Should().BeTrue();
			semaphore.ShouldNotWait();
		}

		public static void ShouldNotWait(this SemaphoreSlim semaphore)
		{
			semaphore.Wait(0).Should().BeFalse();
		}
	}

	internal sealed class TestException : Exception
	{
	}
}
