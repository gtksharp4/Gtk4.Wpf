using Xunit;

// Pure unit tests — no global state. Parallelisation is off to stay consistent
// with the rest of the repo's test assemblies.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
