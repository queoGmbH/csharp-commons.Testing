using System;
using System.Linq;

using Build.Common.Enums;

using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.NuGet.Push;
using Cake.Frosting;

namespace Build
{
    public class PushNuGetPackagesToQueo : FrostingTask<Context>
    {
        public override void Run(Context context)
        {
            CheckRequirements(context);

            foreach (string contextPackageFile in context.NuGet.NuGetPackages)
            {
                context.NuGetPush(contextPackageFile, new NuGetPushSettings()
                {
                    Source = context.Arguments.GetArguments("NuGetSource").First(),
                    ApiKey = context.Arguments.GetArguments("NuGetKey").First()
                });
            }
        }

        /// <summary>Gets whether or not the task should be run.</summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     <c>true</c> if the task should run; otherwise <c>false</c>.
        /// </returns>
        public override bool ShouldRun(Context context)
        {
            return !context.General.IsLocal && context.General.CurrentBranch == Branches.Main;
        }

        private void CheckRequirements(Context context)
        {
            /* Für diese Aufgabe werden Passwort und Nutzername für den queo Transfer FTP benötigt. */

            if (!context.Arguments.HasArgument("NuGetSource"))
            {
                throw new MissingFieldException("NuGetSource nicht vorhanden");
            }

            if (!context.Arguments.HasArgument("NuGetKey"))
            {
                throw new MissingFieldException("NuGetKey nicht vorhanden");
            }
        }
    }
}
