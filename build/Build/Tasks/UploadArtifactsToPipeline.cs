using System.Linq;

using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Frosting;

namespace Build
{
    public sealed class UploadArtifactsToPipeline : FrostingTask<Context>
    {
        /// <summary>Runs the task using the specified context.</summary>
        /// <param name="context">The context.</param>
        public override void Run(Context context)
        {
            context.Information("Remote Build - Artefakte werden über die Pipeline bereitgestellt.");
            context.Information("Upload läuft ...");

            foreach (string pipelineFile in context.General.ArtifactsToUploadToPipeline.Concat(context.NuGet.NuGetPackages))
            {
                context.AzurePipelines().Commands.UploadArtifact(
                    "website",
                    pipelineFile,
                    "artifacts");
            }
        }

        /// <summary>Gets whether or not the task should be run.</summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     <c>true</c> if the task should run; otherwise <c>false</c>.
        /// </returns>
        public override bool ShouldRun(Context context) => !context.General.IsLocal;
    }
}
