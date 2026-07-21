using Build.Common.Enums;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using System.IO;

namespace Build.Tasks
{
    public sealed class BuildNuGetPackage : FrostingTask<Context>
    {
        public override void Run(Context context)
        {
            string versionPrefix, versionSuffix;
            foreach (var projectToBuild in context.Lib)
            {
                if (!context.General.IsLocal && context.General.CurrentBranch == Branches.Main)
                {
                    versionPrefix = $"{projectToBuild.ArtifactVersion.Major}.{projectToBuild.ArtifactVersion.Minor}.{projectToBuild.ArtifactVersion.Patch}";
                    versionSuffix = string.Empty;
                }
                else
                {
                    versionPrefix = $"{projectToBuild.ArtifactVersion.Major}.{projectToBuild.ArtifactVersion.Minor}.{projectToBuild.ArtifactVersion.Patch}";
                    versionSuffix = $"{projectToBuild.ArtifactVersion.Prerelease}-{projectToBuild.ArtifactVersion.Build}";
                }
                context.DotNetPack(Path.Combine(context.Environment.WorkingDirectory.FullPath, projectToBuild.MainProject),
                    new Cake.Common.Tools.DotNet.Pack.DotNetPackSettings()
                    {
                        Configuration = "Release",
                        OutputDirectory = "./.artifacts",
                        VersionSuffix = versionSuffix
                    });
            }
        }
    }
}
