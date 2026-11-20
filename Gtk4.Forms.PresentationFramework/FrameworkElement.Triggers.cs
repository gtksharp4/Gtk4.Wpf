using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	readonly TriggerCollection _triggers = new TriggerCollection();

	/// <summary>
	/// Element-level trigger collection. This implementation attaches all
	/// <see cref="TriggerBase"/> subtypes (not only <see cref="EventTrigger"/>), mirroring
	/// the runtime capability that <see cref="Style"/> triggers already provide.
	/// </summary>
	public TriggerCollection Triggers => _triggers;

	/// <summary>
	/// Attaches all triggers in <see cref="Triggers"/> to this element.
	/// WPF restricts element-level triggers to <see cref="EventTrigger"/> only, but this
	/// implementation supports all <see cref="TriggerBase"/> subtypes so that property
	/// triggers declared on an element (not in a Style) work at runtime.
	/// Call from <c>InitializeComponent</c> or after the element is added to the tree.
	/// Layer hook for generated code; real WPF has no such member, so it is hidden from IntelliSense.
	/// </summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void AttachElementTriggers()
	{
		foreach (var t in _triggers)
			t.Attach(this);
	}

	/// <summary>Detaches all element-level triggers. Call on unload/dispose.</summary>
	internal void DetachElementTriggers()
	{
		foreach (var t in _triggers)
			t.Detach(this);
	}
}