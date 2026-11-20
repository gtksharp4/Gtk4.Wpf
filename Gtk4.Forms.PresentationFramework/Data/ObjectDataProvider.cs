using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>
/// Creates an object from a CLR type (or uses an existing instance) and optionally
/// invokes a named method, exposing the result as <see cref="DataSourceProvider.Data"/>.
/// <para>
/// Auto-refresh note: <see cref="ObjectDataProvider"/> does not monitor property changes on
/// the underlying object or its method parameters.  Call <see cref="DataSourceProvider.Refresh"/>
/// explicitly when the source data changes and a fresh query is needed.
/// </para>
/// </summary>
public class ObjectDataProvider : DataSourceProvider
{
	Type? _objectType;
	object? _objectInstance;
	string? _methodName;

	// ── Object source ────────────────────────────────────────────────────────

	/// <summary>
	/// Type to instantiate.  Mutually exclusive with <see cref="ObjectInstance"/>.
	/// Setting this clears <see cref="ObjectInstance"/>.
	/// </summary>
	public Type? ObjectType
	{
		get => _objectType;
		set
		{
			_objectType = value;
			_objectInstance = null;
		}
	}

	/// <summary>
	/// An existing object to use as the source.  Mutually exclusive with
	/// <see cref="ObjectType"/>.  Setting this clears <see cref="ObjectType"/>.
	/// </summary>
	public object? ObjectInstance
	{
		get => _objectInstance;
		set
		{
			_objectInstance = value;
			_objectType = null;
		}
	}

	// ── Method invocation ────────────────────────────────────────────────────

	/// <summary>
	/// Name of the method to invoke on the object.  When empty the object itself
	/// becomes <see cref="DataSourceProvider.Data"/>.
	/// </summary>
	public string? MethodName
	{
		get => _methodName;
		set => _methodName = value;
	}

	/// <summary>Parameters passed to <see cref="MethodName"/>.</summary>
	public ObservableCollection<object?> MethodParameters { get; } =
		new ObservableCollection<object?>();

	/// <summary>Parameters passed to the constructor when using <see cref="ObjectType"/>.</summary>
	public ObservableCollection<object?> ConstructorParameters { get; } =
		new ObservableCollection<object?>();

	// ── DataSourceProvider ───────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override void QueryData()
	{
		if (_objectInstance == null && _objectType != null
			&& FindMethod(_objectType) is { IsStatic: true } sm)
		{
			// A static method needs no instance (e.g. Enum.GetValues).
			Invoke(sm, null);
			return;
		}

		object? target;

		if (_objectInstance != null)
		{
			target = _objectInstance;
		}
		else if (_objectType != null)
		{
			try
			{
				var ctorArgs = ConstructorParameters.Count > 0
					? new object?[ConstructorParameters.Count]
					: Array.Empty<object?>();
				for (int i = 0; i < ConstructorParameters.Count; i++)
					ctorArgs[i] = ConstructorParameters[i];

				target = ctorArgs.Length == 0
					? Activator.CreateInstance(_objectType)
					: Activator.CreateInstance(_objectType, ctorArgs);
			}
			catch (Exception ex)
			{
				OnQueryFinished(ex);
				return;
			}
		}
		else
		{
			OnQueryFinished(null);
			return;
		}

		if (string.IsNullOrEmpty(_methodName))
		{
			OnQueryFinished(target);
			return;
		}

		var method = FindMethod(target!.GetType());
		if (method == null)
		{
			OnQueryFinished(null);
			return;
		}
		Invoke(method, target);
	}

	// An overload whose parameter types accept the arguments wins (Math.Max with two doubles); failing
	// that, the first with a matching parameter count.
	MethodInfo? FindMethod(Type type)
	{
		if (string.IsNullOrEmpty(_methodName)) return null;
		var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
		MethodInfo? fallback = null;
		foreach (var m in type.GetMethods(flags))
		{
			if (m.Name != _methodName || m.ContainsGenericParameters) continue;
			var ps = m.GetParameters();
			if (ps.Length != MethodParameters.Count) continue;
			fallback ??= m;
			if (AcceptsArguments(ps)) return m;
		}
		return fallback;
	}

	bool AcceptsArguments(ParameterInfo[] ps)
	{
		for (int i = 0; i < ps.Length; i++)
		{
			var arg = MethodParameters[i];
			var type = ps[i].ParameterType;
			if (arg == null ? type.IsValueType && Nullable.GetUnderlyingType(type) == null : !type.IsInstanceOfType(arg))
				return false;
		}
		return true;
	}

	void Invoke(MethodInfo method, object? target)
	{
		var args = new object?[MethodParameters.Count];
		for (int i = 0; i < args.Length; i++)
			args[i] = MethodParameters[i];
		try
		{
			OnQueryFinished(method.Invoke(target, args.Length == 0 ? null : args));
		}
		catch (Exception ex)
		{
			OnQueryFinished(ex is TargetInvocationException tie && tie.InnerException != null
				? tie.InnerException
				: ex);
		}
	}
}
