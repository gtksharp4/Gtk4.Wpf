using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		List<ICollectionView> _views = new List<ICollectionView>();
		bool _watchViews;

		// Re-evaluates when the current item of a collection on the path moves (master/detail).
		void RefreshCurrentViews(object? source)
		{
			if (!_watchViews || source == null || string.IsNullOrEmpty(_binding.PathString)) return;
			var views = new List<ICollectionView>();
			TryWalk(source, _binding.PathString, views, out _);
			if (SameViews(views)) return;
			DetachViews();
			_views = views;
			foreach (var view in _views) view.CurrentChanged += OnViewCurrentChanged;
		}

		bool SameViews(List<ICollectionView> views)
		{
			if (views.Count != _views.Count) return false;
			for (int i = 0; i < views.Count; i++)
				if (!ReferenceEquals(views[i], _views[i])) return false;
			return true;
		}

		void DetachViews()
		{
			foreach (var view in _views) view.CurrentChanged -= OnViewCurrentChanged;
			_views.Clear();
		}

		void OnViewCurrentChanged(object? sender, EventArgs e)
		{
			if (_targetRef.TryGetTarget(out var target)) PushToTarget(target, ResolveSource(target));
		}
	}
}
