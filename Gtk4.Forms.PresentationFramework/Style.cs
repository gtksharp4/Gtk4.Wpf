using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// WPF-compatible style: <see cref="TargetType"/>, <see cref="Setters"/>,
/// <see cref="BasedOn"/> chain, and implicit-style registration by type.
/// </summary>
public partial class Style
{
	/// <summary>The type this style targets.</summary>
	public Type? TargetType { get; set; }

	/// <summary>Optional base style — applied before this style's setters.</summary>
	public Style? BasedOn { get; set; }

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
