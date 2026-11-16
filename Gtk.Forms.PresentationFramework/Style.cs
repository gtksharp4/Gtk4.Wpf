using System;
using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// WPF-compatible style: <see cref="TargetType"/>, <see cref="Setters"/>,
	/// <see cref="BasedOn"/> chain, and implicit-style registration by type.
	/// </summary>
	public class Style
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
		/// to <paramref name="target"/>.
		/// </summary>
		public void Apply(DependencyObject target)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));

			// Build the chain: [root…, parent, this] order so outer styles apply first.
			var chain = new List<Style>();
			var s = this;
			while (s != null)
			{
				chain.Add(s);
				s = s.BasedOn;
			}
			// Apply outermost (most base) first; this style last (highest priority).
			for (int i = chain.Count - 1; i >= 0; i--)
				foreach (var setter in chain[i].Setters)
					setter.Apply(target);
		}
	}

	/// <summary>Collection of <see cref="SetterBase"/> items.</summary>
	public class SetterBaseCollection : List<SetterBase> { }

	/// <summary>Base class for all setter types.</summary>
	public abstract class SetterBase
	{
		/// <summary>Applies this setter to <paramref name="target"/>.</summary>
		internal abstract void Apply(DependencyObject target);
	}

	/// <summary>
	/// Sets a single <see cref="DependencyProperty"/> value as part of a <see cref="Style"/>.
	/// <para>
	/// Either <see cref="Property"/> (a resolved <see cref="DependencyProperty"/>) or
	/// <see cref="PropertyName"/> (a string resolved against <see cref="Style.TargetType"/>
	/// at apply time) may be set; <see cref="Property"/> takes precedence.
	/// </para>
	/// <para>
	/// <see cref="Value"/> is kept as an <c>object?</c>; typed value-converter resolution
	/// is a later phase (TODO: type-converter round).
	/// </para>
	/// </summary>
	public class Setter : SetterBase
	{
		/// <summary>The dependency property to set (takes precedence over <see cref="PropertyName"/>).</summary>
		public DependencyProperty? Property { get; set; }

		/// <summary>
		/// String property name, resolved against the owner type at apply time when
		/// <see cref="Property"/> is null.
		/// </summary>
		public string? PropertyName { get; set; }

		/// <summary>The value to assign.</summary>
		public object? Value { get; set; }

		/// <inheritdoc/>
		internal override void Apply(DependencyObject target)
		{
			var dp = Property;
			if (dp == null && PropertyName != null)
				dp = DependencyProperty.FindByName(PropertyName, target.GetType());
			if (dp != null)
				target.SetValue(dp, Value);
		}
	}
}
