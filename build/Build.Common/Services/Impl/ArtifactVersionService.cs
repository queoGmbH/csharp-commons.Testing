using System;

using Build.Common.Enums;
using Build.Common.Extensions;

using Semver;

namespace Build.Common.Services.Impl
{
    public class ArtifactVersionService : IArtifactVersionService
    {
        private readonly IBranchService _branchService;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public ArtifactVersionService(IBranchService branchService) => _branchService = branchService;

        public SemVersion GetArtifactVersion(
            bool isLocalBuild, Branches branch, string branchName,
            string buildNumber, Action<string> logInformation, string assemblyInfo)
        {
            string[] versionParts = assemblyInfo.Split('.');
            string tag;
            string build;
            if (!isLocalBuild)
            {
                switch (branch)
                {
                    case Branches.Main:
                    case Branches.Master:
                        logInformation.Invoke("Remote Master Branch - es werden keine Tags an die Version angefügt.");
                        tag = string.Empty;
                        build = string.Empty;
                        break;
                    case Branches.Develop:
                        logInformation.Invoke("Remote Develop Branch - es wird Beta + Buildnummer angefügt.");
                        tag = "beta";
                        build = buildNumber;
                        break;
                    default:
                        logInformation.Invoke("Remote Branch - es wird der Branchname + Buildnummer angefügt.");
                        tag = _branchService.Clean(branchName);
                        build = buildNumber;
                        break;
                }
            }
            else
            {
                logInformation.Invoke("Lokaler Branch - es werden local und Datum + Uhrzeit für lokalen Build angefügt.");
                switch (branch)
                {
                    case Branches.Main:
                    case Branches.Master:
                        logInformation.Invoke("Lokaler Master Branch - es werden keine Tags an die Version angefügt.");
                        tag = "local";
                        build = string.Empty;
                        break;
                    case Branches.Develop:
                        logInformation.Invoke("Remote Develop Branch - es wird Beta + Buildnummer angefügt.");
                        tag = "local-beta";
                        build = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        break;
                    default:
                        logInformation.Invoke("Remote Branch - es wird der Branchname + Buildnummer angefügt.");
                        tag = $"local-{_branchService.Clean(branchName)}";
                        build = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        break;
                }
            }

            return new SemVersion(versionParts[0].AsInt(), versionParts[1].AsInt(), versionParts[2].AsInt(), tag,
                build);
        }
    }
}
