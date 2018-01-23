using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NRun.Core;
using NRun.WindowsService;
using NRun.WindowsService.Install;

namespace WindowsServiceExample
{
	class Program
	{
		static readonly string ServiceName = "Windows Service Example";

		static void Main(string[] args)
		{
			if (TryInstallWindowsService(args))
				return;

			var schedule = JobSchedule.CreateFromCrontab("*/5 * * * * *");
			var job = Job.Create(ExecuteAsync, schedule);
			var jobService = new JobService(job);

			if (Environment.UserInteractive)
				ConsoleApp.Run(jobService);
			else
				WindowsService.Run(jobService, new WindowsServiceSettings { ServiceName = ServiceName });
		}

		static async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await Task.Delay(10).ConfigureAwait(false);

			string fileName = "c:\\temp\\WindowsServiceExample\\log.txt";
			var fileInfo = new FileInfo(fileName);
			if (!fileInfo.Directory.Exists)
				Directory.CreateDirectory(fileInfo.DirectoryName);

			string line = DateTime.Now.ToString();
			Console.WriteLine(line);
			File.AppendAllLines(fileName, new[] { line });
		}

		static bool TryInstallWindowsService(string[] arguments)
		{
			if (arguments == null || arguments.Length == 0)
				return false;

			if (arguments.Length == 1)
			{
				string argument = arguments[0];
				if (argument == "-install")
					WindowsServiceInstaller.Install(new WindowsServiceInstallerSettings { ServiceName = ServiceName });
				else if (argument == "-uninstall")
					WindowsServiceInstaller.Uninstall(new WindowsServiceInstallerSettings { ServiceName = ServiceName });
				else
					Console.WriteLine("Invalid argument: '{0}'", argument);
			}
			else
			{
				Console.WriteLine("Invalid number of arguments: {0}", arguments.Length);
			}

			return true;
		}
	}

	internal static class ConsoleApp
	{
		public static void Run(JobService jobService)
		{
			var semaphore = new SemaphoreSlim(0);

			void onServiceFaulted(object sender, Exception exception) => semaphore.Release();
			jobService.ServiceFaulted += onServiceFaulted;

			try
			{
				jobService.Start();
				Task.Run(() =>
				{
					Console.WriteLine("Presse any key to stop.");
					Console.ReadKey();
					semaphore.Release();
				});
				semaphore.Wait();
				jobService.Stop();
			}
			finally
			{
				jobService.ServiceFaulted -= onServiceFaulted;
				semaphore.Dispose();
			}
		}
	}
}
