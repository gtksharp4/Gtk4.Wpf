using System.Globalization;

namespace System.Windows.Data;

internal static partial class MultiBindingEngine
{
	internal sealed partial class MultiRecord
	{
		UpdateSourceTrigger _trigger = UpdateSourceTrigger.PropertyChanged;
		RoutedEventHandler? _lostFocusHandler;
		bool _dirty;

		void AttachTwoWay(FrameworkElement target, UpdateSourceTrigger trigger)
		{
			_trigger = trigger;
			if (trigger == UpdateSourceTrigger.Explicit) return;
			_targetChangedCallback = OnTargetPropertyChanged;
			TargetChangeTracker.Subscribe(target, _dp, _targetChangedCallback);
			if (trigger != UpdateSourceTrigger.LostFocus) return;
			_lostFocusHandler = OnTargetLostFocus;
			target.AddHandler(UIElement.LostFocusEvent, _lostFocusHandler);
		}

		void DetachTwoWay()
		{
			if (!_targetRef.TryGetTarget(out var t)) t = null;
			if (_targetChangedCallback != null && t != null)
				TargetChangeTracker.Unsubscribe(t, _dp, _targetChangedCallback);
			_targetChangedCallback = null;
			if (_lostFocusHandler != null && t != null)
				t.RemoveHandler(UIElement.LostFocusEvent, _lostFocusHandler);
			_lostFocusHandler = null;
		}

		void OnTargetLostFocus(object sender, RoutedEventArgs e)
		{
			if (_dirty && _targetRef.TryGetTarget(out var target)) WriteBack(target);
		}

		/// <summary>TwoWay write-back: target DP changed (or lost focus) → ConvertBack → write each child.</summary>
		void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (_pushing) return;
			if (!_targetRef.TryGetTarget(out var target)) return;
			if (_trigger == UpdateSourceTrigger.LostFocus) { _dirty = true; return; }
			WriteBack(target);
		}

		internal void WriteBackNow(FrameworkElement target) => WriteBack(target);

		void WriteBack(FrameworkElement target)
		{
			_dirty = false;
			if (_mb.Converter == null) return;

			var targetValue = target.GetValue(_dp);
			var sourceTypes = new Type[_mb.Bindings.Count];
			for (int i = 0; i < sourceTypes.Length; i++)
			{
				var src = ResolveChildSource(target, _mb.Bindings[i]);
				sourceTypes[i] = BindingEngine.TryResolveWriteTarget(src, _mb.Bindings[i].PathString, out _, out var p)
					? p.PropertyType
					: typeof(object);
			}

			object?[]? backValues;
			try
			{
				backValues = _mb.Converter.ConvertBack(
					targetValue, sourceTypes, _mb.ConverterParameter, BindingEngine.ResolveCulture(_mb.ConverterCulture));
			}
			catch (Exception ex) when (!BindingEngine.IsCritical(ex))
			{
				return;
			}
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
					try
					{
						if (BindingEngine.TryResolveWriteTarget(src, b.PathString, out _, out var p)
						    && !BindingEngine.TryConvertToSourceType(bv, p.PropertyType, out bv, out _,
							    BindingEngine.ResolveCulture(_mb.ConverterCulture)))
							continue;
						BindingEngine.WriteBackPath(src, b.PathString, bv);
					}
					catch (Exception ex) when (!BindingEngine.IsCritical(ex))
					{
						// WPF skips a child whose write fails.
					}
				}
			}
			finally
			{
				_pushing = false;
			}
		}
	}
}
