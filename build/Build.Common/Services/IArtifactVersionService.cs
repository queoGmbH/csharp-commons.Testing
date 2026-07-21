using System;

using Build.Common.Enums;

using Semver;

namespace Build.Common.Services
{
    public interface IArtifactVersionService
    {
        SemVersion GetArtifactVersion(
            bool isLocalBuild, Branches branch, string branchName, string buildNumber, Action<string> logInformation,
            string assemblyInfo);
    }
}
