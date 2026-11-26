namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>Keeps the widget untargetable whatever Visibility or IsHitTestVisible say (placed adorners).</summary>
	internal bool SuppressCanTarget { get; set; }

	/// <summary>Re-applies CanTarget to the widget if one exists.</summary>
	internal void RefreshCanTarget()
	{
		if (HasBackingWidget) ApplyCanTarget(_widget!);
	}
}
