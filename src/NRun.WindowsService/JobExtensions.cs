using NRun.Core;
using System;
using System.Diagnostics;
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
				m_jobService.ServiceFaulted += OnJobServiceFaulted;
				m_jobService.Start();
			}

			protected override void OnStop()
			{
				m_jobService.Stop();
				m_jobService.ServiceFaulted -= OnJobServiceFaulted;
			}

			private void OnJobServiceFaulted(object sender, Exception exception)
			{
				try
				{
					OnStop();
				}
				finally
				{
					// NOTE: calling Environment.Exit() (instead of Stop()) ensures that the service is shutdown properly to enable automatic restarts.
					EventLog.WriteEntry($"Job service faulted; forcing process to exit. Exception={exception}", EventLogEntryType.Error);
					Environment.Exit(-1);
				}
			}

			readonly JobService m_jobService;
		}
	}
}
