using System.Text.RegularExpressions;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Bester-Versuch-Extraktion des ausloesenden Service-Typs aus generischen Container-/Host-Aufbaufehlern
/// (siehe REQ-10). Die Kulanzregelung "falls ermittelbar" gilt ausschliesslich fuer diese generischen
/// Aufbaufehler (z. B. zyklische Abhaengigkeiten, die durch <c>validateScopes</c> aufgedeckt werden). Fuer den
/// durch REQ-07 abgedeckten Fall - Serviceauflosung ohne passende Registrierung - ist der Service-Typ bereits
/// direkt bekannt und wird nicht ueber diese Klasse ermittelt.
/// </summary>
internal static class DiFailureDiagnostics
{
    private static readonly Regex QuotedTypeNamePattern = new(@"'([^']+)'", RegexOptions.Compiled);

    /// <summary>
    /// Ergaenzt <paramref name="baseMessage"/> um den aus <paramref name="exception"/> ermittelten
    /// ausloesenden Service-Typ, sofern dieser sich aus der Fehlermeldung extrahieren laesst.
    /// </summary>
    public static string DescribeSetupFailure(string baseMessage, Exception exception)
    {
        var match = QuotedTypeNamePattern.Match(exception.Message);

        return match.Success
            ? $"{baseMessage} Ausloesender Service-Typ (soweit ermittelbar): '{match.Groups[1].Value}'."
            : baseMessage;
    }
}
