using Xunit;

// GTK is not thread-safe; disable parallelism so tests run sequentially on one thread.
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
