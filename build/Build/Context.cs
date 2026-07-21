using System.Collections.Generic;

using Build.Common.Enums;

using Cake.Core;
using Cake.Frosting;

using Semver;

namespace Build
{
    public class Context : FrostingContext
    {
        public Context(ICakeContext context)
            : base(context)
        {
        }

        public IList<Lib> Lib { get; } = new List<Lib>() {
            new Lib() { BuildConfig = "Release", MainProject = @"src\Commons.Testing\Commons.Testing.csproj" }
        };

        public General General { get; } = new();

        public Tests Tests { get; } = new();

        public NuGet NuGet { get; } = new();
    }

    public class General
    {
        public string ArtifactsDir => ".artifacts";

        public IList<string> ArtifactsToUploadToPipeline { get; } = new List<string>();

        public Branches CurrentBranch { get; set; }

        public string CurrentBranchName { get; set; }

        public bool IsLocal { get; set; }

        public string Name { get; } = "Commons.Testing";

        public string SolutionName { get; } = "Commons.Testing.sln";
    }

    public class Lib
    {
        public SemVersion ArtifactVersion { get; set; }
        public string BuildConfig { get; set; } = "Release";

        public string MainProject { get; set; }
    }

    public class NuGet
    {
        public IList<string> NuGetPackages { get; } = new List<string>();
    }

    public class Tests
    {
        public Dictionary<string, string> TestProjects { get; } = new Dictionary<string, string>() {
            { "Commons.Testing.Tests", @"tests\Commons.Testing.Tests\Commons.Testing.Tests.csproj" }
        };
        public string BuildConfig { get; } = "Release";
    }
}
