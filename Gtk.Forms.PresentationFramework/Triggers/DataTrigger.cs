using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;

namespace System.Windows
{
	/// <summary>
	/// Data trigger: fires when a <see cref="Binding"/> expression evaluates to <see cref="Value"/>.
	/// Uses the existing <see cref="BindingEngine"/> path-resolution helpers.
	/// </summary>
	public class DataTrigger : TriggerBase
	{
		/// <summary>Binding that provides the value to compare.</summary>
		public Binding? Binding { get; set; }

		/// <summary>The value that activates this trigger.</summary>
		public object? Value { get; set; }

		/// <summary>Setters applied when the condition is active.</summary>
		public SetterBaseCollection Setters { get; } = new SetterBaseCollection();

		readonly Dictionary<FrameworkElement, DataTriggerState> _states =
			new Dictionary<FrameworkElement, DataTriggerState>();

		/// <inheritdoc/>
		internal override void Attach(FrameworkElement target)
		{
			if (Binding == null) return;

			// E5: nested path segments are not yet supported — explicit contract violation.
			var path = Binding.Path;
			if (!string.IsNullOrEmpty(path) && path!.Contains(".", StringComparison.Ordinal))
				throw new NotSupportedException(
					$"DataTrigger: nested binding paths ('{path}') are not yet supported. " +
					"Use a single-segment path.");

			var state = new DataTriggerState(this, target);
			_states[target] = state;
			state.Evaluate();
		}

		/// <inheritdoc/>
		internal override void Detach(FrameworkElement target)
		{
			if (!_states.TryGetValue(target, out var state)) return;
			_states.Remove(target);
			state.Dispose();
		}

		sealed class DataTriggerState
		{
			readonly DataTrigger _trigger;
			readonly FrameworkElement _target;
			// E1: use this DataTriggerState instance as the contributor key.
			List<DependencyProperty>? _appliedDps;

			INotifyPropertyChanged? _inpcSource;
			// E5: single-segment path only — multi-segment raises at attach time (see Attach).
			string? _watchedProp;
			PropertyChangedCallback? _dcHook;

			internal DataTriggerState(DataTrigger trigger, FrameworkElement target)
			{
				_trigger = trigger;
				_target = target;
				Subscribe();
			}

			void Subscribe()
			{
				// Watch DataContext changes so we can re-subscribe to new sources.
				_dcHook = OnDcChanged;
				_target.AddChangeHook(FrameworkElement.DataContextProperty, _dcHook);
				SubscribeSource(GetSource());
			}

			void OnDcChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				UnsubscribeSource();
				SubscribeSource(GetSource());
				Evaluate();
			}

			object? GetSource()
			{
				var b = _trigger.Binding;
				if (b == null) return null;
				if (b.Source != null) return b.Source;
				return _target.DataContext;
			}

			void SubscribeSource(object? source)
			{
				if (source is INotifyPropertyChanged inpc)
				{
					_inpcSource = inpc;
					var path = _trigger.Binding?.Path;
					// Single-segment path only (multi-segment validated at Attach time).
					_watchedProp = string.IsNullOrEmpty(path) ? null : path!.Split('.')[0];
					_inpcSource.PropertyChanged += OnSourceChanged;
				}
			}

			void UnsubscribeSource()
			{
				if (_inpcSource != null)
				{
					_inpcSource.PropertyChanged -= OnSourceChanged;
					_inpcSource = null;
					_watchedProp = null;
				}
			}

			void OnSourceChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (_watchedProp != null && e.PropertyName != null &&
					e.PropertyName != _watchedProp) return;
				Evaluate();
			}

			internal void Evaluate()
			{
				var source = GetSource();
				var current = BindingEngine.ResolvePath(source, _trigger.Binding?.Path);
				bool match = Equals(current, _trigger.Value);
				bool active = _appliedDps != null;
				if (match && !active) Apply();
				else if (!match && active) UnApply();
			}

			void Apply()
			{
				_appliedDps = new List<DependencyProperty>();
				foreach (var setter in _trigger.Setters)
					setter.ApplyToTriggerLayer(_target, this, _appliedDps);
			}

			void UnApply()
			{
				if (_appliedDps == null) return;
				var dps = _appliedDps;
				_appliedDps = null;
				_target.ClearTriggerValues(dps, this);
			}

			internal void Dispose()
			{
				if (_dcHook != null)
					_target.RemoveChangeHook(FrameworkElement.DataContextProperty, _dcHook);
				UnsubscribeSource();
				UnApply();
			}
		}
	}
}
