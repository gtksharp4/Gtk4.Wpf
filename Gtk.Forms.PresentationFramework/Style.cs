using System;
using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Phase-0 stub. Stores a <see cref="TargetType"/> and a list of <see cref="Setter"/>s.
	/// Application to elements (implicit styles, triggers) is Phase-1 work.
	/// </summary>
	public class Style
	{
		/// <summary>The type this style targets.</summary>
		public Type? TargetType { get; set; }

		/// <summary>Optional base style.</summary>
		public Style? BasedOn { get; set; }

		/// <summary>Property setters that compose this style.</summary>
		public IList<Setter> Setters { get; } = new List<Setter>();

		/// <summary>Creates an empty style.</summary>
		public Style() { }

		/// <summary>Creates a style for <paramref name="targetType"/>.</summary>
		public Style(Type targetType) => TargetType = targetType;

		/// <summary>
		/// Applies the setters in this style to <paramref name="target"/>.
		/// Phase-0: only direct <see cref="DependencyProperty"/> setters are applied;
		/// triggers and BasedOn are Phase-1.
		/// </summary>
		public void Apply(DependencyObject target)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));
			foreach (var setter in Setters)
				setter.Apply(target);
		}
	}

	/// <summary>
	/// Sets a single <see cref="DependencyProperty"/> value as part of a <see cref="Style"/>.
	/// </summary>
	public class Setter
	{
		/// <summary>The dependency property to set.</summary>
		public DependencyProperty? Property { get; set; }

		/// <summary>The value to assign.</summary>
		public object? Value { get; set; }

		/// <summary>Applies this setter to <paramref name="target"/>.</summary>
		internal void Apply(DependencyObject target)
		{
			if (Property != null)
				target.SetValue(Property, Value);
		}
	}
}
