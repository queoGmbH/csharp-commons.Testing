using System.IO;

using Build.Common.Enums;

using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Frosting;

namespace Build
{
    public sealed class Build : FrostingTask<Context>
    {
        public override void Run(Context context)
        {
            string versionSuffix;
            foreach (var projectToBuild in context.Lib)
            {
                if (!context.General.IsLocal && context.General.CurrentBranch == Branches.Main)
                {
                    versionSuffix = string.Empty;
                }
                else
                {
                    versionSuffix = $"{projectToBuild.ArtifactVersion.Prerelease}-{projectToBuild.ArtifactVersion.Build}";
                }

                context.DotNetBuild(Path.Combine(context.Environment.WorkingDirectory.FullPath, projectToBuild.MainProject),
                    new Cake.Common.Tools.DotNet.Build.DotNetBuildSettings()
                    {
                        Configuration = "Release",
                        OutputDirectory = "./.artifacts",
                        Verbosity = DotNetVerbosity.Normal,
                        VersionSuffix = versionSuffix,
                        // Every multi-targeted TFM would otherwise be built in parallel into the
                        // same flat OutputDirectory, causing concurrent writes to the same
                        // deps.json/dll and an intermittent "file in use" MSB4018 error.
                        MSBuildSettings = new DotNetMSBuildSettings().WithProperty("BuildInParallel", "false")
                    });
            }
        }
    }
}
