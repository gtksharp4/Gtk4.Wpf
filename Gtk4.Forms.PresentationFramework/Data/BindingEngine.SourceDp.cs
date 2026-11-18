using System.Reflection;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		FrameworkElement? _dpSource;
		DependencyProperty? _dpSourceProperty;
		PropertyChangedCallback? _dpSourceCallback;

		/// <summary>
		/// Watches a dependency-property source (e.g. <c>{Binding ActualWidth, ElementName=x}</c>):
		/// when the source is a <see cref="FrameworkElement"/> (not INPC) and the path is a single
		/// segment naming one of its DPs, re-pushes the target whenever that DP changes — WPF
		/// listens to DP sources the same way it listens to INPC sources.
		/// </summary>
		void AttachDpSource(object? source)
		{
			if (source is not FrameworkElement fe) return;
			var path = _binding.Path;
			if (string.IsNullOrEmpty(path) || path!.IndexOfAny(new[] { '.', '[', '(' }) >= 0) return;
			var dp = FindDependencyProperty(fe.GetType(), path);
			if (dp == null) return;

			_dpSource = fe;
			_dpSourceProperty = dp;
			_dpSourceCallback = OnSourceDpChanged;
			TargetChangeTracker.Subscribe(fe, dp, _dpSourceCallback);
		}

		void DetachDpSource()
		{
			if (_dpSource != null && _dpSourceProperty != null && _dpSourceCallback != null)
				TargetChangeTracker.Unsubscribe(_dpSource, _dpSourceProperty, _dpSourceCallback);
			_dpSource = null;
			_dpSourceProperty = null;
			_dpSourceCallback = null;
		}

		void OnSourceDpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (_targetRef.TryGetTarget(out var target))
				PushToTarget(target, d);
		}

		static DependencyProperty? FindDependencyProperty(Type type, string name)
		{
			var field = type.GetField(
				name + "Property",
				BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			return field?.GetValue(null) as DependencyProperty;
		}
	}
}
