using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>
/// The automation peer of a <see cref="ProgressBar"/>. The layer has no <c>RangeBase</c>, so it derives from
/// <see cref="FrameworkElementAutomationPeer"/> directly. The value is read-only, as in WPF.
/// </summary>
public class ProgressBarAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
	public ProgressBarAutomationPeer(ProgressBar owner) : base(owner) { }

	ProgressBar Bar => (ProgressBar)Owner;

	// An indeterminate bar has no value to report.
	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.RangeValue
			? (Bar.IsIndeterminate ? null : this)
			: base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "ProgressBar";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ProgressBar;

	bool IRangeValueProvider.IsReadOnly => true;

	double IRangeValueProvider.LargeChange => double.NaN;

	double IRangeValueProvider.SmallChange => double.NaN;

	double IRangeValueProvider.Maximum => Bar.Maximum;

	double IRangeValueProvider.Minimum => Bar.Minimum;

	double IRangeValueProvider.Value => Bar.Value;

	void IRangeValueProvider.SetValue(double value)
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		throw new InvalidOperationException("A progress bar value is read-only.");
	}

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == ProgressBar.ValueProperty || property == ProgressBar.MinimumProperty
		|| property == ProgressBar.MaximumProperty || property == ProgressBar.IsIndeterminateProperty
		|| base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		if (Bar.IsIndeterminate) return;
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueMin, Bar.Minimum);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueMax, Bar.Maximum);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ValueNow, Bar.Value);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == ProgressBar.ValueProperty)
			RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
		else if (e.Property == ProgressBar.MinimumProperty)
			RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MinimumProperty, e.OldValue, e.NewValue);
		else if (e.Property == ProgressBar.MaximumProperty)
			RaisePropertyChangedEvent(RangeValuePatternIdentifiers.MaximumProperty, e.OldValue, e.NewValue);
	}
}
