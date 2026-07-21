using System;

using Build.Common.Extensions;
using Build.Common.Services.Impl;

using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Frosting;

namespace Build
{
    public sealed class Lifetime : FrostingLifetime<Context>
    {
        public override void Setup(Context context, ISetupContext info)
        {
            context.Information("Setting things up...");

            context.CleanDirectory(context.General.ArtifactsDir.AsDirectoryPath());

            context.General.IsLocal = context.BuildSystem().IsLocalBuild;

            SetBranchInContext(context);

            context.Information(string.Join(Environment.NewLine, context.Environment.GetEnvironmentVariables()));
        }

        public override void Teardown(Context context, ITeardownContext info) =>
            context.Information("Tearing things down...");

        private void SetBranchInContext(Context context)
        {
            string branchName = context.GitVersion(new GitVersionSettings { NoFetch = true }).BranchName.ToLower();
            context.General.CurrentBranchName = branchName;
            context.General.CurrentBranch = new BranchService().GetBranch(branchName);
        }
    }
}
