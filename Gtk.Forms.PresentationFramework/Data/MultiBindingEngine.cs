using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Windows.Data
{
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
			}

			internal void Deactivate() => Detach();

			internal void Push(FrameworkElement target)
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
}
