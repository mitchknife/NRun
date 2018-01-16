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

			var jobService = new JobService(job, new JobServiceSettings { StopTimeout = settings.StopTimeout });
			jobService.UnhandledException += (sender, exception) =>
			{
				// TODO: log this exception as well?
				Environment.Exit(1);
			};

			var service = new OurServiceBase(jobService) { ServiceName = settings.ServiceName };
			ServiceBase.Run(service);
		}

		private sealed class OurServiceBase : ServiceBase
		{
			public OurServiceBase(JobService jobService)
			{
				m_jobService = jobService;
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
	}
}
