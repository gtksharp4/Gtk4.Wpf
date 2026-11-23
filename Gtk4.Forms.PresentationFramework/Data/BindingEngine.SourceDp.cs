using System.Reflection;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		DependencyObject? _dpSource;
		DependencyProperty? _dpSourceProperty;
		PropertyChangedCallback? _dpSourceCallback;

		/// <summary>
		/// Watches a dependency-property source (e.g. <c>{Binding ActualWidth, ElementName=x}</c>):
		/// when the source is a <see cref="FrameworkElement"/> (not INPC) and the path is a single
		/// segment naming one of its DPs, re-pushes the target whenever that DP changes — WPF
		/// listens to DP sources the same way it listens to INPC sources.
		/// <para>
		/// <b>Gap vs WPF:</b> only <see cref="FrameworkElement"/> sources with a single-segment
		/// path are watched (the hook reuses the FE-keyed <c>TargetChangeTracker</c>). A binding
		/// whose source is any other <see cref="DependencyObject"/> (a Freezable, a non-FE
		/// element, a DO held in DataContext), or whose DP sits behind a multi-segment path,
		/// reads the value once and does not update live; WPF's <c>PropertyPathWorker</c>
		/// listens on any DependencyObject source.
		/// </para>
		/// </summary>
		void AttachDpSource(object? source)
		{
			if (source is not DependencyObject fe) return;
			var path = PropertyPath.ResolveOwner(source, _binding.PathString);
			if (string.IsNullOrEmpty(path)) return;
			var dp = path![0] == '(' && path.IndexOf(')') == path.Length - 1
				? AttachedProperty(source, path)
				: path.IndexOfAny(new[] { '.', '[', '(' }) >= 0 ? null : FindDependencyProperty(fe.GetType(), path);
			if (dp == null) return;

			_dpSource = fe;
			_dpSourceProperty = dp;
			_dpSourceCallback = OnSourceDpChanged;
			fe.AddChangeHook(dp, _dpSourceCallback);
		}

		void DetachDpSource()
		{
			if (_dpSource != null && _dpSourceProperty != null && _dpSourceCallback != null)
				_dpSource.RemoveChangeHook(_dpSourceProperty, _dpSourceCallback);
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
