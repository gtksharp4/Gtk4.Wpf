using System;
using System.Collections.Generic;

namespace System.Windows
{
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

	/// <summary>Collection of <see cref="SetterBase"/> items.</summary>
	public class SetterBaseCollection : List<SetterBase> { }

	/// <summary>Base class for all setter types.</summary>
	public abstract class SetterBase
	{
		/// <summary>Applies this setter to <paramref name="target"/> as a local value (direct apply).</summary>
		internal abstract void Apply(DependencyObject target);

		/// <summary>Applies this setter into the style-value layer of <paramref name="target"/> (D6).</summary>
		internal abstract void ApplyToStyleLayer(DependencyObject target);

		/// <summary>
		/// D8: Applies this setter into the trigger-value layer of <paramref name="target"/>,
		/// keyed by <paramref name="contributor"/> (the trigger-state instance).
		/// Appends the resolved DP to <paramref name="appliedDps"/> for un-apply bookkeeping.
		/// </summary>
		internal abstract void ApplyToTriggerLayer(
			DependencyObject target,
			object contributor,
			System.Collections.Generic.List<DependencyProperty> appliedDps);
	}

	/// <summary>
	/// Sets a single <see cref="DependencyProperty"/> value as part of a <see cref="Style"/>.
	/// <para>
	/// Either <see cref="Property"/> (a resolved <see cref="DependencyProperty"/>) or
	/// <see cref="PropertyName"/> (a string resolved against <see cref="Style.TargetType"/>
	/// at apply time) may be set; <see cref="Property"/> takes precedence.
	/// </para>
	/// <para>
	/// When <see cref="Value"/> is a <c>string</c> and the target DP has a non-string
	/// <c>PropertyType</c>, the value is converted via
	/// <see cref="System.Windows.Markup.XamlValueConverter.Convert"/> before being applied.
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

		/// <summary>The value to assign. A string is type-converted on apply.</summary>
		public object? Value { get; set; }

		/// <summary>
		/// Resolves <paramref name="rawValue"/> to the target type of <paramref name="dp"/>.
		/// Returns <paramref name="rawValue"/> unchanged when it is not a string or the DP
		/// type is <c>string</c>.
		/// </summary>
		static object? CoerceValue(DependencyProperty dp, object? rawValue)
		{
			if (rawValue is string s && dp.PropertyType != typeof(string))
				return System.Windows.Markup.XamlValueConverter.Convert(dp.OwnerType, dp, s);
			return rawValue;
		}

		/// <inheritdoc/>
		internal override void Apply(DependencyObject target)
		{
			var dp = Property;
			if (dp == null && PropertyName != null)
				dp = DependencyProperty.FindByName(PropertyName, target.GetType());
			if (dp != null)
				target.SetValue(dp, CoerceValue(dp, Value));
		}

		/// <inheritdoc/>
		internal override void ApplyToStyleLayer(DependencyObject target)
		{
			var dp = Property;
			if (dp == null && PropertyName != null)
				dp = DependencyProperty.FindByName(PropertyName, target.GetType());
			if (dp != null)
				target.SetStyleValue(dp, CoerceValue(dp, Value));
		}

		/// <inheritdoc/>
		internal override void ApplyToTriggerLayer(
			DependencyObject target,
			object contributor,
			System.Collections.Generic.List<DependencyProperty> appliedDps)
		{
			var dp = Property;
			if (dp == null && PropertyName != null)
				dp = DependencyProperty.FindByName(PropertyName, target.GetType());
			if (dp == null) return;
			appliedDps.Add(dp);
			target.SetTriggerValue(dp, contributor, CoerceValue(dp, Value));
		}
	}
}
