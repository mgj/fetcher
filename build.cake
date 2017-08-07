#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"
#tool "nuget:?package=vswhere"

var sln = new FilePath("Fetcher.sln");
var binDir = new DirectoryPath("bin");
var outputDir = new DirectoryPath("artifacts");
var target = Argument("target", "Default");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
	CleanDirectories(binDir.FullPath);
	CleanDirectories(outputDir.FullPath);
});

FilePath msBuildPath;
Task("ResolveBuildTools")
	.Does(() => 
{
	var vsLatest = VSWhereLatest();
	msBuildPath = (vsLatest == null)
		? null
		: vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");
});

GitVersion versionInfo = null;
Task("Version").Does(() => {
	GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.BuildServer
	});

	versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
	Information("VI:\t{0}", versionInfo.FullSemVer);
});

Task("Restore").Does(() => {
	NuGetRestore(sln);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.IsDependentOn("ResolveBuildTools")
	.Does(() =>  {

	var settings = new MSBuildSettings 
	{
		Configuration = "Release",
		ToolPath = msBuildPath
	};
	
	MSBuild(sln, settings);
});

Task("GitLink")
	.WithCriteria(() => IsRunningOnWindows())
	.IsDependentOn("Build")
	.Does(() => {
	GitLink("./", new GitLinkSettings {
		RepositoryUrl = "https://github.com/mgj/fetcher",
		ArgumentCustomization = args => args.Append(
			"-ignore fetcher.core.tests,fetcher.playground.core,fetcher.playground.droid,fetcher.playground.touch")
	});
});

Task("PackageAll")
	.IsDependentOn("GitLink")
	.Does(() => {

	EnsureDirectoryExists(outputDir);

	var nugetSettings = new NuGetPackSettings {
		Authors = new [] { "Mikkel Jensen" },
		Owners = new [] { "Mikkel Jensen" },
		IconUrl = new Uri("https://artm.dk/images/android-logo.png"),
		ProjectUrl = new Uri("https://github.com/mgj/fetcher"),
		LicenseUrl = new Uri("https://github.com/mgj/fetcher/blob/master/LICENSE"),
		Copyright = "Copyright (c) Mikkel Jensen",
		RequireLicenseAcceptance = false,
		Version = versionInfo.NuGetVersion,
		Symbols = false,
		NoPackageAnalysis = true,
		OutputDirectory = outputDir,
		Verbosity = NuGetVerbosity.Detailed,
		BasePath = "./nuspec"
	};
	nugetSettings.ReleaseNotes = ParseReleaseNotes("./releasenotes/fetcher.md").Notes.ToArray();
	NuGetPack("./nuspec/artm.fetcher.nuspec", nugetSettings);
});

Task("UploadAppVeyorArtifact")
	.IsDependentOn("PackageAll")
	.WithCriteria(() => !isPullRequest)
	.WithCriteria(() => isRunningOnAppVeyor)
	.Does(() => {

	Information("Artifacts Dir: {0}", outputDir.FullPath);

	foreach(var file in GetFiles(outputDir.FullPath + "/*")) {
		Information("Uploading {0}", file.FullPath);
		AppVeyor.UploadArtifact(file.FullPath);
	}
});

Task("Default")
	.IsDependentOn("UploadAppVeyorArtifact")
	.Does(() => {});

RunTarget(target);