using Build.Tasks;
using Cake.Frosting;

namespace Build {
    [IsDependentOn(typeof(FormatCheck))]
    [IsDependentOn(typeof(NugetRestore))]
    [IsDependentOn(typeof(GenerateVersion))]
    [IsDependentOn(typeof(Build))]
    [IsDependentOn(typeof(RunTestsAndPublishResults))]
    [IsDependentOn(typeof(BuildNuGetPackage))]
    [IsDependentOn(typeof(GetNuGetPackagesFromArtifacts))]
    [IsDependentOn(typeof(UploadArtifactsToPipeline))]
    public partial class Default { }

    [IsDependentOn(typeof(FormatCheck))]
    [IsDependentOn(typeof(NugetRestore))]
    [IsDependentOn(typeof(GenerateVersion))]
    [IsDependentOn(typeof(Build))]
    [IsDependentOn(typeof(RunTestsAndPublishResults))]
    public partial class BuildAndTest { }

    [IsDependentOn(typeof(NugetRestore))]
    [IsDependentOn(typeof(GenerateVersion))]
    [IsDependentOn(typeof(BuildNuGetPackage))]
    [IsDependentOn(typeof(GetNuGetPackagesFromArtifacts))]
    [IsDependentOn(typeof(UploadArtifactsToPipeline))]
    public partial class BuildPackage { }
}
