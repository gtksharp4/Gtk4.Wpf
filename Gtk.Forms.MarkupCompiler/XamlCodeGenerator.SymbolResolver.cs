using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Gtk.Forms.MarkupCompiler
{
	/// <summary>
	/// Reflects the referenced assemblies to determine whether a CLR property is a
	/// <c>DependencyProperty</c> (a public static field named <c>&lt;Prop&gt;Property</c>
	/// whose type name contains "DependencyProperty") or a plain CLR property.
	/// <para>
	/// When <paramref name="referencePaths"/> is empty the resolver immediately falls back
	/// to the name heuristic so existing unit tests (which supply no references) keep passing.
	/// </para>
	/// </summary>
	sealed class DpSymbolResolver : IDisposable
	{
		// (typeFullName, propName) → true = DP, false = CLR
		readonly ConcurrentDictionary<(string, string), bool> _cache =
			new ConcurrentDictionary<(string, string), bool>();

		readonly List<Assembly> _loaded = new List<Assembly>();
		readonly bool _hasReferences;

		internal DpSymbolResolver(IReadOnlyList<string> referencePaths)
		{
			if (referencePaths == null || referencePaths.Count == 0)
			{
				_hasReferences = false;
				return;
			}

			_hasReferences = true;
			foreach (string path in referencePaths)
			{
				if (!File.Exists(path)) continue;
				try
				{
					// Load via LoadFrom so the assembly resolves its own dependencies.
					Assembly asm = Assembly.LoadFrom(path);
					_loaded.Add(asm);
				}
				catch { /* skip unloadable (native, strong-name mismatch, etc.) */ }
			}
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

		bool ResolveIsDP(string typeFullName, string propName)
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

		/// <summary>No unmanaged resources; just clears the loaded-assembly list.</summary>
		public void Dispose() => _loaded.Clear();
	}
}
