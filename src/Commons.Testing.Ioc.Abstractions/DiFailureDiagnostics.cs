using System.Text.RegularExpressions;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Best-effort extraction of the triggering service type from generic container/host bootstrap failures.
/// The "if determinable" leniency applies exclusively to these generic bootstrap failures (e.g. circular
/// dependencies uncovered by <c>validateScopes</c>). For the case of service resolution without a matching
/// registration, the service type is already known directly and is not determined via this class.
/// </summary>
internal static class DiFailureDiagnostics
{
    private static readonly Regex QuotedTypeNamePattern = new(@"'([^']+)'", RegexOptions.Compiled);

    /// <summary>
    /// Appends the triggering service type determined from <paramref name="exception"/> to
    /// <paramref name="baseMessage"/>, provided it can be extracted from the error message.
    /// </summary>
    public static string DescribeSetupFailure(string baseMessage, Exception exception)
    {
        var match = QuotedTypeNamePattern.Match(exception.Message);

        return match.Success
            ? $"{baseMessage} Ausloesender Service-Typ (soweit ermittelbar): '{match.Groups[1].Value}'."
            : baseMessage;
    }
}
