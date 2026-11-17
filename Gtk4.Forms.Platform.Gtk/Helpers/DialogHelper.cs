using System.Linq;
using Gtk;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Platform.GTK.Extensions;

namespace Gtk4.Forms.Platform.GTK.Helpers;
// TODO GTK4: migrate to Gtk.AlertDialog (async Choose/ChooseFinish).
// Callers in Platform.cs use synchronous Action<> lambdas; a full rewrite
// would require making those async, which is out of scope for this compile pass.
#pragma warning disable CS0612
internal static class DialogHelper
{
	public static void ShowAlert(PlatformRenderer platformRender, AlertArguments arguments)
	{
		MessageDialog messageDialog = new MessageDialog(
			platformRender.Toplevel as Window,
			DialogFlags.DestroyWithParent,
			MessageType.Other,
			GetAlertButtons(arguments),
			arguments.Message);

		SetDialogTitle(arguments.Title, messageDialog);
		SetButtonText(arguments.Accept, ButtonsType.Ok, messageDialog);
		SetButtonText(arguments.Cancel, ButtonsType.Cancel, messageDialog);

		ResponseType result = RunDialog(messageDialog);

		if (result == ResponseType.Ok)
		{
			arguments.SetResult(true);
		}
		else
		{
			arguments.SetResult(false);
		}

		messageDialog.Destroy();
	}

	public static void ShowActionSheet(PlatformRenderer platformRender, ActionSheetArguments arguments)
	{
		MessageDialog messageDialog = new MessageDialog(
			platformRender.Toplevel as Window,
			DialogFlags.DestroyWithParent,
			MessageType.Other,
			ButtonsType.Cancel,
			null);

		SetDialogTitle(arguments.Title, messageDialog);
		SetButtonText(arguments.Cancel, ButtonsType.Cancel, messageDialog);
		SetDestructionButton(arguments.Destruction, messageDialog);
		AddExtraButtons(arguments, messageDialog);

		ResponseType result = RunDialog(messageDialog);

		if (result == ResponseType.Cancel)
		{
			arguments.SetResult(arguments.Cancel);
		}
		else if (result == ResponseType.Reject)
		{
			arguments.SetResult(arguments.Destruction);
		}

		messageDialog.Destroy();
	}

	/// <summary>
	/// Runs <paramref name="dialog"/> synchronously by spinning a local GLib.MainLoop.
	/// </summary>
	/// <remarks>
	/// GTK4 removed the nested-event-loop gtk_dialog_run (MessageDialog.Run()).
	/// The replacement is to subscribe the Response signal, present the dialog, and run
	/// a local GLib.MainLoop — quitting it when the signal fires — which is exactly what
	/// gtk_dialog_run did internally (Application.cs:108 uses the same pattern).
	/// TODO GTK4: replace with Gtk.AlertDialog.Choose / ChooseFinish once callers are async.
	/// </remarks>
	static ResponseType RunDialog(MessageDialog dialog)
	{
		var result = ResponseType.None;
		var loop = new GLib.MainLoop();

		dialog.Response += (o, args) =>
		{
			result = args.ResponseId;

			if (loop.IsRunning)
				loop.Quit();
		};

		dialog.Present();
		loop.Run();

		return result;
	}

	static void SetDialogTitle(string title, MessageDialog messageDialog)
	{
		messageDialog.Title = title ?? string.Empty;
	}

	static void SetButtonText(string text, ButtonsType type, MessageDialog messageDialog)
	{
		string gtkLabel = string.Empty;

		switch (type)
		{
			case ButtonsType.Ok:
				gtkLabel = "gtk-ok";
				break;
			case ButtonsType.Cancel:
				gtkLabel = "gtk-cancel";
				break;
		}

		// GTK4: GtkHButtonBox is gone; buttons live in a plain Box inside ContentArea.
		var buttonsBox = messageDialog.GetDescendants()
			.OfType<Box>()
			.FirstOrDefault();

		if (buttonsBox == null)
			return;

		var targetButton = buttonsBox.GetDescendants()
			.OfType<Gtk.Button>()
			.FirstOrDefault(x => x.Label == gtkLabel);

		if (targetButton == null)
			return;

		if (string.IsNullOrEmpty(text))
		{
			targetButton.Hide();
		}
		else
		{
			targetButton.Label = text;
		}
	}

	static void SetDestructionButton(string destruction, MessageDialog messageDialog)
	{
		if (!string.IsNullOrEmpty(destruction))
		{
			var destructionButton =
				messageDialog.AddButton(destruction, ResponseType.Reject) as Gtk.Button;

			var destructionColor = Color.Red.ToGtkColor();
			destructionButton.Child.SetForegroundColor(destructionColor, StateType.Normal);
			destructionButton.Child.SetForegroundColor(destructionColor, StateType.Prelight);
			destructionButton.Child.SetForegroundColor(destructionColor, StateType.Active);
		}
	}

	static void AddExtraButtons(ActionSheetArguments arguments, MessageDialog messageDialog)
	{
		var vbox = messageDialog.ContentArea;

		// As we are not showing any message in this dialog, we just
		// hide default container and avoid it from using space
		vbox.Children[0].Hide();

		if (arguments.Buttons.Any())
		{
			for (int i = 0; i < arguments.Buttons.Count(); i++)
			{
				var button = new Gtk.Button();
				button.Label = arguments.Buttons.ElementAt(i);
				button.Clicked += (o, e) =>
				{
					arguments.SetResult(button.Label);
					messageDialog.Destroy();
				};
				button.Show();

				vbox.PackStart(button, false, false, 0);
			}
		}
	}

	static ButtonsType GetAlertButtons(AlertArguments arguments)
	{
		bool hasAccept = !string.IsNullOrEmpty(arguments.Accept);
		bool hasCancel = !string.IsNullOrEmpty(arguments.Cancel);

		ButtonsType type = ButtonsType.None;

		if (hasAccept && hasCancel)
		{
			type = ButtonsType.OkCancel;
		}
		else if (hasAccept && !hasCancel)
		{
			type = ButtonsType.Ok;
		}
		else if (!hasAccept && hasCancel)
		{
			type = ButtonsType.Cancel;
		}

		return type;
	}
}
#pragma warning restore CS0612