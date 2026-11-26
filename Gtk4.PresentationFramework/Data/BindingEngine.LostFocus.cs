namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		UpdateSourceTrigger _trigger = UpdateSourceTrigger.PropertyChanged;
		RoutedEventHandler? _lostFocusHandler;
		bool _dirty;

		// LostFocus: target changes only mark the binding dirty; the write happens when
		// UIElement.LostFocusEvent (raised by the GTK focus controller) reaches the target.
		void AttachLostFocus(DependencyObject target)
		{
			if (_trigger != UpdateSourceTrigger.LostFocus || _lostFocusHandler != null) return;
			_lostFocusHandler = OnTargetLostFocus;
			target.AddHandler(UIElement.LostFocusEvent, _lostFocusHandler);
		}

		void DetachLostFocus()
		{
			if (_lostFocusHandler == null) return;
			if (_targetRef.TryGetTarget(out var target))
				target.RemoveHandler(UIElement.LostFocusEvent, _lostFocusHandler);
			_lostFocusHandler = null;
		}

		void OnTargetLostFocus(object sender, RoutedEventArgs e)
		{
			if (!_dirty || !_targetRef.TryGetTarget(out var target)) return;
			WriteBack(target);
		}
	}
}
