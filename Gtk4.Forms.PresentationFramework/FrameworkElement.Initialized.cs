using System.ComponentModel;

namespace System.Windows;

/// <summary>
/// <see cref="ISupportInitialize"/> and the <see cref="Initialized"/> lifecycle event.
/// </summary>
public abstract partial class FrameworkElement : ISupportInitialize
{
	bool _initPending;
	bool _isInitialized;

	/// <summary>
	/// Occurs once when the element is initialized: at <see cref="EndInit"/>, or — when no
	/// <see cref="BeginInit"/> is pending — lazily when the element is first attached to a
	/// parent, or at the latest just before its first <see cref="Loaded"/>. A CLR event, not routed.
	/// </summary>
	public event EventHandler? Initialized;

	/// <summary><see langword="true"/> once <see cref="Initialized"/> has been raised (set before its handlers run).</summary>
	public bool IsInitialized => _isInitialized;

	/// <summary><see langword="true"/> between <see cref="BeginInit"/> and <see cref="EndInit"/>.</summary>
	internal bool IsInitPending => _initPending;

	/// <summary>Starts the initialization of this element.</summary>
	/// <exception cref="InvalidOperationException">A <see cref="BeginInit"/> is already pending.</exception>
	public virtual void BeginInit()
	{
		if (_initPending)
			throw new InvalidOperationException("Cannot have nested BeginInit calls on the same instance.");
		_initPending = true;
		InitScope.Enter();
	}

	/// <summary>Ends the initialization of this element and raises <see cref="Initialized"/> (once).</summary>
	/// <exception cref="InvalidOperationException">No matching <see cref="BeginInit"/>.</exception>
	public virtual void EndInit()
	{
		if (!_initPending)
			throw new InvalidOperationException("Must call BeginInit before EndInit.");
		_initPending = false;
		try { TryFireInitialized(); }
		finally { InitScope.Exit(); }
	}

	/// <summary>
	/// Raises <see cref="Initialized"/>. <see cref="IsInitialized"/> is already
	/// <see langword="true"/> when this runs; it runs exactly once per element.
	/// </summary>
	protected virtual void OnInitialized(EventArgs e)
	{
		Initialized?.Invoke(this, e);
	}

	/// <summary>
	/// Marks the element initialized and calls <see cref="OnInitialized"/> unless it already
	/// is, or a <see cref="BeginInit"/> is pending (WPF's <c>TryFireInitialized</c>).
	/// </summary>
	internal void TryFireInitialized()
	{
		if (_initPending || _isInitialized)
			return;
		_isInitialized = true;
		OnInitialized(EventArgs.Empty);
	}
}
