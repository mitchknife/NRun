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

		protected Job CreateTestJob(Action<CancellationToken> execute)
		{
			return new Job(async ct =>
			{
				await Task.Delay(1).ConfigureAwait(false);
				execute(ct);
			});
		}

		protected Job CreateTestJob(Action start, Action stop)
		{
			return new Job(async ct =>
			{
				if (!ct.CanBeCanceled)
					throw new InvalidOperationException("cancellationToken must be able to be cancelled.");
				start();
				var taskCompletion = new TaskCompletionSource<bool>();
				using (ct.Register(() => taskCompletion.SetResult(true)))
					await taskCompletion.Task.ConfigureAwait(false);
				stop();
			});
		}
	}

	internal static class TestExtensions
	{
		public static void ShouldWait(this SemaphoreSlim semaphore, int count)
		{
			int actualCount = 0;
			while (semaphore.Wait(actualCount == count ? 100 : 1000))
				actualCount++;

			actualCount.Should().Be(count, "semaphore.Release() should have been called {0} times", count);
		}
	}

	internal sealed class TestException : Exception
	{
	}
}
