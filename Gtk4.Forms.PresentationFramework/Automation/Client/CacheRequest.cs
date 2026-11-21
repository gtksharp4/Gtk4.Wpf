using System.Collections.Generic;

namespace System.Windows.Automation;

/// <summary>
/// Says which properties and patterns to snapshot when an element is retrieved. An active request applies to
/// every element the calling thread retrieves until it is deactivated.
/// </summary>
public sealed class CacheRequest
{
	[ThreadStatic]
	static Stack<CacheRequest>? _stack;

	readonly List<AutomationProperty> _properties = new List<AutomationProperty>();
	readonly List<AutomationPattern> _patterns = new List<AutomationPattern>();
	TreeScope _scope = TreeScope.Element;
	Condition _filter = Automation.ControlViewCondition;
	AutomationElementMode _mode = AutomationElementMode.Full;
	bool _active;

	/// <summary>The request on top of the calling thread's stack, or an empty request.</summary>
	public static CacheRequest Current => ActiveOrNull ?? new CacheRequest();

	internal static CacheRequest? ActiveOrNull => _stack != null && _stack.Count > 0 ? _stack.Peek() : null;

	internal bool IsEmpty => _properties.Count == 0 && _patterns.Count == 0 && _scope == TreeScope.Element;

	internal IReadOnlyList<AutomationProperty> Properties => _properties;

	internal IReadOnlyList<AutomationPattern> Patterns => _patterns;

	public void Add(AutomationProperty property)
	{
		if (property == null) throw new ArgumentNullException(nameof(property));
		VerifyNotActive();
		if (!_properties.Contains(property)) _properties.Add(property);
	}

	public void Add(AutomationPattern pattern)
	{
		if (pattern == null) throw new ArgumentNullException(nameof(pattern));
		VerifyNotActive();
		if (!_patterns.Contains(pattern)) _patterns.Add(pattern);
	}

	/// <summary>
	/// Which elements around the retrieved one are cached: itself and its children. Descendants is accepted
	/// but not cached.
	/// </summary>
	public TreeScope TreeScope
	{
		get => _scope;
		set
		{
			VerifyNotActive();
			if (value == 0 || (value & ~TreeScope.Subtree) != 0)
				throw new ArgumentException("The scope must combine Element, Children and Descendants.",
						nameof(value));
			_scope = value;
		}
	}

	/// <summary>The view that decides which children and parent are cached.</summary>
	public Condition TreeFilter
	{
		get => _filter;
		set
		{
			VerifyNotActive();
			_filter = value ?? throw new ArgumentNullException(nameof(value));
		}
	}

	/// <summary>Always honoured as <see cref="AutomationElementMode.Full"/>: cached elements keep their peer.</summary>
	public AutomationElementMode AutomationElementMode
	{
		get => _mode;
		set
		{
			VerifyNotActive();
			_mode = value;
		}
	}

	public CacheRequest Clone()
	{
		var copy = new CacheRequest { _scope = _scope, _filter = _filter, _mode = _mode };
		copy._properties.AddRange(_properties);
		copy._patterns.AddRange(_patterns);
		return copy;
	}

	/// <summary>Makes the request the current one until the returned object is disposed.</summary>
	public IDisposable Activate()
	{
		Push();
		return new Activation(this);
	}

	public void Push()
	{
		(_stack ??= new Stack<CacheRequest>()).Push(this);
		_active = true;
	}

	public void Pop()
	{
		if (_stack == null || _stack.Count == 0 || !ReferenceEquals(_stack.Peek(), this))
			throw new InvalidOperationException("The request is not the active cache request.");
		_stack.Pop();
		_active = _stack.Contains(this);
	}

	void VerifyNotActive()
	{
		if (_active) throw new InvalidOperationException("An active cache request cannot be changed.");
	}

	sealed class Activation : IDisposable
	{
		CacheRequest? _request;

		internal Activation(CacheRequest request) => _request = request;

		public void Dispose()
		{
			_request?.Pop();
			_request = null;
		}
	}
}
