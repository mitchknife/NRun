using System;
using System.Threading;

namespace NRun.ConsoleApp
{
	public static class ConsoleApp
	{
		/// <summary>
		/// Runs the supplied job service as a Console App.
		/// </summary>
		/// <param name="settings">The Console App settings.</param>
		/// <remarks>
		/// This method can be used when running as a console app instead of handling the job service start and stop methods yourself.
		/// Supports stopping via CTRL+C in both Windows and Linux.
		/// </remarks>
		public static void Run(ConsoleAppSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));
			if (settings.JobService == null)
				throw new ArgumentException("JobService is required.", nameof(settings));

			var jobService = settings.JobService;
			using (var signal = new AutoResetEvent(false))
			{
				Console.CancelKeyPress += (sender, args) => signal.Set();
				jobService.ServiceFaulted += (sender, exception) => signal.Set();
				jobService.Start();
				signal.WaitOne();
				jobService.Stop();
			}
		}
	}
}
