using System.Collections.Generic;
using System.Text;

namespace Build.Common.Builder
{
    /// <summary>
    /// Builder for dotnet test Coverlet agruments until https://github.com/Romanx/Cake.Coverlet gets released for Cake 2.0 ;-)
    /// </summary>
    public class DotNetTestCoverletParameterBuilder
    {
        private const string ParameterSeparator = " ";
        private const string ValueListStartEnd = "\"";
        private const string ValuesSeparator = "%2c"; /* see https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md note for PS and DevOps users */

        public static implicit operator string(DotNetTestCoverletParameterBuilder builder)
        {
            return builder.Build();
        }

        public bool CollectCoverage { get; set; }
        public string CoverletOutputFormat { get; set; }
        public string CoverletOutput { get; set; }
        public List<string> Exclude { get; set; }
        public List<string> ExcludeByFile { get; set; }

        public string Build()
        {
            // $" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput={testArtifactsPath}/{nameAndPath.Key}.coverage.xml /p:Exclude=\"[*.Tests?]*\" /p:ExcludeByFile=\"**/Data/Migrations/*.cs\""

            StringBuilder parameters = new StringBuilder();

            parameters.Append(ParameterSeparator); /* To separate parameters from 'dotnet test' command */
            parameters.Append("/p:CollectCoverage=").Append(CollectCoverage).Append(ParameterSeparator);
            parameters.Append("/p:CoverletOutputFormat=").Append(CoverletOutputFormat).Append(ParameterSeparator);
            parameters.Append("/p:CoverletOutput=").Append(CoverletOutput).Append(ParameterSeparator);

            parameters.Append("/p:Exclude=").Append(ValueListStartEnd).Append(string.Join(ValuesSeparator, Exclude)).Append(ValueListStartEnd).Append(ParameterSeparator);
            parameters.Append("/p:ExcludeByFile=").Append(ValueListStartEnd).Append(string.Join(ValuesSeparator, ExcludeByFile)).Append(ValueListStartEnd);

            return parameters.ToString();
        }
    }
}
