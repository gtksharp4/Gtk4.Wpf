using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="MessageBox"/> button-set → result mapping via a fake presenter.
/// No window is opened; GTK is not required.
/// </summary>
public class MessageBoxTests : IDisposable
{
	readonly IMessageBoxPresenter _original;

	public MessageBoxTests()
	{
		_original = MessageBox.Presenter;
	}

	public void Dispose()
	{
		MessageBox.Presenter = _original;
	}

	// ── Button label mapping (pure, no presenter needed) ─────────────────────

	[Theory]
	[InlineData("_OK",     MessageBoxResult.OK)]
	[InlineData("OK",      MessageBoxResult.OK)]
	[InlineData("_Cancel", MessageBoxResult.Cancel)]
	[InlineData("Cancel",  MessageBoxResult.Cancel)]
	[InlineData("_Yes",    MessageBoxResult.Yes)]
	[InlineData("Yes",     MessageBoxResult.Yes)]
	[InlineData("_No",     MessageBoxResult.No)]
	[InlineData("No",      MessageBoxResult.No)]
	[InlineData(null,      MessageBoxResult.None)]
	[InlineData("Other",   MessageBoxResult.None)]
	public void ResultFromLabel_maps_correctly(string? label, MessageBoxResult expected)
	{
		Assert.Equal(expected, MessageBoxButtonMapping.ResultFromLabel(label));
	}

	[Theory]
	[InlineData(MessageBoxButton.OK,          MessageBoxResult.OK)]
	[InlineData(MessageBoxButton.OKCancel,    MessageBoxResult.Cancel)]
	[InlineData(MessageBoxButton.YesNo,       MessageBoxResult.None)]
	[InlineData(MessageBoxButton.YesNoCancel, MessageBoxResult.Cancel)]
	public void DefaultResult_is_the_win32_dismissal_result(MessageBoxButton btn, MessageBoxResult expected)
	{
		Assert.Equal(expected, MessageBoxButtonMapping.DefaultResult(btn));
	}

	[Theory]
	[InlineData(MessageBoxButton.OK,          1)]
	[InlineData(MessageBoxButton.OKCancel,    2)]
	[InlineData(MessageBoxButton.YesNo,       2)]
	[InlineData(MessageBoxButton.YesNoCancel, 3)]
	public void ButtonLabels_returns_correct_count(MessageBoxButton btn, int expected)
	{
		Assert.Equal(expected, MessageBoxButtonMapping.ButtonLabels(btn).Count);
	}

	// ── Show() delegates to the injected presenter ────────────────────────────

	[Fact]
	public void Show_text_only_calls_presenter_with_OK_button()
	{
		var fake = new FakePresenter(MessageBoxResult.OK);
		MessageBox.Presenter = fake;
		var r = MessageBox.Show("hello");
		Assert.Equal(MessageBoxResult.OK, r);
		Assert.Equal("hello", fake.LastText);
		Assert.Equal(MessageBoxButton.OK, fake.LastButton);
	}

	[Fact]
	public void Show_with_caption_passes_caption()
	{
		var fake = new FakePresenter(MessageBoxResult.Cancel);
		MessageBox.Presenter = fake;
		MessageBox.Show("msg", "cap");
		Assert.Equal("cap", fake.LastCaption);
	}

	[Fact]
	public void Show_with_button_YesNo_calls_presenter_YesNo()
	{
		var fake = new FakePresenter(MessageBoxResult.No);
		MessageBox.Presenter = fake;
		var r = MessageBox.Show("q", "title", MessageBoxButton.YesNo);
		Assert.Equal(MessageBoxResult.No, r);
		Assert.Equal(MessageBoxButton.YesNo, fake.LastButton);
	}

	[Fact]
	public void Show_full_overload_passes_all_arguments()
	{
		var fake = new FakePresenter(MessageBoxResult.Yes);
		MessageBox.Presenter = fake;
		MessageBox.Show("text", "cap", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
		Assert.Equal(MessageBoxButton.YesNoCancel, fake.LastButton);
		Assert.Equal(MessageBoxImage.Warning, fake.LastIcon);
	}

	// ── F4: GtkMessageBoxPresenter.MapResponse — per-button index mapping ───────

	/// <summary>
	/// Each button index must map to the correct MessageBoxResult.
	/// These cover the pure response-collection logic added in F3/F4;
	/// the GTK dialog itself is not opened (no display needed).
	/// </summary>
	[Theory]
	[InlineData(MessageBoxButton.OK,          0, MessageBoxResult.OK)]
	[InlineData(MessageBoxButton.OKCancel,    0, MessageBoxResult.OK)]
	[InlineData(MessageBoxButton.OKCancel,    1, MessageBoxResult.Cancel)]
	[InlineData(MessageBoxButton.YesNo,       0, MessageBoxResult.Yes)]
	[InlineData(MessageBoxButton.YesNo,       1, MessageBoxResult.No)]
	[InlineData(MessageBoxButton.YesNoCancel, 0, MessageBoxResult.Yes)]
	[InlineData(MessageBoxButton.YesNoCancel, 1, MessageBoxResult.No)]
	[InlineData(MessageBoxButton.YesNoCancel, 2, MessageBoxResult.Cancel)]
	public void MapResponse_index_maps_to_correct_result(
		MessageBoxButton button, int index, MessageBoxResult expected)
	{
		Assert.Equal(expected, GtkMessageBoxPresenter.MapResponse(index, button));
	}

	[Theory]
	[InlineData(MessageBoxButton.OK, MessageBoxResult.OK)]
	[InlineData(MessageBoxButton.OKCancel, MessageBoxResult.Cancel)]
	[InlineData(MessageBoxButton.YesNo, MessageBoxResult.None)]
	[InlineData(MessageBoxButton.YesNoCancel, MessageBoxResult.Cancel)]
	public void MapResponse_negative_index_is_the_dismissal_result(MessageBoxButton button, MessageBoxResult expected)
	{
		Assert.Equal(expected, GtkMessageBoxPresenter.MapResponse(-1, button));
	}

	[Theory]
	[InlineData(MessageBoxButton.OK)]
	[InlineData(MessageBoxButton.OKCancel)]
	[InlineData(MessageBoxButton.YesNo)]
	[InlineData(MessageBoxButton.YesNoCancel)]
	public void MapResponse_out_of_range_returns_None(MessageBoxButton button)
	{
		Assert.Equal(MessageBoxResult.None, GtkMessageBoxPresenter.MapResponse(99, button));
	}

	// ── MapIconName — WPF image → themed icon name ──────────────────────────────

	[Theory]
	[InlineData(MessageBoxImage.Error, "dialog-error")]
	[InlineData(MessageBoxImage.Warning, "dialog-warning")]
	[InlineData(MessageBoxImage.Question, "dialog-question")]
	[InlineData(MessageBoxImage.Information, "dialog-information")]
	public void MapIconName_maps_known_images(MessageBoxImage image, string expected)
	{
		Assert.Equal(expected, GtkMessageBoxPresenter.MapIconName(image));
	}

	[Fact]
	public void MapIconName_none_has_no_icon()
	{
		Assert.Null(GtkMessageBoxPresenter.MapIconName(MessageBoxImage.None));
	}

	// ── Fake presenter ────────────────────────────────────────────────────────

	sealed class FakePresenter : IMessageBoxPresenter
	{
		readonly MessageBoxResult _result;
		public string? LastText;
		public string? LastCaption;
		public MessageBoxButton LastButton;
		public MessageBoxImage LastIcon;

		public FakePresenter(MessageBoxResult result) => _result = result;

		public MessageBoxResult Show(
			string messageText, string caption,
			MessageBoxButton button, MessageBoxImage icon)
		{
			LastText = messageText;
			LastCaption = caption;
			LastButton = button;
			LastIcon = icon;
			return _result;
		}
	}
}