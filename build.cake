// For inspiration see: https://github.com/Jericho/Picton/blob/develop/build.cake

// Install addins.
#addin "nuget:?package=Cake.Coveralls&version=0.5.0"
#addin "nuget:?package=Cake.Json&version=1.0.2.13"

// Install tools.
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0012"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=coveralls.io&version=1.3.4"
#tool "nuget:?package=xunit.runner.console&version=2.2.0"


///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");


///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var libraryName = "BencodeNET";

var sourceFolder = "./";

var xunitRunnerJsonFile = "./BencodeNET.Tests/xunit.runner.json";
var testProjectFile = "./BencodeNET.Tests/BencodeNET.Tests.csproj";
var codeCoverageOutput = "coverage.xml";
var codeCoverageFilter = "+[*]* -[*.Tests]*";

var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();
var versionInfo = GitVersion(new GitVersionSettings() { OutputType = GitVersionOutput.Json });
var milestone = string.Concat("v", versionInfo.MajorMinorPatch);
var buildVersion = $"{versionInfo.SemVer}+{AppVeyor.Environment.Build.Number}";
var packageVersion = versionInfo.SemVer;

var isLocalBuild = BuildSystem.IsLocalBuild;
var	isPullRequest = BuildSystem.AppVeyor.Environment.PullRequest.IsPullRequest;
var	isTagged = (
    BuildSystem.AppVeyor.Environment.Repository.Tag.IsTag &&
    !string.IsNullOrWhiteSpace(BuildSystem.AppVeyor.Environment.Repository.Tag.Name)
);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information($"Building version {packageVersion} of {libraryName} using version {cakeVersion} of Cake" + Environment.NewLine);

    Information("Variables:" + Environment.NewLine
        + $"\t IsLocalBuild: {isLocalBuild}" + Environment.NewLine
        + $"\t IsPullRequest: {isPullRequest}" + Environment.NewLine
        + $"\t IsTagged: {isTagged}" + Environment.NewLine
    );

    Information("Versions:" + Environment.NewLine
        + $"\t Milestone: {milestone}" + Environment.NewLine
        + $"\t BuildNumber: {AppVeyor.Environment.Build.Number}" + Environment.NewLine
        + $"\t BuildVersion: {buildVersion}" + Environment.NewLine
        + $"\t PackageVersion: {packageVersion}" + Environment.NewLine
    );
});


///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("AppVeyor_Set-Build-Version")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UpdateBuildVersion(buildVersion);
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreBuild(sourceFolder + libraryName + ".sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.Append("/p:SemVer=" + packageVersion)
    });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest(testProjectFile, new DotNetCoreTestSettings
    {
        NoBuild = true,
        Configuration = configuration
    });

    DotNetCoreTest(testProjectFile, new DotNetCoreTestSettings
    {
        NoBuild = true,
        Configuration = configuration,
		ArgumentCustomization = args => args.Append("-- UseDefaultNetStandardVersion=true")
    });
});

Task("Disable-xUnit-ShadowCopy")
    .Does(() =>
{
    Information("Disabling xUnit shadow copying assemblies for code coverage to work...");
    SerializeJsonToFile(xunitRunnerJsonFile, new { shadowCopy = false });
});

Task("Run-Code-Coverage")
    .IsDependentOn("Disable-xUnit-ShadowCopy")
    .IsDependentOn("Build")
    .Does(() =>
{
    Action<ICakeContext> testAction = ctx =>
    {
        ctx.DotNetCoreTest(testProjectFile, new DotNetCoreTestSettings
        {
            NoBuild = true,
            Configuration = configuration
        });

        ctx.DotNetCoreTest(testProjectFile, new DotNetCoreTestSettings
        {
            NoBuild = true,
            Configuration = configuration,
			ArgumentCustomization = args => args.Append("-- UseDefaultNetStandardVersion=true")
        });
    };

    OpenCover(testAction,
        codeCoverageOutput,
        new OpenCoverSettings
        {
            OldStyle = true,
            SkipAutoProps = true,
            MergeOutput = false
        }
        .WithFilter(codeCoverageFilter)
    );

    if (FileExists(xunitRunnerJsonFile))
        DeleteFile(xunitRunnerJsonFile);
});

Task("Upload-Coverage-Result")
    .WithCriteria(() => !isLocalBuild)
    .Does(() =>
{
    CoverallsIo(codeCoverageOutput);
});


///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("AppVeyor")
    .IsDependentOn("AppVeyor_Set-Build-Version")
    .IsDependentOn("Run-Code-Coverage")
    .IsDependentOn("Upload-Coverage-Result");

Task("Default")
    .IsDependentOn("AppVeyor");


///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);