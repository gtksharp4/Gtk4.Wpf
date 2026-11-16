using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

		// ── Metadata per owner-type ──────────────────────────────────────────────────
		readonly Dictionary<Type, PropertyMetadata> _metadataByType =
			new Dictionary<Type, PropertyMetadata>();

		/// <summary>
		/// Overrides the metadata for this property on <paramref name="forType"/> and its
		/// subclasses.  Used in static constructors of derived controls to supply a
		/// type-specific default value (e.g. <c>DefaultStyleKey</c>).
		/// </summary>
		public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
		{
			if (forType == null) throw new ArgumentNullException(nameof(forType));
			if (typeMetadata == null) throw new ArgumentNullException(nameof(typeMetadata));
			lock (_metadataByType)
				_metadataByType[forType] = typeMetadata;
		}

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

		/// <summary>
		/// Finds a registered <see cref="DependencyProperty"/> by <paramref name="name"/>
		/// for <paramref name="ownerType"/> (walks up the inheritance chain).
		/// Returns <c>null</c> when no match is found.
		/// <para>
		/// For types with static field initializers but no explicit static constructor
		/// (<c>beforefieldinit</c>), the CLR may defer type initialization until a static
		/// member is first accessed. Because DP registration happens in those initializers,
		/// this method explicitly runs the class constructor for each type in the chain so
		/// that all DPs are registered before the lookup proceeds.
		/// </para>
		/// </summary>
		public static DependencyProperty? FindByName(string name, Type ownerType)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

			// Ensure each type in the chain has its static initializer (and DP registrations)
			// done before we search. This is a no-op for already-initialized types.
			Type? t = ownerType;
			while (t != null && t != typeof(object))
			{
				System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
				t = t.BaseType;
			}

			lock (_registryLock)
			{
				t = ownerType;
				while (t != null)
				{
					if (_registry.TryGetValue((name, t), out var dp)) return dp;
					t = t.BaseType;
				}
			}
			return null;
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
