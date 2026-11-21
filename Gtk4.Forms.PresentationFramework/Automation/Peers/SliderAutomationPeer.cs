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

	public bool IsReadOnly => false;

	public double LargeChange => Slider.LargeChange;

	public double SmallChange => Slider.SmallChange;

	public double Maximum => Slider.Maximum;

	public double Minimum => Slider.Minimum;

	public double Value => Slider.Value;

	public void SetValue(double value)
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (value < Minimum || value > Maximum) throw new ArgumentOutOfRangeException(nameof(value));
		Slider.Value = value;
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == Slider.ValueProperty || property == Slider.MinimumProperty
		|| property == Slider.MaximumProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueMin, Minimum);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueMax, Maximum);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueNow, Value);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == Slider.ValueProperty)
			RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
	}
}
