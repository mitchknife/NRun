using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace NRun.WindowsService.Install
{
	public static class WindowsServiceInstaller
	{
		public static void Install(WindowsServiceInstallerSettings settings)
		{
			string path = "/assemblypath=" + Assembly.GetEntryAssembly().Location;
			using (TransactedInstaller transactedInstaller = new TransactedInstaller())
			{
				transactedInstaller.Installers.Add(new OurInstaller(settings));
				transactedInstaller.Context = new InstallContext(null, new[] { path });
				transactedInstaller.Install(new Hashtable());
			}
		}

		public static void Uninstall(WindowsServiceInstallerSettings settings)
		{
			string path = "/assemblypath=" + Assembly.GetEntryAssembly().Location;
			using (TransactedInstaller transactedInstaller = new TransactedInstaller())
			{
				transactedInstaller.Installers.Add(new OurInstaller(settings));
				transactedInstaller.Context = new InstallContext(null, new[] { path });
				transactedInstaller.Uninstall(null);
			}
		}

		private sealed class OurInstaller : Installer
		{
			public OurInstaller(WindowsServiceInstallerSettings settings)
			{
				ServiceAccount account = ServiceAccount.LocalService;
				Installers.AddRange(new Installer[]
				{
					new ServiceProcessInstaller { Account = account },
					new ServiceInstaller
					{
						ServiceName = settings.ServiceName,
						DisplayName = settings.DisplayName ?? settings.ServiceName,
						Description = settings.DisplayName ?? settings.ServiceName,
						StartType = ServiceStartMode.Automatic
					}
				});
			}
		}
	}
}
