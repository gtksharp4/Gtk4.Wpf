using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// WPF-compatible style: <see cref="TargetType"/>, <see cref="Setters"/>,
/// <see cref="BasedOn"/> chain, and implicit-style registration by type.
/// </summary>
public partial class Style
{
	Type? _targetType;
	Style? _basedOn;

	/// <summary>The type this style targets.</summary>
	/// <exception cref="InvalidOperationException">The style is sealed.</exception>
	public Type? TargetType
	{
		get => _targetType;
		set { CheckSealed(); _targetType = value; }
	}

	/// <summary>Optional base style — applied before this style's setters.</summary>
	/// <exception cref="InvalidOperationException">The style is sealed.</exception>
	public Style? BasedOn
	{
		get => _basedOn;
		set { CheckSealed(); _basedOn = value; }
	}

	/// <summary>Whether the style is sealed and can no longer be modified.</summary>
	public bool IsSealed { get; private set; }

	/// <summary>Seals the style (and its <see cref="BasedOn"/> chain) against further modification.</summary>
	public void Seal()
	{
		if (IsSealed) return;
		IsSealed = true;
		Setters.IsSealed = true;
		Triggers.IsSealed = true;
		_basedOn?.Seal();
	}

	void CheckSealed()
	{
		if (IsSealed) throw new InvalidOperationException("Cannot modify a sealed Style.");
	}

	/// <summary>Property setters that compose this style.</summary>
	public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

	/// <summary>Creates an empty style.</summary>
	public Style() { }

	/// <summary>Creates a style for <paramref name="targetType"/>.</summary>
	public Style(Type targetType) => TargetType = targetType;

	/// <summary>
	/// Applies the full BasedOn chain (outermost first) then this style's setters
	/// to <paramref name="target"/> using the style-value layer (D6).
	/// D7: a cycle guard prevents infinite loops in <see cref="BasedOn"/> chains.
	/// </summary>
	public void Apply(DependencyObject target)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));

		// Build the chain: [root…, parent, this] order so outer styles apply first.
		// D7: guard against BasedOn cycles using a visited list (reference equality).
		var chain = new List<Style>();
		var s = this;
		while (s != null)
		{
			// Cycle detection via reference scan of already-visited styles.
			bool seen = false;
			for (int v = 0; v < chain.Count; v++)
				if (ReferenceEquals(chain[v], s)) { seen = true; break; }
			if (seen) break;
			chain.Add(s);
			s = s.BasedOn;
		}
		// Apply outermost (most base) first; this style last (highest priority).
		for (int i = chain.Count - 1; i >= 0; i--)
			foreach (var setter in chain[i].Setters)
				setter.ApplyToStyleLayer(target);
	}
}
