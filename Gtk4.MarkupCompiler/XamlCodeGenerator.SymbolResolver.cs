using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// Reflects the referenced assemblies to determine whether a CLR property is a
	/// <c>DependencyProperty</c> (a public static field named <c>&lt;Prop&gt;Property</c>
	/// whose type name contains "DependencyProperty") or a plain CLR property.
	/// <para>
	/// Assemblies are loaded via <c>Assembly.Load(File.ReadAllBytes(path))</c> so that
	/// the files are read and immediately released — no file locks are held after the
	/// constructor returns.
	/// </para>
	/// <para>
	/// When <paramref name="referencePaths"/> is empty the resolver immediately falls back
	/// to the name heuristic so existing unit tests (which supply no references) keep passing.
	/// </para>
	/// </summary>
	sealed partial class DpSymbolResolver : IDisposable
	{
		// (typeFullName, propName) → true = DP, false = CLR
		readonly ConcurrentDictionary<(string, string), bool> _cache =
			new ConcurrentDictionary<(string, string), bool>();

		readonly List<Assembly> _loaded = new List<Assembly>();
		readonly bool _hasReferences;

		/// <summary>
		/// <c>true</c> when at least one reference assembly path was supplied to the constructor
		/// (regardless of how many paths were successfully loaded).
		/// </summary>
		internal bool HasReferences => _hasReferences;

		// simple-name → Assembly lookup for the AssemblyResolve handler.
		readonly Dictionary<string, Assembly> _byName =
			new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

		ResolveEventHandler _resolveHandler;

		internal DpSymbolResolver(IReadOnlyList<string> referencePaths)
		{
			if (referencePaths == null || referencePaths.Count == 0)
			{
				_hasReferences = false;
				return;
			}

			_hasReferences = true;

			// Load ALL reference paths up-front with ReadAllBytes so that the
			// source files are closed immediately (no lock is held after the ctor).
			foreach (string path in referencePaths)
			{
				if (!File.Exists(path)) continue;
				try
				{
					byte[] bytes = File.ReadAllBytes(path);
					Assembly asm = Assembly.Load(bytes);
					_loaded.Add(asm);
					// Register under the simple name so the resolve handler can serve it.
					string simpleName = asm.GetName().Name ?? string.Empty;
					if (!_byName.ContainsKey(simpleName))
						_byName[simpleName] = asm;
				}
				catch { /* skip unloadable (native, strong-name mismatch, etc.) */ }
			}

			// AppDomain.CurrentDomain.AssemblyResolve is used (not AssemblyLoadContext.Default.Resolving)
			// because the task targets netstandard2.0, where AssemblyLoadContext is not available on all
			// host runtimes (older MSBuild nodes). AppDomain.AssemblyResolve is universally available.
			_resolveHandler = OnAssemblyResolve;
			AppDomain.CurrentDomain.AssemblyResolve += _resolveHandler;
		}

		Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			// args.Name is the full AssemblyName string; extract the simple name.
			string fullName = args.Name ?? string.Empty;
			int comma = fullName.IndexOf(',');
			string simpleName = comma >= 0 ? fullName.Substring(0, comma).Trim() : fullName.Trim();
			Assembly result;
			_byName.TryGetValue(simpleName, out result);
			return result;
		}

		/// <summary>
		/// False only for a resolvable type that does not derive from DependencyObject (Run, SolidColorBrush):
		/// it has DP fields but no SetValue, so its attributes are plain CLR assignments.
		/// </summary>
		internal bool IsDependencyObject(string typeFullName)
		{
			if (!_hasReferences) return true;
			try
			{
				for (Type t = FindType(typeFullName); t != null; t = t.BaseType)
				{
					if (t.FullName == "System.Windows.DependencyObject") return true;
				}
				return FindType(typeFullName) == null;
			}
			catch { return true; }
		}


		/// <summary>
		/// Returns <c>true</c> when <paramref name="typeFullName"/>.<paramref name="propName"/>
		/// is backed by a <c>DependencyProperty</c> field (checked on the declaring type and all
		/// its base types).  Falls back to the name heuristic when no references were supplied.
		/// </summary>
		internal bool IsDP(string typeFullName, string propName)
		{
			if (!_hasReferences)
				return FallbackHeuristic(typeFullName, propName);

			return _cache.GetOrAdd((typeFullName, propName), key =>
				ResolveIsDP(key.Item1, key.Item2));
		}

		/// <summary>
		/// Returns the CLR <see cref="Type"/> of <paramref name="propName"/> on
		/// <paramref name="typeFullName"/>, or <c>null</c> when the type cannot be found
		/// or the property does not exist.
		/// </summary>
		internal Type GetPropertyType(string typeFullName, string propName)
		{
			if (!_hasReferences) return null;
			try
			{
				Type owner = FindType(typeFullName);
				if (owner == null) return null;
				// Walk up the hierarchy in case the property is declared on a base type.
				Type current = owner;
				while (current != null)
				{
					PropertyInfo pi = current.GetProperty(
						propName,
						BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (pi != null) return pi.PropertyType;
					try { current = current.BaseType; }
					catch { break; }
				}
				return null;
			}
			catch { return null; }
		}

		// (typeFullName, memberName) → true = CLR event.
		readonly ConcurrentDictionary<(string, string), bool> _eventCache =
			new ConcurrentDictionary<(string, string), bool>();

		/// <summary>
		/// Returns <c>true</c> when <paramref name="typeFullName"/>.<paramref name="memberName"/>
		/// is a public CLR event (checked on the declaring type and all its base types), so a XAML
		/// <c>Click="Handler"</c> attribute compiles to <c>+= Handler</c> rather than an assignment.
		/// Falls back to a common-event-name heuristic when no references were supplied.
		/// </summary>
		internal bool IsEvent(string typeFullName, string memberName)
		{
			if (!_hasReferences)
				return FallbackEventHeuristic(memberName);

			return _eventCache.GetOrAdd((typeFullName, memberName), key =>
				ResolveIsEvent(key.Item1, key.Item2));
		}

		bool ResolveIsEvent(string typeFullName, string memberName)
		{
			try
			{
				Type type = FindType(typeFullName);
				if (type == null)
					return FallbackEventHeuristic(memberName);

				Type current = type;
				while (current != null)
				{
					EventInfo ev = current.GetEvent(
						memberName,
						BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (ev != null) return true;

					try { current = current.BaseType; }
					catch { break; }
				}
				return false;
			}
			catch
			{
				return FallbackEventHeuristic(memberName);
			}
		}

		bool ResolveIsDP(string typeFullName, string propName)
		{
			try
			{
				string fieldName = propName + "Property";

				Type type = FindType(typeFullName);
				if (type == null)
					return FallbackHeuristic(typeFullName, propName);

				// Walk the inheritance chain (handles inherited DPs declared on a base type).
				Type current = type;
				while (current != null)
				{
					FieldInfo field = current.GetField(
						fieldName,
						BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

					if (field != null && IsDependencyPropertyType(field.FieldType))
						return true;

					try { current = current.BaseType; }
					catch { break; }
				}

				// Not a DP field → CLR property.
				return false;
			}
			catch
			{
				// Never let a load/resolution failure abort the compile — fall back.
				return FallbackHeuristic(typeFullName, propName);
			}
		}

		/// <summary>
		/// Returns <c>true</c> when <paramref name="typeFullName"/> can be found in any of the
		/// loaded assemblies.  Used by the xmlns-definition resolver to disambiguate when the
		/// same local name might exist in several CLR namespaces.
		/// Always returns <c>false</c> when no reference assemblies were supplied.
		/// </summary>
		internal bool TypeExists(string typeFullName)
		{
			if (!_hasReferences) return false;
			try { return FindType(typeFullName) != null; }
			catch { return false; }
		}

		/// <summary>
		/// Finds <paramref name="typeFullName"/> in any of the loaded assemblies.
		/// </summary>
		Type FindType(string typeFullName)
		{
			foreach (Assembly asm in _loaded)
			{
				try
				{
					Type t = asm.GetType(typeFullName, throwOnError: false, ignoreCase: false);
					if (t != null) return t;
				}
				catch { }
			}
			return null;
		}

		static bool IsDependencyPropertyType(Type t) =>
			t != null && t.FullName != null &&
			t.FullName.Contains("DependencyProperty");

		// ── Fallback (name heuristic — same as the previous IsKnownDp) ──────────

		static readonly HashSet<string> _clrOnlyNames =
			new HashSet<string>(StringComparer.Ordinal)
			{
				"Title", "StartupUri", "Name",
				"Text", "Header", "ToolTip",
				"Content", "Tag", "DataContext", "CommandParameter",
			};

		internal static bool FallbackHeuristic(string fqType, string propName)
		{
			if (_clrOnlyNames.Contains(propName)) return false;
			return fqType != null &&
				fqType.StartsWith("System.Windows", StringComparison.Ordinal);
		}

		/// <summary>
		/// Removes the <see cref="AppDomain.AssemblyResolve"/> handler and clears the
		/// loaded-assembly list. No unmanaged resources are held.
		/// </summary>
		public void Dispose()
		{
			if (_resolveHandler != null)
			{
				AppDomain.CurrentDomain.AssemblyResolve -= _resolveHandler;
				_resolveHandler = null;
			}
			_loaded.Clear();
			_byName.Clear();
		}
	}
}
