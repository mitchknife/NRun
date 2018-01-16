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

Task("Build")
	.IsDependentOn("Version")
	.Does(() =>
	{
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings
		{
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
			Configuration = configuration,
			OutputDirectory = "artifacts",
			MSBuildSettings = msBuildSettings,
		});
	});

Task("Default")
	.IsDependentOn("Build");

RunTarget(target);
