namespace System.Windows;

/// <summary>
/// Default <see cref="IMessageBoxPresenter"/>: shows a GTK dialog synchronously
/// by pumping the GTK main loop until the user responds.
/// </summary>
/// <remarks>
/// Builds a custom dialog (not <c>Gtk.MessageDialog</c>) so the layout matches WPF: an
/// optional icon beside the message, and the action buttons right-aligned along the bottom.
/// GTK 4 removed <c>gtk_dialog_run</c>; synchrony is recovered by presenting the window and
/// pumping <c>Gtk.Application.RunIteration(true)</c> until a button is clicked — the same
/// technique used by <see cref="Window.ShowDialog"/>. A blocking nested
/// <see cref="GLib.MainLoop"/> is NOT used because it would freeze the single GTK thread,
/// preventing the button handlers from firing.
/// <para>
/// <b>Non-interactive environments:</b> in a headless test or CI session the dialog
/// blocks indefinitely because no user can click a button. Inject a fake
/// <see cref="IMessageBoxPresenter"/> (via <see cref="MessageBox.Presenter"/>) to
/// avoid this. The result-mapping logic in <see cref="MessageBoxButtonMapping"/>
/// is pure and covered by tests without opening any window.
/// </para>
/// </remarks>
sealed class GtkMessageBoxPresenter : IMessageBoxPresenter
{
	public MessageBoxResult Show(
		string messageText,
		string caption,
		MessageBoxButton button,
		MessageBoxImage icon)
	{
		var win = new Gtk.Window
		{
			Title = caption,
			Modal = true,
			Resizable = false,
		};

		var root = new Gtk.Box(Gtk.Orientation.Vertical, 16)
		{
			MarginTop = 16,
			MarginBottom = 16,
			MarginStart = 16,
			MarginEnd = 16,
		};

		// Message row: optional icon beside the (wrapped, left-aligned) text, as in WPF.
		var messageRow = new Gtk.Box(Gtk.Orientation.Horizontal, 12);
		string? iconName = MapIconName(icon);
		if (iconName != null)
		{
			var img = Gtk.Image.NewFromIconName(iconName);
			img.WidthRequest = 32;
			img.HeightRequest = 32;
			img.Valign = Gtk.Align.Start;
			messageRow.Append(img);
		}
		messageRow.Append(new Gtk.Label(messageText)
		{
			Wrap = true,
			Xalign = 0f,
			Valign = Gtk.Align.Center,
		});
		root.Append(messageRow);

		// Action row: buttons right-aligned along the bottom (WPF places them bottom-right;
		// Gtk.MessageDialog placed them at the start).
		var actions = new Gtk.Box(Gtk.Orientation.Horizontal, 8) { Halign = Gtk.Align.End };

		int responseIndex = -1;
		bool responded = false;

		var labels = MessageBoxButtonMapping.ButtonLabels(button);
		for (int i = 0; i < labels.Count; i++)
		{
			int index = i; // capture per-button for the handler
			// UseUnderline renders the "_" mnemonic prefix as an accelerator (e.g. "_OK" → OK)
			// rather than showing the literal underscore.
			var b = new Gtk.Button { Label = labels[i], UseUnderline = true, WidthRequest = 88 };
			b.Clicked += (_, _) => { responseIndex = index; responded = true; };
			actions.Append(b);
		}
		root.Append(actions);

		win.Child = root;

		// Closing the window with no button click maps to DefaultResult (Win32 dismissal).
		win.CloseRequest += (_, _) => { responded = true; };

		win.Present();

		// Pump the GTK main loop until a button fires. RunIteration(true) blocks for the next
		// event and dispatches it, keeping the thread live without a nested GLib.MainLoop that
		// would deadlock same-thread handlers.
		while (!responded)
			Gtk.Application.RunIteration(true);

		win.Destroy();

		return MapResponse(responseIndex, button);
	}

	/// <summary>
	/// Maps the response index (assigned during button creation) back to a
	/// <see cref="MessageBoxResult"/> via the button label at that index.
	/// A negative index means the window was closed without a click: <see cref="MessageBoxButtonMapping.DefaultResult"/>.
	/// </summary>
	internal static MessageBoxResult MapResponse(int responseIndex, MessageBoxButton button)
	{
		if (responseIndex < 0)
			return MessageBoxButtonMapping.DefaultResult(button);

		var labels = MessageBoxButtonMapping.ButtonLabels(button);
		if (responseIndex >= labels.Count)
			return MessageBoxResult.None;

		return MessageBoxButtonMapping.ResultFromLabel(labels[responseIndex]);
	}

	/// <summary>Maps the WPF message image to a themed icon name, or null for none.</summary>
	/// <remarks>Stop/Error/Hand, Warning/Exclamation and Information/Asterisk share enum values.</remarks>
	internal static string? MapIconName(MessageBoxImage icon) =>
		icon switch
		{
			MessageBoxImage.Error       => "dialog-error",
			MessageBoxImage.Warning     => "dialog-warning",
			MessageBoxImage.Question    => "dialog-question",
			MessageBoxImage.Information => "dialog-information",
			_                           => null
		};
}
