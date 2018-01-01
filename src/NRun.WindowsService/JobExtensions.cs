using NRun.Core;
using System;
using System.ServiceProcess;

namespace NRun.WindowsService
{

	public static class JobExtensions
    {
		/// <summary>
		/// Sets up and executes the supplied job as a Windows Service.
		/// </summary>
		/// <param name="job">The job to execute.</param>
		/// <param name="settings">The Windows Service settings.</param>
		/// <remarks>
		/// This method should be used in the entry point of your console application when run as a Windows Service.
		/// </remarks>
		public static void ExecuteAsWindowsService(this IJob job, WindowsServiceSettings settings)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));
			if (string.IsNullOrEmpty(settings.ServiceName))
				throw new ArgumentException("ServiceName is required.", nameof(settings));

			var service = new OurWindowsService(job) { ServiceName = settings.ServiceName };
			ServiceBase.Run(service);
		}

		private sealed class OurWindowsService : ServiceBase
		{
			public OurWindowsService(IJob job)
			{
				m_jobService = new OurJobService(job);
			}

			protected override void OnStart(string[] args)
			{
				m_jobService.Start();
			}

			protected override void OnStop()
			{
				m_jobService.Stop();
			}

			readonly JobService m_jobService;
		}

		private sealed class OurJobService : JobService
		{
			public OurJobService(IJob job)
				: base(new[] { job })
			{
			}

			protected override bool HandleServiceFaulted(Exception exception)
			{
				Environment.Exit(-1);
				return true;
			}
		}
	}
}
