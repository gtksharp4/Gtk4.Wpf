using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	// A XAML-attribute {DynamicResource} keeps Local precedence in WPF: the resolved value lives in the
	// local slot (so triggers do not override it), refreshed live. An explicit SetValue/ClearValue detaches.

	internal static readonly object LocalLayerContributor = new object();

	DependencyProperty? _applyingLocalDp;

	internal void SetLocalResourceReference(DependencyProperty dp, object key)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (key == null) throw new ArgumentNullException(nameof(key));
		_resRefs ??= new List<ResRef>();
		_resRefs.RemoveAll(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, LocalLayerContributor));
		_resRefs.Add(new ResRef(dp, key, LocalLayerContributor));
		SubscribeToResourceChain();
		ApplyResourceRef(dp, key, LocalLayerContributor);
	}

	bool HasLocalResourceRef(DependencyProperty dp) => _resRefs != null && _resRefs.Exists(
		r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, LocalLayerContributor));

	void ApplyLocalResourceRef(DependencyProperty dp, object key)
	{
		var previous = _applyingLocalDp;
		_applyingLocalDp = dp;
		try
		{
			if (TryFindResourceCore(key, out var value)) SetValue(dp, value);
			else ClearValue(dp);
		}
		finally
		{
			_applyingLocalDp = previous;
		}
	}

	internal override void OnExplicitLocalChange(DependencyProperty dp)
	{
		if (ReferenceEquals(_applyingLocalDp, dp) || _resRefs == null) return;
		int removed = _resRefs.RemoveAll(
			r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, LocalLayerContributor));
		if (removed > 0 && _resRefs.Count == 0)
			UnsubscribeFromResourceChain();
	}
}
