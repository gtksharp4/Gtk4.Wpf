using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>
/// Creates an object from a CLR type (or uses an existing instance) and optionally
/// invokes a named method, exposing the result as <see cref="DataSourceProvider.Data"/>.
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

		// Invoke MethodName via reflection.
		var methodArgs = MethodParameters.Count > 0
			? new object?[MethodParameters.Count]
			: Array.Empty<object?>();
		for (int i = 0; i < MethodParameters.Count; i++)
			methodArgs[i] = MethodParameters[i];

		try
		{
			var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
			var method = target!.GetType().GetMethod(_methodName, flags);
			if (method == null)
			{
				OnQueryFinished(null);
				return;
			}
			var result = method.Invoke(target, methodArgs.Length == 0 ? null : methodArgs);
			OnQueryFinished(result);
		}
		catch (Exception ex)
		{
			OnQueryFinished(ex is TargetInvocationException tie && tie.InnerException != null
				? tie.InnerException
				: ex);
		}
	}
}
