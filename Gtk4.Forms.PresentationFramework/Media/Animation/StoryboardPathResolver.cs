using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Media.Animation;

/// <summary>Walks a <see cref="PropertyPath"/> from a target to the property a timeline animates.</summary>
internal static class StoryboardPathResolver
{
	static readonly Dictionary<string, Type?> _typeCache = new Dictionary<string, Type?>();

	/// <summary>
	/// Resolves <paramref name="path"/> from <paramref name="start"/>. Frozen intermediate values are cloned
	/// and the clone is set back on its parent, so the animation never touches a frozen object.
	/// </summary>
	internal static (DependencyObject Target, DependencyProperty Property) Resolve(
		DependencyObject start, PropertyPath path)
	{
		var steps = PropertyPathParser.Parse(path);
		object current = start;
		for (var i = 0; i < steps.Count; i++)
		{
			var step = steps[i];
			var last = i == steps.Count - 1;
			if (step.Index >= 0)
			{
				if (last || !(current is IList list) || step.Index >= list.Count)
					throw PropertyPathParser.Bad(path, $"'[{step.Index}]' is not a valid step here");
				current = Unfreeze(list[step.Index], v => list[step.Index] = v, path);
				continue;
			}
			if (!(current is DependencyObject obj))
			{
				var message = PropertyPathParser.Bad(
					path, $"'{step.Name}' cannot be read from a {current?.GetType().Name ?? "null"}").Message;
				if (current != null && IsKnownLayerGap(current.GetType()))
					throw new UnanimatableTargetException(message);
				throw PropertyPathParser.Bad(
					path, $"'{step.Name}' cannot be read from a {current?.GetType().Name ?? "null"}");
			}
			var dp = step.Parameter ?? FindProperty(step, obj.GetType());
			if (dp == null && !last && IsOwnerQualifier(step, obj.GetType()))
			{
				steps[i + 1].Owner = step.Name;
				continue;
			}
			if (last)
			{
				if (dp == null)
					throw PropertyPathParser.Bad(
						path, $"'{step.Name}' is not a dependency property of {obj.GetType().Name}");
				return (obj, dp);
			}
			var value = dp != null ? obj.GetValue(dp) : ReadClr(obj, step.Name);
			if (value == null)
				throw PropertyPathParser.Bad(path, $"'{step.Name}' of {obj.GetType().Name} is null");
			current = dp != null ? Unfreeze(value, v => obj.SetValue(dp, v), path) : value;
		}
		throw PropertyPathParser.Bad(path, "the path does not end in a property");
	}

	// WPF DependencyObjects that are plain CLR classes in this layer: animating through them is skipped,
	// not an error. Anything else that is not a DependencyObject is a path WPF itself rejects.
	static readonly Type[] _knownLayerGaps =
		{ typeof(GradientStop), typeof(PathFigure), typeof(PathSegment), typeof(Pen) };

	static bool IsKnownLayerGap(Type type)
	{
		foreach (var gap in _knownLayerGaps)
			if (gap.IsAssignableFrom(type)) return true;
		return false;
	}

	static object Unfreeze(object? value, Action<object> setBack, PropertyPath path)
	{
		if (value == null) throw PropertyPathParser.Bad(path, "an intermediate value is null");
		if (value is Freezable { IsFrozen: true } frozen)
		{
			var clone = frozen.Clone();
			setBack(clone);
			return clone;
		}
		return value;
	}

	// "UIElement.Opacity" written without parentheses: the first segment names the owner type.
	static bool IsOwnerQualifier(PathStep step, Type targetType) =>
		step.Name != null && step.Owner == null && targetType.GetProperty(step.Name) == null
		&& FindType(step.Name, targetType) != null;

	static object? ReadClr(object obj, string? name)
	{
		var info = name == null ? null : obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
		return info?.GetValue(obj);
	}

	static DependencyProperty? FindProperty(PathStep step, Type targetType)
	{
		if (step.Name == null) return null;
		if (step.Owner != null)
		{
			var owner = FindType(step.Owner, targetType);
			var dp = owner == null ? null : DependencyProperty.FindByName(step.Name, owner);
			if (dp != null && (dp.IsAttached || owner!.IsAssignableFrom(targetType))) return dp;
		}
		return DependencyProperty.FindByName(step.Name, targetType);
	}

	static Type? FindType(string name, Type targetType)
	{
		for (var t = targetType; t != null; t = t.BaseType)
			if (t.Name == name) return t;
		lock (_typeCache)
		{
			if (_typeCache.TryGetValue(name, out var cached)) return cached;
			Type? found = null;
			foreach (var assembly in new[] { typeof(UIElement).Assembly, typeof(DependencyObject).Assembly })
				foreach (var t in assembly.GetTypes())
					if (t.Name == name && typeof(DependencyObject).IsAssignableFrom(t)) { found = t; break; }
			return _typeCache[name] = found;
		}
	}
}
