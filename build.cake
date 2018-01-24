#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

string solutionFileName = "NRun.sln";
GitVersion gitVersion = null;
DotNetCoreMSBuildSettings msBuildSettings = null;

Task("Clean")
	.Does(() =>
	{
		CleanDirectories($"src/**/bin");
		CleanDirectories($"src/**/obj");
		CleanDirectories($"tests/**/bin");
		CleanDirectories($"tests/**/obj");
		CleanDirectories("artifacts");
	});

Task("Version")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		gitVersion = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
		Information("Assembly: {0}", gitVersion.AssemblySemVer);
		Information("NuGet: {0}", gitVersion.NuGetVersion);
		msBuildSettings = new DotNetCoreMSBuildSettings()
			.WithProperty("Version", gitVersion.NuGetVersion)
			.WithProperty("AssemblyVersion", gitVersion.AssemblySemVer)
			.WithProperty("FileVersion", gitVersion.AssemblySemVer);
	});

Task("Restore")
	.IsDependentOn("Version")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName, new DotNetCoreRestoreSettings
		{
			MSBuildSettings = msBuildSettings,
		});
	});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings
		{
			NoRestore = true,
			Configuration = configuration,
			MSBuildSettings = msBuildSettings,
		});
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => 
	{
		foreach(var project in GetFiles("./tests/**/*.UnitTests.csproj"))
		{
			DotNetCoreTest(project.ToString(), new DotNetCoreTestSettings
			{
				Configuration = configuration,
				NoBuild = true,
			});
		}
	});

Task("Package")
	.IsDependentOn("Test")
	.Does(() =>
	{
		DotNetCorePack(solutionFileName, new DotNetCorePackSettings
		{
			NoBuild = true,
			NoRestore = true,
			Configuration = configuration,
			OutputDirectory = "artifacts",
			MSBuildSettings = msBuildSettings,
		});
	});

Task("Default")
	.IsDependentOn("Build");

RunTarget(target);
