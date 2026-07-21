using Build.Common.Enums;

namespace Build.Common.Services.Impl
{
    public class BranchService : IBranchService
    {
        public string Clean(string branchName) =>
            branchName.ToLower()
                .Replace("/", "-")
                .Replace("_", "-")
                .Replace("#", "")
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ü", "ue");

        public Branches GetBranch(string branchName)
        {
            if (branchName == "master" || branchName == "main")
            {
                return Branches.Main;
            }

            if (branchName == "develop" || branchName == "dev")
            {
                return Branches.Develop;
            }

            if (branchName.StartsWith("feature/"))
            {
                return Branches.Feature;
            }

            if (branchName.StartsWith("hotfix") || branchName.StartsWith("fix"))
            {
                return Branches.Hotfix;
            }

            return Branches.Other;
        }
    }
}
