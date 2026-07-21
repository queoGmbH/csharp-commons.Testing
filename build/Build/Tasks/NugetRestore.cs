using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace Build
{
    public class NugetRestore : FrostingTask<Context>
    {
        /// <summary>Runs the task using the specified context.</summary>
        /// <param name="context">The context.</param>
        public override void Run(Context context) => context.DotNetRestore();
    }
}
