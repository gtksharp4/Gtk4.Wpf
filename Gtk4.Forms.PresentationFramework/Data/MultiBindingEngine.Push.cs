using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Data;

internal static partial class MultiBindingEngine
{
	internal sealed partial class MultiRecord
	{
		internal void Push(FrameworkElement target)
		{
			if (_pushing) return;
			_pushing = true;
			_dirty = false;
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

			BindingEngine.SetTargetSafe(target, _dp, result, _mb.FallbackValue);
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
