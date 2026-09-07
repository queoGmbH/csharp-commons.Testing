using NUnit.Framework;

// REQ-11: Demonstrates and verifies that the library is compatible with parallel NUnit execution at the
// fixture level, since it does not hold any shared mutable static state. Multiple WebApplicationFactory
// instances per test class run in parallel and independently of one another.
[assembly: Parallelizable(ParallelScope.Fixtures)]
