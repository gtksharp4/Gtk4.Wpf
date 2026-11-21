using Xunit;

// One GTK thread and process-wide WPF state: tests must run one at a time.
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
