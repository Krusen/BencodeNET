// For inspiration see: https://github.com/Jericho/Picton/blob/develop/build.cake

// Install addins
#addin "nuget:?package=Cake.Coveralls&version=0.9.0"
#addin "nuget:?package=Cake.Coverlet&version=2.2.1"

// Install tools
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#tool "nuget:?package=coveralls.io&version=1.4.2"
#tool "nuget:?package=xunit.runner.console&version=2.4.1"


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
var codeCoverageInclude = "[BencodeNET]*";

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
    var testSettings = new DotNetCoreTestSettings
    {
        // We need to rebuild for each run to use a different value for 'UseDefaultNetStandardVersion'
        //NoBuild = true,
        Configuration = configuration
    };

    Func<ProcessArgumentBuilder, ProcessArgumentBuilder> argsBuilder = args => args
        .Append("/p:CollectCoverage=true")
        .AppendSwitch("/p:CoverletOutputFormat", "=", "\\\"json,opencover\\\"") // Quotes needs to be escaped for 'dotnet test' (see https://github.com/Microsoft/msbuild/issues/2999)
        .Append("/p:CoverletOutput=../")
        .AppendSwitchQuoted("/p:Include", "=", codeCoverageInclude);

    testSettings.ArgumentCustomization = args => argsBuilder(args).Append("/p:UseDefaultNetStandardVersion=true");
    DotNetCoreTest(testProjectFile, testSettings);

    testSettings.ArgumentCustomization = args => argsBuilder(args).Append("/p:MergeWith=../coverage.json");
    DotNetCoreTest(testProjectFile, testSettings);
});

Task("Upload-Coverage-Result")
    .WithCriteria(() => !isLocalBuild)
    .Does(() =>
{
    CoverallsIo("coverage.opencover.xml");
});


///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("AppVeyor")
    .IsDependentOn("AppVeyor_Set-Build-Version")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Upload-Coverage-Result");

Task("Default")
    .IsDependentOn("AppVeyor");


///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);