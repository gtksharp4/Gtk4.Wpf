using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Windows.Data
{
	/// <summary>
	/// Phase-0 one-way binding engine.
	/// <para>
	/// Sets a binding on a target DP, resolves the source (explicit <see cref="Binding.Source"/>
	/// or the element's <c>DataContext</c>), reads the value via reflection, pushes it into the
	/// target DP, and subscribes to <see cref="INotifyPropertyChanged"/> for future updates.
	/// </para>
	/// <para>
	/// Two-way and <see cref="BindingMode.OneWayToSource"/> are Phase-1 work;
	/// they fall back to one-way here.
	/// </para>
	/// </summary>
	internal static class BindingEngine
	{
		// Registered bindings per target element (ConditionalWeakTable lets the entry
		// be collected with the element).
		static readonly ConditionalWeakTable<FrameworkElement, List<BindingRecord>> _bindings =
			new ConditionalWeakTable<FrameworkElement, List<BindingRecord>>();

		/// <summary>
		/// Registers <paramref name="binding"/> on <paramref name="dp"/> of
		/// <paramref name="target"/> and applies it immediately.
		/// </summary>
		internal static void SetBinding(FrameworkElement target, DependencyProperty dp, BindingBase bindingBase)
		{
			// Remove any existing binding on the same DP.
			var list = _bindings.GetOrCreateValue(target);
			list.RemoveAll(r => r.TargetProperty == dp);

			if (bindingBase is Binding binding)
			{
				var record = new BindingRecord(target, dp, binding);
				list.Add(record);
				record.Activate();
			}
		}

		/// <summary>
		/// Re-evaluates all bindings whose source was the inherited DataContext.
		/// Called when DataContext changes.
		/// </summary>
		internal static void RefreshBindings(FrameworkElement target)
		{
			if (!_bindings.TryGetValue(target, out var list)) return;
			foreach (var record in list)
				record.Activate();
		}

		// ── Path resolution ──────────────────────────────────────────────────────

		/// <summary>
		/// Resolves a dot-separated path on <paramref name="source"/>.
		/// Returns <c>null</c> when any step is null or the property does not exist.
		/// </summary>
		internal static object? ResolvePath(object? source, string? path)
		{
			if (source == null) return null;
			if (string.IsNullOrEmpty(path)) return source;

			var current = source;
			foreach (var segment in path.Split('.'))
			{
				if (current == null) return null;
				var prop = current.GetType().GetProperty(
					segment,
					BindingFlags.Public | BindingFlags.Instance);
				if (prop == null) return null;
				current = prop.GetValue(current);
			}
			return current;
		}

		// ── Inner record ────────────────────────────────────────────────────────

		sealed class BindingRecord
		{
			readonly WeakReference<FrameworkElement> _targetRef;
			readonly DependencyProperty _dp;
			readonly Binding _binding;

			// The INotifyPropertyChanged source we are currently subscribed to.
			INotifyPropertyChanged? _inpcSource;

			// The first segment of the path (the property we listen on INPC for).
			string? _watchedProperty;

			internal DependencyProperty TargetProperty => _dp;

			internal BindingRecord(FrameworkElement target, DependencyProperty dp, Binding binding)
			{
				_targetRef = new WeakReference<FrameworkElement>(target);
				_dp = dp;
				_binding = binding;
			}

			/// <summary>Resolves the source and pushes the current value.</summary>
			internal void Activate()
			{
				if (!_targetRef.TryGetTarget(out var target)) return;

				// Detach from previous INPC source.
				if (_inpcSource != null)
				{
					_inpcSource.PropertyChanged -= OnPropertyChanged;
					_inpcSource = null;
					_watchedProperty = null;
				}

				var source = ResolveSource(target);
				Push(target, source);

				// Subscribe for future changes on the first path segment.
				if (source is INotifyPropertyChanged inpc)
				{
					_inpcSource = inpc;
					_inpcSource.PropertyChanged += OnPropertyChanged;
					_watchedProperty = string.IsNullOrEmpty(_binding.Path)
						? null
						: _binding.Path!.Split('.')[0];
				}
			}

			object? ResolveSource(FrameworkElement target)
			{
				if (_binding.Source != null) return _binding.Source;
				return target.DataContext;
			}

			void Push(FrameworkElement target, object? source)
			{
				var rawValue = ResolvePath(source, _binding.Path);

				object? value = rawValue;
				if (_binding.Converter != null)
					value = _binding.Converter.Convert(rawValue, _dp.PropertyType,
						_binding.ConverterParameter, CultureInfo.CurrentCulture);

				target.SetValue(_dp, value);
			}

			void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (!_targetRef.TryGetTarget(out var target)) return;

				// Only re-push when the changed property matches the first path segment.
				if (_watchedProperty != null &&
					e.PropertyName != null &&
					e.PropertyName != _watchedProperty) return;

				Push(target, sender);
			}
		}
	}
}
