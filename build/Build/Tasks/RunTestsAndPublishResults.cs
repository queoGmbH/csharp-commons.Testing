using System.Collections.Generic;
using Cake.Common.Build;
using Cake.Common.Build.AzurePipelines.Data;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.IO.Arguments;
using Cake.Frosting;

using Path = System.IO.Path;

namespace Build
{
    public sealed class RunTestsAndPublishResults : FrostingTask<Context>
    {
        /// <summary>Runs the task using the specified context.</summary>
        /// <param name="context">The context.</param>
        public override void Run(Context context)
        {
            IDictionary<string, string> testProjects = new Dictionary<string, string>();
            foreach (var testProject in context.Tests.TestProjects)
            {
                testProjects.Add(
                    testProject.Key,
                    Path.Combine(context.Environment.WorkingDirectory.FullPath, testProject.Value));
            }

            string testArtifactsPath = Path.Combine(context.Environment.WorkingDirectory.FullPath,
                $"{context.General.ArtifactsDir}.tests");

            context.CleanDirectory(testArtifactsPath);

            try
            {
                foreach (KeyValuePair<string, string> nameAndPath in testProjects)
                {
                    const string runSettingsOverrides =
                        "-- " +
                        "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover " +
                        "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=\"[*.Tests?]*\"";

                    context.DotNetTest(
                        nameAndPath.Value,
                        new DotNetTestSettings
                        {
                            VSTestReportPath =
                                Path.Combine(testArtifactsPath, $"{nameAndPath.Key}.TestResult.xml"),
                            Configuration = context.Tests.BuildConfig,
                            Collectors = new List<string> { "XPlat Code Coverage" },
                            ResultsDirectory = new DirectoryPath(Path.Combine(testArtifactsPath, nameAndPath.Key)),
                            ArgumentCustomization = delegate (ProcessArgumentBuilder argument)
                            {
                                argument.Append(new TextArgument(runSettingsOverrides));
                                return argument;
                            }
                        });
                }
                if (context.Environment.Platform.IsWindows())
                {
                    context.ReportGenerator(new GlobPattern($"{testArtifactsPath}/**/coverage.opencover.xml"), Path.Combine(testArtifactsPath, "coverage"), new ReportGeneratorSettings()
                    {
                        ReportTypes = new List<ReportGeneratorReportType>()
                        {
                            ReportGeneratorReportType.Cobertura,
                            ReportGeneratorReportType.HtmlInline_AzurePipelines
                        }
                    });
                }
            }
            finally
            {
                if (!context.BuildSystem().IsLocalBuild)
                {
                    foreach (KeyValuePair<string, string> nameAndPath in testProjects)
                    {
                        context.AzurePipelines().Commands.PublishTestResults(
                            new AzurePipelinesPublishTestResultsData
                            {
                                TestResultsFiles =
                                    new List<FilePath>
                                    {
                                        Path.Combine(context.Environment.WorkingDirectory.FullPath,
                                            $"{context.General.ArtifactsDir}.tests",
                                            $"{nameAndPath.Key}.TestResult.xml")
                                    },
                                TestRunner = AzurePipelinesTestRunnerType.VSTest
                            });
                    }
                    if (context.Environment.Platform.IsWindows())
                    {
                        context.AzurePipelines().Commands.PublishCodeCoverage(new AzurePipelinesPublishCodeCoverageData
                        {
                            CodeCoverageTool = AzurePipelinesCodeCoverageToolType.Cobertura,
                            SummaryFileLocation = Path.Combine(testArtifactsPath, "coverage/Cobertura.xml"),
                            ReportDirectory = Path.Combine(testArtifactsPath, "coverage")
                        });
                    }
                }
            }
        }
    }
}
