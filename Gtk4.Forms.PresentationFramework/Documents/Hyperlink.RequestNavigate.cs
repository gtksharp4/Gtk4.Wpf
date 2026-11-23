using System.Windows.Navigation;

namespace System.Windows.Documents;

public partial class Hyperlink
{
	/// <summary>Identifies the bubbling RequestNavigate routed event.</summary>
	public static readonly RoutedEvent RequestNavigateEvent = EventManager.RegisterRoutedEvent(
		"RequestNavigate", RoutingStrategy.Bubble, typeof(RequestNavigateEventHandler), typeof(Hyperlink));

	/// <summary>Raised when navigation is requested; bubbles to ancestors and stops when handled.</summary>
	public event RequestNavigateEventHandler RequestNavigate
	{
		add => this.AddHandler(RequestNavigateEvent, value);
		remove => this.RemoveHandler(RequestNavigateEvent, value);
	}

	void RaiseEvent(RoutedEventArgs args) => RoutedEventHost.RaiseEvent(this, args);
}
