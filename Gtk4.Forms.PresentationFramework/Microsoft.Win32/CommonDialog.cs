using System;
using System.Windows;

namespace Microsoft.Win32;

/// <summary>
/// Base class for the file dialogs. A dialog is shown modally and returns whether the user accepted it.
/// </summary>
public abstract class CommonDialog
{
	/// <summary>Injectable native presenter; replace in tests so no real dialog opens.</summary>
	internal static IFileDialogPresenter Presenter { get; set; } = new GtkFileDialogPresenter();

	/// <summary>Creates a dialog.</summary>
	protected CommonDialog()
	{
	}

	/// <summary>An arbitrary object the caller associates with the dialog.</summary>
	public object? Tag { get; set; }

	// The owner of the current ShowDialog call; GTK needs the window, not a handle.
	internal Window? OwnerWindow { get; set; }

	/// <summary>Restores every property to its default.</summary>
	public abstract void Reset();

	/// <summary>
	/// Runs the dialog. <paramref name="hwndOwner"/> is always <see cref="IntPtr.Zero"/>: GTK has no window handle.
	/// </summary>
	/// <returns><see langword="true"/> when the user accepted the dialog.</returns>
	protected abstract bool RunDialog(IntPtr hwndOwner);

	/// <summary>Called before the dialog is shown; derived classes may throw to refuse.</summary>
	protected virtual void CheckPermissionsToShowDialog()
	{
	}

	/// <summary>Shows the dialog without an owner.</summary>
	/// <returns><see langword="true"/> when accepted, <see langword="false"/> when cancelled.</returns>
	public virtual bool? ShowDialog()
	{
		CheckPermissionsToShowDialog();
		OwnerWindow = null;
		return RunDialog(IntPtr.Zero);
	}

	/// <summary>Shows the dialog on top of <paramref name="owner"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="owner"/> is null.</exception>
	public bool? ShowDialog(Window owner)
	{
		if (owner == null)
			throw new ArgumentNullException(nameof(owner));
		CheckPermissionsToShowDialog();
		OwnerWindow = owner;
		try
		{
			return RunDialog(IntPtr.Zero);
		}
		finally
		{
			OwnerWindow = null;
		}
	}
}
