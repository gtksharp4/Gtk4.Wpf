namespace System.Windows.Navigation;

/// <summary>Handles <c>Navigated</c>.</summary>
public delegate void NavigatedEventHandler(object sender, NavigationEventArgs e);

/// <summary>Handles <c>LoadCompleted</c>.</summary>
public delegate void LoadCompletedEventHandler(object sender, NavigationEventArgs e);

/// <summary>Handles <c>Navigating</c>.</summary>
public delegate void NavigatingCancelEventHandler(object sender, NavigatingCancelEventArgs e);

/// <summary>Handles <c>NavigationStopped</c>.</summary>
public delegate void NavigationStoppedEventHandler(object sender, NavigationEventArgs e);
