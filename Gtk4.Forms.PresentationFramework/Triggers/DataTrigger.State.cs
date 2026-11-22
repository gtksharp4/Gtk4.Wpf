using System.Collections.Generic;
using System.Windows.Data;

namespace System.Windows;

public partial class DataTrigger
{
	/// <summary>
	/// Per-element live state for a <see cref="DataTrigger"/>.
	/// Subscribes to DataContext changes and uses <see cref="PathWatcher"/> to
	/// track every segment of a dot-separated binding path, re-evaluating the
	/// trigger condition whenever any segment raises PropertyChanged.
	/// </summary>
	sealed class DataTriggerState
	{
		readonly DataTrigger _trigger;
		readonly FrameworkElement _target;
		List<DependencyProperty>? _appliedDps;

		PathWatcher? _pathWatcher;
		PropertyChangedCallback? _dcHook;

		internal DataTriggerState(DataTrigger trigger, FrameworkElement target)
		{
			_trigger = trigger;
			_target = target;
			Subscribe();
		}

		void Subscribe()
		{
			_dcHook = OnDcChanged;
			_target.AddChangeHook(FrameworkElement.DataContextProperty, _dcHook);
			SubscribeSource(GetRoot());
		}

		void OnDcChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DisposePathWatcher();
			SubscribeSource(GetRoot());
			Evaluate();
		}

		object? GetRoot()
		{
			var b = _trigger.Binding;
			if (b == null) return null;
			if (b.Source != null) return b.Source;
			return _target.DataContext;
		}

		void SubscribeSource(object? root)
		{
			DisposePathWatcher();
			var path = PropertyPath.ResolveOwner(root, _trigger.Binding?.PathString);
			_pathWatcher = new PathWatcher(root, path);
			_pathWatcher.Changed += OnPathChanged;
		}

		void DisposePathWatcher()
		{
			if (_pathWatcher != null)
			{
				_pathWatcher.Changed -= OnPathChanged;
				_pathWatcher.Dispose();
				_pathWatcher = null;
			}
		}

		void OnPathChanged() => Evaluate();

		internal void Evaluate()
		{
			var root = GetRoot();
			var current = BindingEngine.ResolvePath(root, _trigger.Binding?.PathString);
			bool match = Equals(current, _trigger.Value);
			bool active = _appliedDps != null;
			if (match && !active) Apply();
			else if (!match && active) UnApply(runActions: true);
		}

		void Apply()
		{
			_appliedDps = new List<DependencyProperty>();
			foreach (var setter in _trigger.Setters)
				setter.ApplyToTriggerLayer(_target, this, _appliedDps);
			foreach (var action in _trigger.EnterActions)
				action.Invoke(_target);
		}

		/// <param name="runActions">
		/// <c>true</c> on a genuine active→inactive transition;
		/// <c>false</c> on style teardown — setters removed silently, no ExitActions.
		/// </param>
		void UnApply(bool runActions)
		{
			if (_appliedDps == null) return;
			var dps = _appliedDps;
			_appliedDps = null;
			_trigger.Setters.UnapplyTriggerLayer(_target, this);
			_target.ClearTriggerValues(dps, this);
			if (runActions)
			{
				foreach (var action in _trigger.ExitActions)
					action.Invoke(_target);
			}
		}

		internal void Dispose()
		{
			if (_dcHook != null)
				_target.RemoveChangeHook(FrameworkElement.DataContextProperty, _dcHook);
			DisposePathWatcher();
			UnApply(runActions: false);
		}
	}
}
