using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Windows.Data;

/// <summary>
/// Manages <see cref="MultiBinding"/> instances on target <see cref="FrameworkElement"/>s.
/// Resolves each child <see cref="Binding"/>, subscribes to source INPC, and calls the
/// <see cref="IMultiValueConverter"/> to push a combined value to the target DP.
/// </summary>
internal static class MultiBindingEngine
{
	static readonly ConditionalWeakTable<FrameworkElement, List<MultiRecord>> _records =
		new ConditionalWeakTable<FrameworkElement, List<MultiRecord>>();

	/// <summary>
	/// Registers <paramref name="mb"/> on <paramref name="dp"/> of <paramref name="target"/>
	/// and returns the live expression object.
	/// </summary>
	internal static MultiBindingExpression SetBinding(
		FrameworkElement target,
		DependencyProperty dp,
		MultiBinding mb)
	{
		var list = _records.GetOrCreateValue(target);

		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0)
		{
			list[idx].Deactivate();
			list.RemoveAt(idx);
		}

		var expr = new MultiBindingExpression(target, dp, mb);
		var record = new MultiRecord(target, dp, mb, expr);
		list.Add(record);
		record.Activate();
		return expr;
	}

	/// <summary>
	/// Removes and deactivates any existing multi-binding record for <paramref name="dp"/>
	/// on <paramref name="target"/>. Called by <see cref="BindingEngine.SetBinding"/> before
	/// installing a simple <see cref="Binding"/> over a prior <see cref="MultiBinding"/>.
	/// </summary>
	internal static void ClearForDp(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		int idx = list.FindIndex(r => r.TargetProperty == dp);
		if (idx >= 0)
		{
			list[idx].Deactivate();
			list.RemoveAt(idx);
		}
	}

	/// <summary>Re-evaluates all multi-binding records on <paramref name="target"/> that use the DataContext.</summary>
	internal static void RefreshDataContextBindings(FrameworkElement target)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list)
			r.RefreshIfUsesDataContext(target);
	}

	/// <summary>Deactivates all multi-binding records on <paramref name="target"/>.</summary>
	internal static void ClearAll(FrameworkElement target)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list) r.Deactivate();
		list.Clear();
	}

	/// <summary>Re-evaluates the multi-binding for <paramref name="dp"/> on target.</summary>
	internal static void Refresh(FrameworkElement target, DependencyProperty dp)
	{
		if (!_records.TryGetValue(target, out var list)) return;
		foreach (var r in list)
		{
			if (ReferenceEquals(r.TargetProperty, dp))
				r.Push(target);
		}
	}

	// ── Inner record ────────────────────────────────────────────────────────

	internal sealed class MultiRecord
	{
		readonly WeakReference<FrameworkElement> _targetRef;
		readonly DependencyProperty _dp;
		readonly MultiBinding _mb;

		// Per-child INPC subscriptions.
		readonly List<(INotifyPropertyChanged inpc, PropertyChangedEventHandler handler)> _subs =
			new List<(INotifyPropertyChanged, PropertyChangedEventHandler)>();

		// TwoWay: callback subscribed to target DP changes.
		PropertyChangedCallback? _targetChangedCallback;
		bool _pushing;

		internal DependencyProperty TargetProperty => _dp;

		internal MultiRecord(
			FrameworkElement target,
			DependencyProperty dp,
			MultiBinding mb,
			MultiBindingExpression expr)
		{
			_targetRef = new WeakReference<FrameworkElement>(target);
			_dp = dp;
			_mb = mb;
		}

		internal void Activate()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			Detach();
			SubscribeSources(target);
			Push(target);

			// TwoWay: subscribe to target DP changes and write-back via ConvertBack.
			if (_mb.Mode == BindingMode.TwoWay)
			{
				_targetChangedCallback = OnTargetPropertyChanged;
				TargetChangeTracker.Subscribe(target, _dp, _targetChangedCallback);
			}
		}

		internal void Deactivate()
		{
			if (_targetChangedCallback != null && _targetRef.TryGetTarget(out var t))
				TargetChangeTracker.Unsubscribe(t, _dp, _targetChangedCallback);
			_targetChangedCallback = null;
			Detach();
		}

		/// <summary>
		/// Re-subscribes sources and re-pushes if any child binding uses DataContext.
		/// Called when the target's DataContext changes (R3).
		/// </summary>
		internal void RefreshIfUsesDataContext(FrameworkElement target)
		{
			bool any = false;
			foreach (var b in _mb.Bindings)
			{
				if (b.Source == null && b.ElementName == null && b.RelativeSource == null)
				{ any = true; break; }
			}
			if (!any) return;
			Detach();
			SubscribeSources(target);
			Push(target);
		}

		internal void Push(FrameworkElement target)
		{
			if (_pushing) return;
			_pushing = true;
			try { PushCore(target); }
			finally { _pushing = false; }
		}

		void PushCore(FrameworkElement target)
		{
			var values = new object?[_mb.Bindings.Count];
			bool anyUnresolved = false;

			for (int i = 0; i < _mb.Bindings.Count; i++)
			{
				var b = _mb.Bindings[i];
				var src = ResolveChildSource(target, b);
				if (src == null && b.Source == null && b.ElementName == null && b.RelativeSource == null)
				{
					// DataContext not set yet — treat as unresolved.
					anyUnresolved = true;
				}
				values[i] = BindingEngine.ResolvePath(src, b.Path);
			}

			object? result;
			if (anyUnresolved)
			{
				result = _mb.FallbackValue == DependencyProperty.UnsetValue
					? null
					: _mb.FallbackValue;
			}
			else if (_mb.Converter != null)
			{
				result = _mb.Converter.Convert(values, _dp.PropertyType,
					_mb.ConverterParameter, CultureInfo.CurrentCulture);
			}
			else
			{
				// No converter — join as string.
				result = string.Join(string.Empty, values);
			}

			// Apply TargetNullValue.
			if (result == null && _mb.TargetNullValue != null)
				result = _mb.TargetNullValue;

			// Apply StringFormat.
			result = ApplyStringFormat(_mb.StringFormat, result);

			target.SetValue(_dp, result);
		}

		void SubscribeSources(FrameworkElement target)
		{
			foreach (var b in _mb.Bindings)
			{
				var src = ResolveChildSource(target, b);
				if (src is INotifyPropertyChanged inpc)
				{
					PropertyChangedEventHandler handler = (s, e) =>
					{
						if (_targetRef.TryGetTarget(out var t)) Push(t);
					};
					inpc.PropertyChanged += handler;
					_subs.Add((inpc, handler));
				}
			}
		}

		void Detach()
		{
			foreach (var (inpc, handler) in _subs)
				inpc.PropertyChanged -= handler;
			_subs.Clear();
		}

		/// <summary>
		/// TwoWay write-back: target DP changed → call ConvertBack → write each child source.
		/// </summary>
		void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (_pushing) return;
			if (!_targetRef.TryGetTarget(out var target)) return;
			if (_mb.Converter == null) return;

			var targetValue = target.GetValue(_dp);
			var sourceTypes = new Type[_mb.Bindings.Count];
			for (int i = 0; i < _mb.Bindings.Count; i++)
				sourceTypes[i] = typeof(object);

			var backValues = _mb.Converter.ConvertBack(
				targetValue, sourceTypes, _mb.ConverterParameter, CultureInfo.CurrentCulture);

			if (backValues == null) return;

			_pushing = true;
			try
			{
				for (int i = 0; i < _mb.Bindings.Count && i < backValues.Length; i++)
				{
					var bv = backValues[i];
					if (ReferenceEquals(bv, Binding.DoNothing)) continue;
					if (ReferenceEquals(bv, DependencyProperty.UnsetValue)) continue;

					var b = _mb.Bindings[i];
					var src = ResolveChildSource(target, b);
					if (src == null) continue;
					BindingEngine.WriteBackPath(src, b.Path, bv);
				}
			}
			finally
			{
				_pushing = false;
			}
		}

		static object? ResolveChildSource(FrameworkElement target, Binding b)
		{
			if (b.Source != null) return b.Source;
			if (b.ElementName != null) return target.FindName(b.ElementName) as FrameworkElement;
			if (b.RelativeSource != null) return b.RelativeSource.Resolve(target);
			return target.DataContext;
		}

		static object? ApplyStringFormat(string? fmt, object? value)
		{
			if (fmt == null) return value;
			string s = fmt.Contains("{0}") ? fmt : fmt + "{0}";
			return string.Format(CultureInfo.InvariantCulture, s, value);
		}
	}
}