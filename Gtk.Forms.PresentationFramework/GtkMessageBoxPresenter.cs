namespace System.Windows
{
	/// <summary>
	/// Default <see cref="IMessageBoxPresenter"/>: shows a GTK dialog synchronously
	/// by pumping the GTK main loop until the user responds.
	/// </summary>
	/// <remarks>
	/// GTK 4 removed <c>gtk_dialog_run</c>; synchrony is recovered by subscribing
	/// <c>Gtk.Dialog.Response</c>, presenting the dialog, then pumping
	/// <c>Gtk.Application.RunIteration(true)</c> until the response flag is set — the
	/// same technique used by <see cref="Window.ShowDialog"/>.  A blocking nested
	/// <see cref="GLib.MainLoop"/> is NOT used because it would freeze the single GTK
	/// thread, preventing the response signal from firing.
	/// <para>
	/// <b>Non-interactive environments:</b> in a headless test or CI session the dialog
	/// blocks indefinitely because no user can click a button. Inject a fake
	/// <see cref="IMessageBoxPresenter"/> (via <see cref="MessageBox.Presenter"/>) to
	/// avoid this. The result-mapping logic in <see cref="MessageBoxButtonMapping"/>
	/// is pure and covered by tests without opening any window.
	/// </para>
	/// </remarks>
#pragma warning disable CS0618 // Gtk.MessageDialog is an [Obsolete] GTK3 compat shim in GtkSharp4
	sealed class GtkMessageBoxPresenter : IMessageBoxPresenter
	{
		public MessageBoxResult Show(
			string messageText,
			string caption,
			MessageBoxButton button,
			MessageBoxImage icon)
		{
			var gtkType = MapIcon(icon);
			var dialog = new Gtk.MessageDialog(
				null,
				Gtk.DialogFlags.Modal,
				gtkType,
				Gtk.ButtonsType.None,
				messageText);

			dialog.Title = caption;

			// Add each button with a distinct response id (index-based) so we can map
			// which button was clicked without relying on button labels (F4 fix).
			var labels = MessageBoxButtonMapping.ButtonLabels(button);
			for (int i = 0; i < labels.Count; i++)
				dialog.AddButton(labels[i], (Gtk.ResponseType)i);

			int responseIndex = -1;
			bool responded = false;

			dialog.Response += (_, args) =>
			{
				responseIndex = (int)args.ResponseId;
				responded = true;
			};

			dialog.Present();

			// Pump the GTK main loop until the response fires. RunIteration(true) blocks
			// for the next event and dispatches it, keeping the thread live without a
			// nested GLib.MainLoop that would deadlock same-thread handlers.
			while (!responded)
				Gtk.Application.RunIteration(true);

			dialog.Destroy();

			return MapResponse(responseIndex, button);
		}

		/// <summary>
		/// Maps the response index (assigned during button creation) back to a
		/// <see cref="MessageBoxResult"/> via the button label at that index.
		/// A negative index means the window was closed without a button click.
		/// </summary>
		internal static MessageBoxResult MapResponse(int responseIndex, MessageBoxButton button)
		{
			if (responseIndex < 0)
				return MessageBoxResult.None;

			var labels = MessageBoxButtonMapping.ButtonLabels(button);
			if (responseIndex >= labels.Count)
				return MessageBoxResult.None;

			return MessageBoxButtonMapping.ResultFromLabel(labels[responseIndex]);
		}

		static Gtk.MessageType MapIcon(MessageBoxImage icon) =>
			icon switch
			{
				MessageBoxImage.Error or
				MessageBoxImage.Stop or
				MessageBoxImage.Hand      => Gtk.MessageType.Error,
				MessageBoxImage.Warning or
				MessageBoxImage.Exclamation => Gtk.MessageType.Warning,
				MessageBoxImage.Question    => Gtk.MessageType.Question,
				MessageBoxImage.Information or
				MessageBoxImage.Asterisk    => Gtk.MessageType.Info,
				_                           => Gtk.MessageType.Other
			};
	}
#pragma warning restore CS0618
}
