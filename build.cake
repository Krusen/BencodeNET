// For inspiration see: https://github.com/Jericho/Picton/blob/develop/build.cake

// Install addins.
#addin "nuget:?package=Cake.Coveralls&version=0.5.0"

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

var testProjectFile = "./BencodeNET.Tests/BencodeNET.Tests.csproj";
var codeCoverageOutput = "coverage.xml";
var codeCoverageFilter = "+[*]* -[*.Tests]*";

var versionInfo = GitVersion(new GitVersionSettings() { OutputType = GitVersionOutput.Json });
var milestone = string.Concat("v", versionInfo.MajorMinorPatch);
var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

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
    Information($"Building version {versionInfo.SemVer} of {libraryName} using version {cakeVersion} of Cake");

    Information("Variables:\r\n"
        + $"\tIsLocalBuild: {isLocalBuild}\r\n"
        + $"\tIsPullRequest: {isPullRequest}\r\n"
        + $"\tIsTagged: {isTagged}"
    );

    Information("GitVersion:\r\n"
        + $"\tSemVer: {versionInfo.SemVer}\r\n"
        + $"\tLegacySemVer: {versionInfo.LegacySemVer}\r\n"
        + $"\tNuGetVersionV2: {versionInfo.NuGetVersionV2}\r\n"
        + $"\tFullSemVer: {versionInfo.FullSemVer}"
    );
});


///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("AppVeyor_Set-Build-Version")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    var version = $"{versionInfo.SemVer}+{AppVeyor.Environment.Build.Number}";
    Information("Setting AppVeyor build version to " + version);
    AppVeyor.UpdateBuildVersion(version);
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
        ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.SemVer)
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
});

Task("Run-Code-Coverage")
    .IsDependentOn("Build")
    .Does(() =>
{
    Action<ICakeContext> testAction = ctx => ctx.DotNetCoreTest(testProjectFile, new DotNetCoreTestSettings
    {
        NoBuild = true,
        Configuration = configuration
    });

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