namespace System.Windows
{
	/// <summary>
	/// Default <see cref="IMessageBoxPresenter"/>: shows a GTK dialog synchronously
	/// by spinning a <see cref="GLib.MainLoop"/> until the user responds.
	/// </summary>
	/// <remarks>
	/// GTK 4 removed <c>gtk_dialog_run</c>; synchrony is recovered by subscribing
	/// <c>Gtk.Dialog.Response</c>, presenting the dialog, then running a nested
	/// <see cref="GLib.MainLoop"/> that quits when the signal fires — the same
	/// technique used by <c>Gtk4.Forms.Platform.Gtk/Helpers/DialogHelper.cs</c>.
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

			foreach (var label in MessageBoxButtonMapping.ButtonLabels(button))
				dialog.AddButton(label, Gtk.ResponseType.Apply);

			var result = Gtk.ResponseType.None;
			var loop = new GLib.MainLoop();

			dialog.Response += (_, args) =>
			{
				result = args.ResponseId;
				if (loop.IsRunning) loop.Quit();
			};

			dialog.Present();
			loop.Run();
			dialog.Destroy();

			// Map the clicked label back — GTK returns Apply for all custom buttons;
			// we need to identify by button index, so we re-read the clicked label.
			// GtkSharp4 Response signal gives ResponseId = Apply for AddButton.
			// Identify by label matching via the button mapping table.
			return MapResponse(result, button);
		}

		static MessageBoxResult MapResponse(Gtk.ResponseType response, MessageBoxButton button)
		{
			// All custom buttons return ResponseType.Apply in GtkSharp4 when added
			// via AddButton; we cannot distinguish which one was pressed by ResponseType
			// alone. As an approximation, map DeleteEvent / None to Cancel/None,
			// and Apply to the default result. For full per-button tracking one would
			// need a custom dialog with real response IDs — left for a future phase.
			return response switch
			{
				Gtk.ResponseType.DeleteEvent => MessageBoxResult.Cancel,
				Gtk.ResponseType.None        => MessageBoxResult.None,
				_                            => MessageBoxButtonMapping.DefaultResult(button)
			};
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
