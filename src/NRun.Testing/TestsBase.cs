using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace NRun.Testing
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

		protected Func<CancellationToken, Task> CreateExecuteAsync(Action<CancellationToken> execute)
		{
			return async ct =>
			{
				await Task.Delay(1).ConfigureAwait(false);
				execute(ct);
			};
		}

		protected Func<CancellationToken, Task> CreateExecuteAsync(Action start, Action stop)
		{
			return async ct =>
			{
				if (!ct.CanBeCanceled)
					throw new InvalidOperationException("cancellationToken must be able to be cancelled.");
				start();
				var taskCompletion = new TaskCompletionSource<bool>();
				using (ct.Register(() => taskCompletion.SetResult(true)))
					await taskCompletion.Task.ConfigureAwait(false);
				stop();
			};
		}
	}
}
