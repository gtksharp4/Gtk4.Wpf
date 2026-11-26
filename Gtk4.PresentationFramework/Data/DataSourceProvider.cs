namespace System.Windows.Data;

/// <summary>
/// Abstract base class for data-source providers (<see cref="ObjectDataProvider"/>,
/// <see cref="XmlDataProvider"/>).  Implements the <c>ISupportInitialize</c> shape
/// so a provider declared as a <c>{StaticResource}</c> initialises lazily.
/// </summary>
public abstract class DataSourceProvider
{
	object? _data;
	bool _initialising;
	bool _initialLoadEnabled = true;
	bool _queried;

	// ── Public surface ───────────────────────────────────────────────────────

	/// <summary>The resulting data object exposed to bindings.</summary>
	public object? Data
	{
		get
		{
			if (!_queried) InitialLoad();
			return _data;
		}
		protected set
		{
			if (ReferenceEquals(_data, value)) return;
			_data = value;
			DataChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>Fires when <see cref="Data"/> changes.</summary>
	public event EventHandler? DataChanged;

	/// <summary>
	/// When <c>true</c> (default), the provider loads data the first time
	/// <see cref="Data"/> is accessed or <see cref="Refresh"/> is called.
	/// Set to <c>false</c> to defer until an explicit <see cref="Refresh"/>.
	/// </summary>
	public bool IsInitialLoadEnabled
	{
		get => _initialLoadEnabled;
		set => _initialLoadEnabled = value;
	}

	/// <summary>Triggers an initial load if <see cref="IsInitialLoadEnabled"/>.</summary>
	public void InitialLoad()
	{
		if (!_initialLoadEnabled) return;
		if (_initialising) return;
		_queried = true;
		_initialising = true;
		try { QueryData(); }
		finally { _initialising = false; }
	}

	/// <summary>Discards the current data and re-queries the source.</summary>
	public void Refresh()
	{
		_queried = true;
		QueryData();
	}

	// ── ISupportInitialize shape ─────────────────────────────────────────────

	/// <summary>Signals the start of an initialisation batch.</summary>
	public void BeginInit() { }

	/// <summary>Signals the end of an initialisation batch; calls <see cref="InitialLoad"/>.</summary>
	public void EndInit() => InitialLoad();

	// ── Protected extension point ────────────────────────────────────────────

	/// <summary>
	/// Derived classes override this to execute the query and call
	/// <see cref="OnQueryFinished"/> with the result.
	/// </summary>
	protected abstract void QueryData();

	/// <summary>
	/// Stores <paramref name="newData"/> as <see cref="Data"/> (fires
	/// <see cref="DataChanged"/>).  Call this from <see cref="QueryData"/>.
	/// </summary>
	protected void OnQueryFinished(object? newData) => Data = newData;
}
