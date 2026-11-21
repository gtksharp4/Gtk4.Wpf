using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>
/// The automation peer of a <see cref="Slider"/>. The layer has no <c>RangeBase</c>, so it derives
/// from <see cref="FrameworkElementAutomationPeer"/> directly.
/// </summary>
public class SliderAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
	public SliderAutomationPeer(Slider owner) : base(owner) { }

	Slider Slider => (Slider)Owner;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "Slider";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

	protected override AutomationOrientation GetOrientationCore() =>
		Slider.Orientation == Orientation.Horizontal ? AutomationOrientation.Horizontal
			: AutomationOrientation.Vertical;

	bool IRangeValueProvider.IsReadOnly => false;

	double IRangeValueProvider.LargeChange => Slider.LargeChange;

	double IRangeValueProvider.SmallChange => Slider.SmallChange;

	double IRangeValueProvider.Maximum => Slider.Maximum;

	double IRangeValueProvider.Minimum => Slider.Minimum;

	double IRangeValueProvider.Value => Slider.Value;

	void IRangeValueProvider.SetValue(double value)
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (value < Slider.Minimum || value > Slider.Maximum) throw new ArgumentOutOfRangeException(nameof(value));
		Slider.Value = value;
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == Slider.ValueProperty || property == Slider.MinimumProperty
		|| property == Slider.MaximumProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueMin, Slider.Minimum);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueMax, Slider.Maximum);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueNow, Slider.Value);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == Slider.ValueProperty)
			RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
	}
}
