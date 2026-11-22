using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace System.Windows.Data;

/// <summary>
/// Path steps with current-item semantics: <c>/</c> moves to the current item of the default view of the
/// collection so far (<c>/</c>, <c>X/Y</c>, <c>/Y</c>), and a step that is not a property of a collection
/// resolves against that current item, as WPF does for master/detail bindings.
/// </summary>
internal static partial class BindingEngine
{
	const BindingFlags Members = BindingFlags.Public | BindingFlags.Instance;

	internal readonly struct PathStep
	{
		internal PathStep(string? name) => Name = name;

		/// <summary>The property name, or null for the current-item step <c>/</c>.</summary>
		internal string? Name { get; }
	}

	internal static List<PathStep> ParseSteps(string path)
	{
		var steps = new List<PathStep>();
		var name = new StringBuilder();
		void Flush()
		{
			if (name.Length > 0) steps.Add(new PathStep(name.ToString()));
			name.Clear();
		}
		foreach (char c in path)
		{
			if (c == '.') Flush();
			else if (c == '/')
			{
				Flush();
				steps.Add(new PathStep(null));
			}
			else name.Append(c);
		}
		Flush();
		return steps;
	}

	/// <summary>The name of the first property step, or null when the path starts at a current item.</summary>
	internal static string? FirstPropertyName(string? path)
	{
		if (string.IsNullOrEmpty(path)) return null;
		var steps = ParseSteps(path!);
		return steps.Count > 0 ? steps[0].Name : null;
	}

	/// <summary>Splits <paramref name="path"/> into the part before its last property and that property.</summary>
	internal static void SplitLeaf(string path, out string head, out string? leaf)
	{
		int i = path.LastIndexOfAny(new[] { '.', '/' });
		if (i < 0) { head = string.Empty; leaf = path; return; }
		head = path[i] == '/' ? path.Substring(0, i + 1) : path.Substring(0, i);
		leaf = i + 1 < path.Length ? path.Substring(i + 1) : null;
	}

	/// <summary>
	/// Walks <paramref name="path"/> from <paramref name="source"/>. Returns false when a step cannot be taken
	/// (null owner or missing property). The default views used are added to <paramref name="views"/>.
	/// </summary>
	internal static bool TryWalk(object? source, string? path, List<ICollectionView>? views, out object? value)
	{
		value = source;
		path = PropertyPath.ResolveOwner(source, path);
		if (string.IsNullOrEmpty(path)) return true;
		foreach (var step in ParseSteps(path!))
		{
			if (value == null) return false;
			if (step.Name == null) value = CurrentOf(value, views);
			else if (!TryGetMember(value, step.Name, views, out value)) return false;
		}
		return true;
	}

	static object? CurrentOf(object collection, List<ICollectionView>? views)
	{
		var view = CollectionViewSource.GetDefaultView(collection);
		if (view == null) return null;
		views?.Add(view);
		return view.CurrentItem;
	}

	/// <summary>Reads a property; one not found on a collection is looked up on its current item.</summary>
	static bool TryGetMember(object owner, string name, List<ICollectionView>? views, out object? value)
	{
		value = null;
		var prop = owner.GetType().GetProperty(name, Members);
		if (prop == null && owner is IEnumerable && owner is not string)
		{
			var item = CurrentOf(owner, views);
			if (item == null) return false;
			owner = item;
			prop = owner.GetType().GetProperty(name, Members);
		}
		if (prop == null) return false;
		if (!TryRead(prop, owner, out value)) return false;
		return true;
	}

	/// <summary>The write target for <paramref name="leaf"/>: a collection without it yields its current item.</summary>
	internal static object? LeafOwner(object owner, string leaf)
	{
		if (owner is IEnumerable && owner is not string && owner.GetType().GetProperty(leaf, Members) == null)
			return CurrentOf(owner, null);
		return owner;
	}
}
