using Xunit;

// Disable parallel test execution: tests use static state (DependencyProperty registry,
// EventManager registry). Parallelism would cause intermittent registration collisions.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
