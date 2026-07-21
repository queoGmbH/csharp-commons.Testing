using Build.Common.Enums;

namespace Build.Common.Services
{
    public interface IBranchService
    {
        string Clean(string branchName);
        Branches GetBranch(string branchName);
    }
}
