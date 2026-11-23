using System.Reflection;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		DependencyObject? _dpSource;
		DependencyProperty? _dpSourceProperty;
		PropertyChangedCallback? _dpSourceCallback;
		PathWatcher? _dpPathWatcher;

		/// <summary>
		/// Watches a <see cref="DependencyObject"/> source (e.g. <c>{Binding ActualWidth, ElementName=x}</c>),
		/// which is not INPC. A single-segment path naming one of its DPs is hooked directly. A longer path
		/// (<c>Tag.Rule</c>, <c>Items[0].Name</c>) is watched step by step through a <see cref="PathWatcher"/>,
		/// which re-subscribes the downstream steps when an upstream step changes.
		/// <para>
		/// <b>Gap vs WPF:</b> a single-segment path that does not name a dependency property is read once.
		/// </para>
		/// </summary>
		void AttachDpSource(object? source)
		{
			if (source is not DependencyObject fe) return;
			var path = PropertyPath.ResolveOwner(source, _binding.PathString);
			if (string.IsNullOrEmpty(path)) return;
			var single = path![0] == '(' && path.IndexOf(')') == path.Length - 1;
			if (!single && path.IndexOfAny(new[] { '.', '[', '(' }) >= 0)
			{
				AttachPathWatcher(fe, path);
				return;
			}
			var dp = single ? AttachedProperty(source, path) : FindDependencyProperty(fe.GetType(), path);
			if (dp == null) return;

			_dpSource = fe;
			_dpSourceProperty = dp;
			_dpSourceCallback = OnSourceDpChanged;
			fe.AddChangeHook(dp, _dpSourceCallback);
		}

		void AttachPathWatcher(DependencyObject source, string path)
		{
			_dpSource = source;
			_dpPathWatcher = new PathWatcher(source, path);
			_dpPathWatcher.Changed += () =>
			{
				if (_targetRef.TryGetTarget(out var target)) PushToTarget(target, source);
			};
		}

		void DetachDpSource()
		{
			if (_dpSource != null && _dpSourceProperty != null && _dpSourceCallback != null)
				_dpSource.RemoveChangeHook(_dpSourceProperty, _dpSourceCallback);
			_dpPathWatcher?.Dispose();
			_dpPathWatcher = null;
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
