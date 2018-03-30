using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NRun.ConsoleApp;
using NRun.Core;
using NRun.WindowsService;

namespace WindowsServiceExample
{
	class Program
	{
		static readonly string ServiceName = "WindowsServiceExample";
		static readonly string DisplayName = "Windows Service Example";
		static readonly string Description = "An example Windows Service.";

		static void Main(string[] args)
		{
			if (TryInstallWindowsService(args))
				return;

			var schedule = JobSchedule.CreateFromCrontab("*/5 * * * * *");
			var job = Job.Create(ExecuteAsync, schedule);
			var jobService = new JobService(job);

			if (Environment.UserInteractive)
				ConsoleApp.Run(new ConsoleAppSettings { JobService = jobService });
			else
				WindowsService.Run(new WindowsServiceSettings { JobService = jobService, ServiceName = ServiceName });
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
				var settings = new WindowsServiceInstallSettings
				{
					ServiceName = ServiceName,
					DisplayName = DisplayName,
					Description = Description,
				};

				string argument = arguments[0];
				if (argument == "-install")
					WindowsService.Install(settings);
				else if (argument == "-uninstall")
					WindowsService.Uninstall(settings);
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
}
