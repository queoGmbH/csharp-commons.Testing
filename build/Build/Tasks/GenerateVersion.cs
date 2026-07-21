using System.IO;

using Build.Common.Services;
using Build.Common.Services.Impl;

using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.Xml;
using Cake.Frosting;

namespace Build
{
    public sealed class GenerateVersion : FrostingTask<Context>
    {
        private Context _context;

        public override void Run(Context context)
        {
            foreach (var projectToBuild in context.Lib)
            {
                string assemblyVersion = context.XmlPeek(
                    Path.Combine(context.Environment.WorkingDirectory.FullPath, projectToBuild.MainProject),
                    "//VersionPrefix");
                context.Information($"Found {assemblyVersion} as assembly version in {projectToBuild.MainProject} csproj file.");
                _context = context;
                IArtifactVersionService versionService = new ArtifactVersionService(new BranchService());
                projectToBuild.ArtifactVersion = versionService.GetArtifactVersion(
                    context.General.IsLocal,
                    context.General.CurrentBranch,
                    context.General.CurrentBranchName,
                    context.AzurePipelines()?.Environment.Build.Number,
                    LogInformation,
                    assemblyVersion);

                context.Information($"Artifact Version: {projectToBuild.ArtifactVersion}");
            }
        }

        private void LogInformation(string informationToLog) => _context.Information(informationToLog);
    }
}
