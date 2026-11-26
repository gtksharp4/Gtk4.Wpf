namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		PropertyChangedCallback? _targetChangedCallback;

		void SubscribeTargetChanges(DependencyObject target)
		{
			UnsubscribeTargetChanges();
			_targetChangedCallback = OnTargetPropertyChanged;
			TargetChangeTracker.Subscribe(target, _dp, _targetChangedCallback);
			AttachLostFocus(target);
		}

		void UnsubscribeTargetChanges()
		{
			DetachLostFocus();
			if (_targetChangedCallback == null) return;
			if (_targetRef.TryGetTarget(out var target))
				TargetChangeTracker.Unsubscribe(target, _dp, _targetChangedCallback);
			_targetChangedCallback = null;
		}

		void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			// Explicit: don't auto write back — wait for UpdateSource().
			if (_trigger == UpdateSourceTrigger.Explicit) return;
			if (_trigger == UpdateSourceTrigger.LostFocus)
			{
				_dirty = !_pushing;
				return;
			}
			WriteBack(target);
		}

	}
}
