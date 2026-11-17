using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	readonly TriggerCollection _triggers = new TriggerCollection();

	/// <summary>
	/// Element-level trigger collection. WPF restricts these to <see cref="EventTrigger"/>
	/// only; non-EventTrigger entries are ignored at attach time.
	/// </summary>
	public TriggerCollection Triggers => _triggers;

	/// <summary>
	/// Attaches all <see cref="EventTrigger"/> entries in <see cref="Triggers"/>.
	/// Call from <c>InitializeComponent</c> or after the element is added to the tree.
	/// </summary>
	internal void AttachElementTriggers()
	{
		foreach (var t in _triggers)
		{
			// WPF restriction: element-level triggers may only be EventTrigger.
			if (t is EventTrigger et)
				et.Attach(this);
		}
	}

	/// <summary>Detaches all element-level triggers. Call on unload/dispose.</summary>
	internal void DetachElementTriggers()
	{
		foreach (var t in _triggers)
		{
			if (t is EventTrigger et)
				et.Detach(this);
		}
	}
}