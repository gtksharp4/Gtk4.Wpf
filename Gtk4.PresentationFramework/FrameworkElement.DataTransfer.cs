using System.Windows.Data;

namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>Occurs when a value is transferred to the binding source (<c>NotifyOnSourceUpdated</c>).</summary>
	public event EventHandler<DataTransferEventArgs> SourceUpdated
	{
		add => AddHandler(Binding.SourceUpdatedEvent, value);
		remove => RemoveHandler(Binding.SourceUpdatedEvent, value);
	}

	/// <summary>Occurs when a value is transferred to the binding target (<c>NotifyOnTargetUpdated</c>).</summary>
	public event EventHandler<DataTransferEventArgs> TargetUpdated
	{
		add => AddHandler(Binding.TargetUpdatedEvent, value);
		remove => RemoveHandler(Binding.TargetUpdatedEvent, value);
	}
}
