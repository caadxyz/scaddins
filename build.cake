using Cake.Common.Diagnostics;
using System.IO;

var target = Argument("target", "Default");
var solutionFile = GetFiles("src/*.sln").First();
var solutionFileWix = GetFiles("installer/SCaddins.Installer.wixproj").First();
var revitTestFrameworkGUIBin = @"../RevitTestFramework/bin/AnyCPU/Debug/RevitTestFrameworkGUI.exe";
var revitTestFrameworkBin = @"src/packages/RevitTestFramework.1.19.23/tools/RevitTestFrameworkConsole.exe";
var buildDir = Directory(@"./src/bin");
var testBuildDir = Directory(@"./src/bin");
var testAssemblyDllName = "SCaddins.Tests.dll";

// METHODS

public MSBuildSettings GetBuildSettings(string config)
{
	var result = new MSBuildSettings()
		.SetConfiguration(config)
		.WithTarget("Clean,Build")
		.WithProperty("Platform","x64")
		.SetVerbosity(Verbosity.Minimal);
	////result.WarningsAsError = true;
	return result;
}

public string GetTestAssembly(string revitVersion)
{
	return string.Format(@"tests/bin/x64/Release{0}/{1}",revitVersion, testAssemblyDllName);
}

public bool APIAvailable(string revitVersion)
{
	return FileExists(@"C:\Program Files\Autodesk\Revit " + revitVersion + @"\RevitAPI.dll");
}

// TASKS

Task("Clean").Does(() => CleanDirectory(buildDir));

Task("Restore-NuGet-Packages").Does(() => NuGetRestore(solutionFile));

Task("Restore-Installer-NuGet-Packages").Does(() => 
		{
		var settings = new NuGetRestoreSettings();
		settings.PackagesDirectory = @"installer/packages";
		settings.WorkingDirectory = @"installer";
		NuGetRestore(solutionFileWix, settings);
		});

Task("CreateAddinManifests")
.Does(() =>
		{
		string text = System.IO.File.ReadAllText(@"src\SCaddins.addin");
		System.IO.File.WriteAllText(@"src\bin\Release2016\SCaddins2016.addin", String.Copy(text).Replace("_REVIT_VERSION_", "2016"));
		System.IO.File.WriteAllText(@"src\bin\Release2017\SCaddins2017.addin", String.Copy(text).Replace("_REVIT_VERSION_", "2017"));
		System.IO.File.WriteAllText(@"src\bin\Release2018\SCaddins2018.addin", String.Copy(text).Replace("_REVIT_VERSION_", "2018"));
		System.IO.File.WriteAllText(@"src\bin\Release2019\SCaddins2019.addin", String.Copy(text).Replace("_REVIT_VERSION_", "2019"));
		});
Task("Revit2016") .IsDependentOn("Restore-NuGet-Packages")
.WithCriteria(APIAvailable("2016"))
.Does(() => MSBuild(solutionFile, GetBuildSettings("Release2016")));

Task("Revit2017")
.IsDependentOn("Restore-NuGet-Packages")
.WithCriteria(APIAvailable("2017"))
.Does(() => MSBuild(solutionFile, GetBuildSettings("Release2017")));

Task("Revit2018")
.IsDependentOn("Restore-NuGet-Packages")
.WithCriteria(APIAvailable("2018"))
.Does(() => MSBuild(solutionFile, GetBuildSettings("Release2018")));

Task("Revit2019")
.IsDependentOn("Restore-NuGet-Packages")
.WithCriteria(APIAvailable("2019"))
.Does(() => MSBuild(solutionFile, GetBuildSettings("Release2019")));

Task("Test2018")
.Does(() => StartProcess(new FilePath(revitTestFrameworkGUIBin).FullPath, GetTestAssembly("2018")));

Task("Test2019")
.Does(() => StartProcess(new FilePath(revitTestFrameworkGUIBin).FullPath, new FilePath(GetTestAssembly("2019")).MakeAbsolute(Context.Environment).FullPath.Replace(@"/",@"\")));

Task("Installer")
.IsDependentOn("Restore-Installer-NuGet-Packages")
.Does(() =>
		{
		Environment.SetEnvironmentVariable("R2016", APIAvailable("2016") ? "Enabled" : "Disabled");
		Environment.SetEnvironmentVariable("R2017", APIAvailable("2017") ? "Enabled" : "Disabled");
		Environment.SetEnvironmentVariable("R2018", APIAvailable("2018") ? "Enabled" : "Disabled");
		Environment.SetEnvironmentVariable("R2019", APIAvailable("2019") ? "Enabled" : "Disabled");
		var settings = new MSBuildSettings();
		settings.SetConfiguration("Release");
		settings.WithTarget("Clean,Build");
		settings.SetVerbosity(Verbosity.Minimal);
		settings.WorkingDirectory = new DirectoryPath(Environment.CurrentDirectory + @"\installer");
		MSBuild(solutionFileWix, settings);  
		});

Task("Dist")
.IsDependentOn("Default")
.IsDependentOn("Installer");

Task("Default")
.IsDependentOn("Clean")
.IsDependentOn("Revit2016")
.IsDependentOn("Revit2017")
.IsDependentOn("Revit2018")
.IsDependentOn("Revit2019")
.IsDependentOn("CreateAddinManifests");

RunTarget(target);
