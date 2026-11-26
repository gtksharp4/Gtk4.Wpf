using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>
/// Implements an identity system for <see cref="DependencyObject"/>-derived types.
/// Mirrors <c>System.Windows.DependencyObjectType</c>.
/// </summary>
/// <remarks>
/// Each unique <see cref="System.Type"/> that is a subtype of
/// <see cref="DependencyObject"/> maps to exactly one <see cref="DependencyObjectType"/>
/// instance. Use <see cref="FromSystemType"/> to obtain it.
/// </remarks>
public sealed class DependencyObjectType
{
	static readonly Dictionary<Type, DependencyObjectType> _cache =
		new Dictionary<Type, DependencyObjectType>();
	static readonly object _lock = new object();
	static int _nextId;

	/// <summary>Unique numeric identity of this type within this process.</summary>
	public int Id { get; }

	/// <summary>Simple name of the underlying CLR type.</summary>
	public string Name => SystemType.Name;

	/// <summary>The underlying CLR type.</summary>
	public Type SystemType { get; }

	/// <summary>
	/// The <see cref="DependencyObjectType"/> for the base type, or <c>null</c>
	/// when this type's base is not a <see cref="DependencyObject"/> subtype.
	/// </summary>
	public DependencyObjectType? BaseType { get; }

	DependencyObjectType(int id, Type systemType, DependencyObjectType? baseType)
	{
		Id = id;
		SystemType = systemType;
		BaseType = baseType;
	}

	// ── Factory ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the <see cref="DependencyObjectType"/> for <paramref name="systemType"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="ArgumentException">
	/// Thrown when <paramref name="systemType"/> is not a subtype of
	/// <see cref="DependencyObject"/>.
	/// </exception>
	public static DependencyObjectType FromSystemType(Type systemType)
	{
		if (systemType == null) throw new ArgumentNullException(nameof(systemType));
		if (!typeof(DependencyObject).IsAssignableFrom(systemType))
			throw new ArgumentException(
				$"Type '{systemType.FullName}' is not a DependencyObject subtype.",
				nameof(systemType));

		return GetOrCreate(systemType);
	}

	static DependencyObjectType GetOrCreate(Type t)
	{
		lock (_lock)
		{
			if (_cache.TryGetValue(t, out var cached)) return cached;

			// Build base first (recursion stops at DependencyObject's own base).
			DependencyObjectType? baseDoType = null;
			if (t.BaseType != null && typeof(DependencyObject).IsAssignableFrom(t.BaseType))
				baseDoType = GetOrCreate(t.BaseType);

			var dot = new DependencyObjectType(_nextId++, t, baseDoType);
			_cache[t] = dot;
			return dot;
		}
	}

	// ── Type-testing ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns <c>true</c> when <paramref name="dependencyObject"/> is an instance of
	/// this type or a derived type.
	/// </summary>
	public bool IsInstanceOfType(DependencyObject? dependencyObject) =>
		dependencyObject != null && SystemType.IsInstanceOfType(dependencyObject);

	/// <summary>
	/// Returns <c>true</c> when this type is a strict subclass of
	/// <paramref name="dependencyObjectType"/>.
	/// </summary>
	public bool IsSubclassOf(DependencyObjectType dependencyObjectType)
	{
		if (dependencyObjectType == null) throw new ArgumentNullException(nameof(dependencyObjectType));
		return SystemType.IsSubclassOf(dependencyObjectType.SystemType);
	}
}
