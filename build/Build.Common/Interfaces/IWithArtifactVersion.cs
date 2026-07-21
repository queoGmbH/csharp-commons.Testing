using Semver;

namespace Build.Common.Interfaces
{
    public interface IWithArtifactVersion
    {
        SemVersion ArtifactVersion { get; set; }
    }
}
