using NUnit.Framework;

// REQ-11: Demonstriert und verifiziert, dass die Bibliothek mit paralleler NUnit-Ausfuehrung auf
// Fixture-Ebene kompatibel ist, da sie keinen gemeinsamen veraenderbaren statischen Zustand haelt.
[assembly: Parallelizable(ParallelScope.Fixtures)]
