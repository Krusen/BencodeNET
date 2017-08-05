// Install addins.
#addin "nuget:?package=Cake.Coveralls&version=0.5.0"

// Install tools.
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

var projectFile = "./BencodeNET/BencodeNET.csproj";

var testProjectFile = "./BencodeNET.Tests/BencodeNET.Tests.csproj";
var codeCoverageOutput = "coverage.xml";
var codeCoverageFilter = "+[*]* -[*.Tests]*";

var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();


///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information($"Building using version {cakeVersion} of Cake");
});


///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Update-Build-Version")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    var versionPeekXpath = "/Project/PropertyGroup/Version/text()";
    var versionPokeXpath = "/Project/PropertyGroup/Version";

    var buildNumber = AppVeyor.Environment.Build.Number;
    var version = XmlPeek(projectFile, versionPeekXpath);

    Information("AppVeyor build version is " + AppVeyor.Environment.Build.Version);
    Information("Project version is " + version);

    var parts = version.Split('.');
    version = string.Join(".", parts[0], parts[1], buildNumber);

    Information("Changing versions to " + version);

    AppVeyor.UpdateBuildVersion(version);
    XmlPoke(projectFile, versionPokeXpath, version);
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
        Configuration = configuration
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
    .Does(() =>
{
    CoverallsIo(codeCoverageOutput);
});


///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("AppVeyor")
    .IsDependentOn("Update-Build-Version")
    .IsDependentOn("Run-Code-Coverage")
    .IsDependentOn("Upload-Coverage-Result");

Task("Default")
    .IsDependentOn("AppVeyor");


///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);