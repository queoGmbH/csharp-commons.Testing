using System;

using Cake.Common.Diagnostics;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Tool;
using Cake.Core;
using Cake.Frosting;

namespace Build
{
    public class FormatCheck : FrostingTask<Context>
    {
        /// <summary>
        ///     The error handler to be executed using the specified context if an exception occurs in the task.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        public override void OnError(Exception exception, Context context)
        {
            context.Error(
                $"Your code does not align with our code style conventions! Please run 'dotnet format whitespace --folder' and/or `dotnet format {context.General.SolutionName}` while executing this from src-Folder locally to format the code! You might need to install the dotnet-format tool on your machine with the command 'dotnet tool install -g dotnet-format'.");
            throw exception;
        }

        /// <summary>Runs the task using the specified context.</summary>
        /// <param name="context">The context.</param>
        public override void Run(Context context)
        {
            context.DotNetTool(
                "format",
                new DotNetToolSettings { ArgumentCustomization = args => args.Append("whitespace").Append("--folder").Append("--verify-no-changes") });

            context.Information("Format finished! Your code looks awesome!");
        }
    }
}
