using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using NRun.Core;

namespace NRun.WindowsService
{
	public static class WindowsService
	{
		/// <summary>
		/// Runs the supplied job service as a Windows Service.
		/// </summary>
		/// <param name="jobService">The job service.</param>
		/// <param name="settings">The Windows Service settings.</param>
		/// <remarks>
		/// This method should be used in the entry point of your console application when run as a Windows Service.
		/// </remarks>
		public static void Run(WindowsServiceSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));
			if (settings.JobService == null)
				throw new ArgumentException("JobService is required.", nameof(settings));
			if (string.IsNullOrEmpty(settings.ServiceName))
				throw new ArgumentException("ServiceName is required.", nameof(settings));

			ServiceBase.Run(new OurServiceBase(settings.JobService) { ServiceName = settings.ServiceName });
		}

		public static void Install(WindowsServiceInstallSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));
			if (settings.ServiceName == null)
				throw new ArgumentException("ServiceName is required.", nameof(settings));

			using (var installer = CreateTransactedInstaller(settings))
				installer.Install(new Hashtable());
		}

		public static void Uninstall(WindowsServiceInstallSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));
			if (settings.ServiceName == null)
				throw new ArgumentException("ServiceName is required.", nameof(settings));

			using (var installer = CreateTransactedInstaller(settings))
				installer.Uninstall(null);
		}

		private static TransactedInstaller CreateTransactedInstaller(WindowsServiceInstallSettings settings)
		{
			var installer = new TransactedInstaller();
			installer.Installers.Add(new ServiceProcessInstaller { Account = ServiceAccount.LocalService });
			installer.Installers.Add(new ServiceInstaller
			{
				ServiceName = settings.ServiceName,
				DisplayName = settings.DisplayName ?? settings.ServiceName,
				Description = settings.Description ?? settings.DisplayName ?? settings.ServiceName,
				StartType = ServiceStartMode.Automatic
			});

			// TODO: should probably pass this in at some point.
			string path = "/assemblypath=" + Assembly.GetEntryAssembly().Location;
			installer.Context = new InstallContext(null, new[] { path });

			return installer;
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
