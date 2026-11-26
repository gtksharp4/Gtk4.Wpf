using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class MessageBoxWin32ValueTests
{
	[Theory]
	[InlineData(MessageBoxButton.OK, 0)]
	[InlineData(MessageBoxButton.OKCancel, 1)]
	[InlineData(MessageBoxButton.AbortRetryIgnore, 2)]
	[InlineData(MessageBoxButton.YesNoCancel, 3)]
	[InlineData(MessageBoxButton.YesNo, 4)]
	[InlineData(MessageBoxButton.RetryCancel, 5)]
	[InlineData(MessageBoxButton.CancelTryContinue, 6)]
	public void Button_ordinals_match_wpf(MessageBoxButton button, int value) => Assert.Equal(value, (int)button);

	[Theory]
	[InlineData(MessageBoxResult.None, 0)]
	[InlineData(MessageBoxResult.OK, 1)]
	[InlineData(MessageBoxResult.Cancel, 2)]
	[InlineData(MessageBoxResult.Abort, 3)]
	[InlineData(MessageBoxResult.Retry, 4)]
	[InlineData(MessageBoxResult.Ignore, 5)]
	[InlineData(MessageBoxResult.Yes, 6)]
	[InlineData(MessageBoxResult.No, 7)]
	[InlineData(MessageBoxResult.TryAgain, 10)]
	[InlineData(MessageBoxResult.Continue, 11)]
	public void Result_ordinals_match_wpf(MessageBoxResult result, int value) => Assert.Equal(value, (int)result);

	[Fact]
	public void Image_values_and_aliases_match_wpf()
	{
		Assert.Equal(0, (int)MessageBoxImage.None);
		Assert.Equal(16, (int)MessageBoxImage.Hand);
		Assert.Equal(MessageBoxImage.Hand, MessageBoxImage.Stop);
		Assert.Equal(MessageBoxImage.Hand, MessageBoxImage.Error);
		Assert.Equal(32, (int)MessageBoxImage.Question);
		Assert.Equal(48, (int)MessageBoxImage.Exclamation);
		Assert.Equal(MessageBoxImage.Exclamation, MessageBoxImage.Warning);
		Assert.Equal(64, (int)MessageBoxImage.Asterisk);
		Assert.Equal(MessageBoxImage.Asterisk, MessageBoxImage.Information);
	}

	[Theory]
	[InlineData(MessageBoxButton.AbortRetryIgnore, "_Abort,_Retry,_Ignore")]
	[InlineData(MessageBoxButton.RetryCancel, "_Retry,_Cancel")]
	[InlineData(MessageBoxButton.CancelTryContinue, "_Cancel,_Try Again,_Continue")]
	public void New_button_sets_have_labels(MessageBoxButton button, string labels) =>
		Assert.Equal(labels, string.Join(",", MessageBoxButtonMapping.ButtonLabels(button)));

	[Theory]
	[InlineData("_Abort", MessageBoxResult.Abort)]
	[InlineData("_Retry", MessageBoxResult.Retry)]
	[InlineData("_Ignore", MessageBoxResult.Ignore)]
	[InlineData("_Try Again", MessageBoxResult.TryAgain)]
	[InlineData("_Continue", MessageBoxResult.Continue)]
	public void New_labels_map_to_results(string label, MessageBoxResult expected) =>
		Assert.Equal(expected, MessageBoxButtonMapping.ResultFromLabel(label));

	[Theory]
	[InlineData(MessageBoxButton.AbortRetryIgnore, 2, MessageBoxResult.Ignore)]
	[InlineData(MessageBoxButton.RetryCancel, 0, MessageBoxResult.Retry)]
	[InlineData(MessageBoxButton.CancelTryContinue, 1, MessageBoxResult.TryAgain)]
	[InlineData(MessageBoxButton.CancelTryContinue, 2, MessageBoxResult.Continue)]
	public void New_sets_map_clicks(MessageBoxButton button, int index, MessageBoxResult expected) =>
		Assert.Equal(expected, GtkMessageBoxPresenter.MapResponse(index, button));

	[Theory]
	[InlineData(MessageBoxButton.RetryCancel, MessageBoxResult.Cancel)]
	[InlineData(MessageBoxButton.CancelTryContinue, MessageBoxResult.Cancel)]
	[InlineData(MessageBoxButton.AbortRetryIgnore, MessageBoxResult.None)]
	public void New_sets_dismissal(MessageBoxButton button, MessageBoxResult expected) =>
		Assert.Equal(expected, MessageBoxButtonMapping.DefaultResult(button));
}
