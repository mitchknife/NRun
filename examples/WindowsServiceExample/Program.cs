using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            if (Environment.UserInteractive)
            {
                InstallWindowsService(args);
                return;
            }

            var schedule = JobSchedule.CreateFromCrontab("*/5 * * * * *");
            var job = Job.Create(ExecuteAsync, schedule);
            var jobService = new JobService(job);

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

        static void InstallWindowsService(string[] arguments)
        {
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
            }

            Console.WriteLine("WindowsServiceExample.exe -(install|uninstall)");
        }
    }
}
