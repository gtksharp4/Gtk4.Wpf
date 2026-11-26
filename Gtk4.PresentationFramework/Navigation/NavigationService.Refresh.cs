using System;
using System.Collections.Generic;

namespace System.Windows.Navigation;

public sealed partial class NavigationService
{
	// Source URI per navigated content, so GoBack/GoForward and Refresh know where an entry came from.
	readonly Dictionary<object, Uri> _sources =
		new Dictionary<object, Uri>(System.Collections.Generic.ReferenceEqualityComparer.Instance);

	// Set while Navigating handlers run, so StopLoading can cancel the navigation in flight.
	NavigatingCancelEventArgs? _pending;

	/// <summary>The URI of the current content, or <c>null</c> when it was navigated to as an object.</summary>
	public Uri? CurrentSource { get; private set; }

	object? Resolve(Uri uri) =>
		Resolver.Resolve(uri) ?? Markup.GtkFormsXamlRegistry.TryCreate(uri, Application.Current?.GetType().Assembly);

	Uri? SourceOf(object content) => _sources.TryGetValue(content, out Uri? uri) ? uri : null;

	internal bool HasSource(object content) => _sources.ContainsKey(content);

	void RememberSource(object? content, Uri? uri)
	{
		if (content == null) return;
		if (uri != null) _sources[content] = uri;
		else _sources.Remove(content);
	}

	// Drops the URI of content that no journal entry shows any more.
	void ForgetSource(object content)
	{
		if (!_journal.Contains(content)) _sources.Remove(content);
	}

	// Raises Navigating on the navigator; a cancelled navigation raises NavigationStopped.
	bool RaiseNavigating(NavigatingCancelEventArgs args)
	{
		var previous = _pending;
		_pending = args;
		try
		{
			Navigating?.Invoke(Host ?? this, args);
		}
		finally
		{
			_pending = previous;
		}
		if (!args.Cancel) return true;
		RaiseStopped(args.Content, args.Uri);
		return false;
	}

	void RaiseStopped(object? content, Uri? uri)
	{
		object navigator = Host ?? this;
		NavigationStopped?.Invoke(navigator, new NavigationEventArgs(uri, content, null, navigator, true));
	}

	/// <summary>
	/// Reloads the current content: a URI navigation is resolved again, an object navigation keeps its content.
	/// Raises <see cref="Navigating"/> with <see cref="NavigationMode.Refresh"/>, then <see cref="Navigated"/> and
	/// <see cref="LoadCompleted"/>. The journal keeps one entry. Does nothing before the first navigation.
	/// </summary>
	public void Refresh()
	{
		if (Content == null) return;
		Uri? uri = CurrentSource;
		object? target = uri == null ? Content : Resolve(uri) ?? Content;
		if (!RaiseNavigating(new NavigatingCancelEventArgs(target, uri, NavigationMode.Refresh))) return;
		if (!ReferenceEquals(target, Content))
		{
			var old = Content;
			_journal.ReplaceCurrent(target);
			ForgetSource(old);
		}
		CommitContent(target, uri, null);
	}

	/// <summary>
	/// Stops the navigation in progress, if a <see cref="Navigating"/> handler is running, and raises
	/// <see cref="NavigationStopped"/>. Navigation here is synchronous, so otherwise the event reports the
	/// current content.
	/// </summary>
	public void StopLoading()
	{
		if (_pending != null)
		{
			_pending.Cancel = true;
			return;
		}
		RaiseStopped(Content, CurrentSource);
	}
}
