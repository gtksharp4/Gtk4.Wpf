using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>Weak ErrorsChanged subscription so a source does not keep the binding record alive.</summary>
	sealed class NotifyErrorsWatcher
	{
		readonly WeakReference<BindingRecord> _record;
		readonly List<INotifyDataErrorInfo> _items = new List<INotifyDataErrorInfo>();

		internal NotifyErrorsWatcher(BindingRecord record, params INotifyDataErrorInfo?[] items)
		{
			_record = new WeakReference<BindingRecord>(record);
			foreach (var item in items)
			{
				if (item == null || _items.Contains(item)) continue;
				_items.Add(item);
				item.ErrorsChanged += OnErrorsChanged;
			}
		}

		internal void Detach()
		{
			foreach (var item in _items) item.ErrorsChanged -= OnErrorsChanged;
			_items.Clear();
		}

		void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
		{
			if (_record.TryGetTarget(out var record)) record.OnNotifyErrorsChanged(sender, e.PropertyName);
		}
	}

	internal sealed partial class BindingRecord
	{
		NotifyErrorsWatcher? _notifyWatcher;
		INotifyDataErrorInfo? _ndOwner;
		INotifyDataErrorInfo? _ndValue;
		string? _ndProp;
		List<ValidationError> _notifyErrors = new List<ValidationError>();

		/// <summary>
		/// Subscribes to the leaf owner (property errors) and, when the bound value is itself
		/// INotifyDataErrorInfo, to the value (entity errors). No-op while the objects are unchanged.
		/// </summary>
		void AttachNotifyErrors(DependencyObject target, object? source, object? resolvedValue)
		{
			INotifyDataErrorInfo? owner = null, value = null;
			string? prop = null;
			if (_binding.ValidatesOnNotifyDataErrors && TryGetLeaf(source, _binding.PathString, out var o, out prop))
			{
				owner = o as INotifyDataErrorInfo;
				value = resolvedValue as INotifyDataErrorInfo;
			}
			if (ReferenceEquals(owner, _ndOwner) && ReferenceEquals(value, _ndValue) && prop == _ndProp) return;
			bool hadErrors = _notifyErrors.Count > 0;
			DetachNotifyErrors();
			_ndOwner = owner;
			_ndValue = value;
			_ndProp = prop;
			if (owner != null || value != null) _notifyWatcher = new NotifyErrorsWatcher(this, owner, value);
			else if (hadErrors) Publish(target);
		}

		void DetachNotifyErrors()
		{
			_notifyWatcher?.Detach();
			_notifyWatcher = null;
			_ndOwner = null;
			_ndValue = null;
			_ndProp = null;
			_notifyErrors = new List<ValidationError>();
		}

		void RefreshNotifyErrors()
		{
			var list = new List<ValidationError>();
			AddErrors(list, _ndOwner, _ndProp);
			if (!ReferenceEquals(_ndValue, _ndOwner) || _ndProp != null) AddErrors(list, _ndValue, null);
			_notifyErrors = list;
		}

		static void AddErrors(List<ValidationError> list, INotifyDataErrorInfo? item, string? prop)
		{
			if (item == null) return;
			IEnumerable? errors = item.GetErrors(prop);
			if (errors == null) return;
			foreach (var e in errors)
				list.Add(new ValidationError(NotifyDataErrorValidationRule.Instance, e));
		}

		internal void OnNotifyErrorsChanged(object? sender, string? property)
		{
			bool relevant = string.IsNullOrEmpty(property)
			                || (ReferenceEquals(sender, _ndOwner) && property == _ndProp);
			if (!relevant || !_targetRef.TryGetTarget(out var target)) return;
			var dispatcher = target.Dispatcher;
			if (dispatcher.CheckAccess()) ApplyNotifyErrors(target);
			else dispatcher.BeginInvoke(() => ApplyNotifyErrors(target));
		}

		void ApplyNotifyErrors(DependencyObject target)
		{
			if (_notifyWatcher == null) return;
			RefreshNotifyErrors();
			Publish(target);
		}
	}
}
