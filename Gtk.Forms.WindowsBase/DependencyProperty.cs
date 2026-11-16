using System;
using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Identifies a dependency property: a name + owner pair with associated metadata.
	/// Instances are created only via the static <c>Register</c> / <c>RegisterAttached</c> /
	/// <c>RegisterReadOnly</c> factory methods.
	/// </summary>
	public sealed class DependencyProperty
	{
		/// <summary>
		/// Sentinel returned by <see cref="DependencyObject.ReadLocalValue"/> when no local
		/// value has been set.  It is never a valid property value.
		/// </summary>
		public static readonly object UnsetValue = new object();

		// ── Registry ────────────────────────────────────────────────────────────────
		// Key: (name, ownerType)
		static readonly Dictionary<(string, Type), DependencyProperty> _registry =
			new Dictionary<(string, Type), DependencyProperty>();
		// Inheriting DPs only — consulted when InheritanceParent changes.
		static readonly List<DependencyProperty> _inheritingRegistry = new List<DependencyProperty>();
		static readonly object _registryLock = new object();

		// ── Instance state ───────────────────────────────────────────────────────────
		/// <summary>Name used to register this property.</summary>
		public string Name { get; }

		/// <summary>CLR type of values this property accepts.</summary>
		public Type PropertyType { get; }

		/// <summary>Type that owns (registered) this property.</summary>
		public Type OwnerType { get; }

		/// <summary>Metadata supplied at registration time.</summary>
		public PropertyMetadata DefaultMetadata { get; }

		/// <summary><c>true</c> if this property is an attached property.</summary>
		public bool IsAttached { get; }

		/// <summary><c>true</c> if this property is read-only.</summary>
		public bool IsReadOnly { get; private set; }

		DependencyProperty(
			string name,
			Type propertyType,
			Type ownerType,
			PropertyMetadata metadata,
			bool isAttached)
		{
			Name = name;
			PropertyType = propertyType;
			OwnerType = ownerType;
			DefaultMetadata = metadata;
			IsAttached = isAttached;
		}

		// ── Metadata per owner-type (AddOwner / OverrideMetadata not yet wired — Phase 0) ──
		readonly Dictionary<Type, PropertyMetadata> _metadataByType =
			new Dictionary<Type, PropertyMetadata>();

		/// <summary>
		/// Returns the most-derived <see cref="PropertyMetadata"/> registered for
		/// <paramref name="forType"/>; falls back to <see cref="DefaultMetadata"/>.
		/// </summary>
		public PropertyMetadata GetMetadata(Type forType)
		{
			if (forType == null) throw new ArgumentNullException(nameof(forType));
			lock (_metadataByType)
			{
				Type? t = forType;
				while (t != null)
				{
					if (_metadataByType.TryGetValue(t, out var m)) return m;
					t = t.BaseType;
				}
			}
			return DefaultMetadata;
		}

		// ── Registration ─────────────────────────────────────────────────────────────

		/// <summary>Registers an instance dependency property.</summary>
		public static DependencyProperty Register(
			string name,
			Type propertyType,
			Type ownerType,
			PropertyMetadata? typeMetadata = null)
			=> RegisterCore(name, propertyType, ownerType,
				typeMetadata ?? new PropertyMetadata(GetDefaultDefault(propertyType)), false);

		/// <summary>Registers an attached dependency property.</summary>
		public static DependencyProperty RegisterAttached(
			string name,
			Type propertyType,
			Type ownerType,
			PropertyMetadata? defaultMetadata = null)
			=> RegisterCore(name, propertyType, ownerType,
				defaultMetadata ?? new PropertyMetadata(GetDefaultDefault(propertyType)), true);

		/// <summary>
		/// Registers a read-only dependency property; the returned key is required to
		/// call <see cref="DependencyObject.SetValue(DependencyPropertyKey,object?)"/>.
		/// </summary>
		public static DependencyPropertyKey RegisterReadOnly(
			string name,
			Type propertyType,
			Type ownerType,
			PropertyMetadata? typeMetadata = null)
		{
			var dp = RegisterCore(name, propertyType, ownerType,
				typeMetadata ?? new PropertyMetadata(GetDefaultDefault(propertyType)), false);
			dp.IsReadOnly = true;
			return new DependencyPropertyKey(dp);
		}

		static DependencyProperty RegisterCore(
			string name,
			Type propertyType,
			Type ownerType,
			PropertyMetadata metadata,
			bool isAttached)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
			if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

			var key = (name, ownerType);
			lock (_registryLock)
			{
				if (_registry.ContainsKey(key))
					throw new InvalidOperationException(
						$"DependencyProperty '{name}' is already registered on '{ownerType.FullName}'.");
				var dp = new DependencyProperty(name, propertyType, ownerType, metadata, isAttached);
				_registry[key] = dp;
				if (metadata is FrameworkPropertyMetadata fmeta && fmeta.Inherits)
					_inheritingRegistry.Add(dp);
				return dp;
			}
		}

		/// <summary>
		/// Returns a snapshot of all registered inheriting dependency properties.
		/// Called by <see cref="DependencyObjectInheritance"/> when the inheritance parent
		/// of an element changes.
		/// </summary>
		internal static IReadOnlyList<DependencyProperty> GetInheritingProperties()
		{
			lock (_registryLock)
				return new List<DependencyProperty>(_inheritingRegistry);
		}

		/// <summary>Returns the default default value for a type (0, false, null, etc.).</summary>
		static object? GetDefaultDefault(Type t)
		{
			if (t == null) throw new ArgumentNullException(nameof(t));
			return t.IsValueType ? Activator.CreateInstance(t) : null;
		}
	}

	/// <summary>
	/// Grants write access to a read-only <see cref="DependencyProperty"/>.
	/// Only the registering type should hold this key.
	/// </summary>
	public sealed class DependencyPropertyKey
	{
		/// <summary>The read-only <see cref="DependencyProperty"/> this key governs.</summary>
		public DependencyProperty DependencyProperty { get; }

		internal DependencyPropertyKey(DependencyProperty dp) => DependencyProperty = dp;
	}
}
