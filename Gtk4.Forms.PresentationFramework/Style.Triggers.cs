namespace System.Windows;

public partial class Style
{
	/// <summary>Triggers that are part of this style.</summary>
	public TriggerCollection Triggers { get; } = new TriggerCollection();

	/// <summary>
	/// Attaches all triggers in this style (and its <see cref="BasedOn"/> chain) to
	/// <paramref name="target"/>. Called from <see cref="Apply"/>.
	/// </summary>
	internal void AttachTriggers(FrameworkElement target)
	{
		// Walk BasedOn chain outermost-first (same order as Apply).
		var chain = BuildChain();
		for (int i = chain.Count - 1; i >= 0; i--)
			foreach (var trigger in chain[i].Triggers)
				trigger.Attach(target);
	}

	/// <summary>
	/// Detaches all triggers in this style (and its <see cref="BasedOn"/> chain) from
	/// <paramref name="target"/>. Called when the style is replaced or nulled.
	/// </summary>
	internal void DetachTriggers(FrameworkElement target)
	{
		var chain = BuildChain();
		for (int i = chain.Count - 1; i >= 0; i--)
			foreach (var trigger in chain[i].Triggers)
				trigger.Detach(target);
	}

	System.Collections.Generic.List<Style> BuildChain()
	{
		var chain = new System.Collections.Generic.List<Style>();
		var s = this;
		while (s != null)
		{
			bool seen = false;
			for (int v = 0; v < chain.Count; v++)
				if (ReferenceEquals(chain[v], s)) { seen = true; break; }
			if (seen) break;
			chain.Add(s);
			s = s.BasedOn;
		}
		return chain;
	}
}
