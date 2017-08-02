var target = Argument("target", "Default");

Task("Set-Build-Version")
    .Does(() =>
{
    var projectFile = "./BencodeNET/BencodeNET.csproj";
    var versionPeekXpath = "/Project/PropertyGroup/Version/text()";
    var versionPokeXpath = "/Project/PropertyGroup/Version";

    var version = XmlPeek(projectFile, versionPeekXpath);
    var parts = version.Split('.');

    var buildNumber = 0;

    if (BuildSystem.IsRunningOnAppVeyor)
    {
        buildNumber = AppVeyor.Environment.Build.Number;
    }

    version = string.Join(".", parts[0], parts[1], buildNumber);

    if (BuildSystem.IsRunningOnAppVeyor)
    {
        AppVeyor.UpdateBuildVersion(version);
        Information("Updated AppVeyor build version to " + version);
    }

    XmlPoke(projectFile, versionPokeXpath, version);
    Information("Set project version to " + version);
});

Task("Default")
    .IsDependentOn("Set-Build-Version");

RunTarget(target);