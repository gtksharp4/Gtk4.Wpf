using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="ScrollViewer"/>.</summary>
public class ScrollViewerAutomationPeer : FrameworkElementAutomationPeer, IScrollProvider
{
	double _horizontalPercent;
	double _verticalPercent;

	public ScrollViewerAutomationPeer(ScrollViewer owner) : base(owner)
	{
		_horizontalPercent = ((IScrollProvider)this).HorizontalScrollPercent;
		_verticalPercent = ((IScrollProvider)this).VerticalScrollPercent;
		owner.ScrollChanged += (s, e) => OnScrolled();
	}

	ScrollViewer Viewer => (ScrollViewer)Owner;

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Scroll ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "ScrollViewer";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

	protected override bool IsControlElementCore() =>
		Viewer.TemplatedParent == null || Viewer.TemplatedParent is ContentPresenter;

	bool Horizontal => Viewer.ScrollableWidth > 0;

	bool Vertical => Viewer.ScrollableHeight > 0;

	bool IScrollProvider.HorizontallyScrollable => Horizontal;

	bool IScrollProvider.VerticallyScrollable => Vertical;

	double IScrollProvider.HorizontalScrollPercent =>
		Horizontal ? Viewer.HorizontalOffset / Viewer.ScrollableWidth * 100 : ScrollPatternIdentifiers.NoScroll;

	double IScrollProvider.VerticalScrollPercent =>
		Vertical ? Viewer.VerticalOffset / Viewer.ScrollableHeight * 100 : ScrollPatternIdentifiers.NoScroll;

	double IScrollProvider.HorizontalViewSize =>
		Horizontal && Viewer.ExtentWidth > 0 ? Viewer.ViewportWidth / Viewer.ExtentWidth * 100 : 100;

	double IScrollProvider.VerticalViewSize =>
		Vertical && Viewer.ExtentHeight > 0 ? Viewer.ViewportHeight / Viewer.ExtentHeight * 100 : 100;

	void IScrollProvider.Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Check(horizontalAmount);
		Check(verticalAmount);
		if ((horizontalAmount != ScrollAmount.NoAmount && !Horizontal)
			|| (verticalAmount != ScrollAmount.NoAmount && !Vertical))
			throw new InvalidOperationException("The element cannot scroll in that direction.");
		switch (horizontalAmount)
		{
			case ScrollAmount.LargeDecrement: Viewer.PageLeft(); break;
			case ScrollAmount.SmallDecrement: Viewer.LineLeft(); break;
			case ScrollAmount.LargeIncrement: Viewer.PageRight(); break;
			case ScrollAmount.SmallIncrement: Viewer.LineRight(); break;
		}
		switch (verticalAmount)
		{
			case ScrollAmount.LargeDecrement: Viewer.PageUp(); break;
			case ScrollAmount.SmallDecrement: Viewer.LineUp(); break;
			case ScrollAmount.LargeIncrement: Viewer.PageDown(); break;
			case ScrollAmount.SmallIncrement: Viewer.LineDown(); break;
		}
	}

	void IScrollProvider.SetScrollPercent(double horizontalPercent, double verticalPercent)
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		CheckPercent(horizontalPercent, nameof(horizontalPercent));
		CheckPercent(verticalPercent, nameof(verticalPercent));
		if ((horizontalPercent != ScrollPatternIdentifiers.NoScroll && !Horizontal)
			|| (verticalPercent != ScrollPatternIdentifiers.NoScroll && !Vertical))
			throw new InvalidOperationException("The element cannot scroll in that direction.");
		if (horizontalPercent != ScrollPatternIdentifiers.NoScroll)
			Viewer.ScrollToHorizontalOffset(Viewer.ScrollableWidth * horizontalPercent / 100);
		if (verticalPercent != ScrollPatternIdentifiers.NoScroll)
			Viewer.ScrollToVerticalOffset(Viewer.ScrollableHeight * verticalPercent / 100);
	}

	static void Check(ScrollAmount amount)
	{
		if (amount < ScrollAmount.LargeDecrement || amount > ScrollAmount.SmallIncrement)
			throw new InvalidOperationException("Invalid scroll amount.");
	}

	static void CheckPercent(double percent, string name)
	{
		if (percent != ScrollPatternIdentifiers.NoScroll && (percent < 0 || percent > 100))
			throw new ArgumentOutOfRangeException(name);
	}

	void OnScrolled()
	{
		var scroll = (IScrollProvider)this;
		var h = scroll.HorizontalScrollPercent;
		var v = scroll.VerticalScrollPercent;
		RaisePropertyChangedEvent(ScrollPatternIdentifiers.HorizontalScrollPercentProperty, _horizontalPercent, h);
		RaisePropertyChangedEvent(ScrollPatternIdentifiers.VerticalScrollPercentProperty, _verticalPercent, v);
		_horizontalPercent = h;
		_verticalPercent = v;
	}
}
