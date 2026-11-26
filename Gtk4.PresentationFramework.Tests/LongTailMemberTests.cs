using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Text, menu, window and dialog long-tail members used by official samples.</summary>
public class LongTailMemberTests : WpfGtkTestBase
{
	[Fact]
	public void MenuItem_ItemsSource_round_trips_through_its_property() => Run(() =>
	{
		var item = new MenuItem();
		Assert.Null(item.ItemsSource);
		var list = new List<string> { "a" };
		item.SetValue(MenuItem.ItemsSourceProperty, list);
		Assert.Same(list, item.ItemsSource);
		Assert.Equal(typeof(IEnumerable), MenuItem.ItemsSourceProperty.PropertyType);
	});

	[Fact]
	public void TextBox_SpellCheck_is_a_stable_view_over_the_attached_values() => Run(() =>
	{
		var tb = new TextBox();
		Assert.Same(tb.SpellCheck, tb.SpellCheck);
		Assert.False(tb.SpellCheck.IsEnabled);
		tb.SpellCheck.IsEnabled = true;
		Assert.True(SpellCheck.GetIsEnabled(tb));
		SpellCheck.SetSpellingReform(tb, SpellingReform.Prereform);
		Assert.Equal(SpellingReform.Prereform, tb.SpellCheck.SpellingReform);
		Assert.Same(SpellCheck.GetCustomDictionaries(tb), tb.SpellCheck.CustomDictionaries);
		Assert.NotSame(tb.SpellCheck, new TextBox().SpellCheck);
		Assert.True(new RichTextBox().SpellCheck != null);
	});

	[Fact]
	public void TextBox_has_no_spelling_errors() => Run(() =>
	{
		var tb = new TextBox { Text = "mispeled" };
		Assert.Null(tb.GetSpellingError(0));
		Assert.Equal(-1, tb.GetSpellingErrorStart(0));
		Assert.Equal(0, tb.GetSpellingErrorLength(0));
	});

	[Fact]
	public void TextBox_Copy_puts_the_selection_on_the_clipboard() => Run(() =>
	{
		Clipboard.Clear();
		var tb = new TextBox { Text = "hello world" };
		tb.Copy();
		Assert.False(Clipboard.ContainsText());
		tb.Select(6, 5);
		tb.Copy();
		Assert.Equal("world", Clipboard.GetText());
		Assert.Equal("hello world", tb.Text);
	});

	[Fact]
	public void TextBox_Cut_removes_the_selection_and_copies_it() => Run(() =>
	{
		var tb = new TextBox { Text = "hello world" };
		tb.Select(5, 6);
		tb.Cut();
		Assert.Equal("hello", tb.Text);
		Assert.Equal(" world", Clipboard.GetText());
		Assert.Equal(0, tb.SelectionLength);
	});

	[Fact]
	public void TextBox_Paste_replaces_the_selection_and_respects_IsReadOnly() => Run(() =>
	{
		Clipboard.SetText("X");
		var tb = new TextBox { Text = "abc" };
		tb.Select(1, 1);
		tb.Paste();
		Assert.Equal("aXc", tb.Text);
		var ro = new TextBox { Text = "abc", IsReadOnly = true };
		ro.Select(0, 3);
		ro.Paste();
		ro.Cut();
		Assert.Equal("abc", ro.Text);
	});

	[Fact]
	public void TextBox_SelectAll_selects_everything() => Run(() =>
	{
		var tb = new TextBox { Text = "abc" };
		tb.SelectAll();
		Assert.Equal("abc", tb.SelectedText);
	});

	[Fact]
	public void TextElement_Background_is_a_plain_instance_property() => Run(() =>
	{
		var run = new Run();
		Assert.Null(run.Background);
		run.Background = Brushes.Red;
		Assert.Equal(Colors.Red, ((SolidColorBrush)run.Background!).Color);
		Assert.Equal(typeof(Brush), TextElement.BackgroundProperty.PropertyType);
		Assert.Null(typeof(TextElement).GetMethod(
			"GetBackground", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
	});

	[Fact]
	public void RoutedPropertyChangedEventArgs_carries_old_and_new_values() => Run(() =>
	{
		var a = new RoutedPropertyChangedEventArgs<double>(1, 2);
		Assert.Equal(1, a.OldValue);
		Assert.Equal(2, a.NewValue);
		Assert.IsAssignableFrom<RoutedEventArgs>(a);
		var evt = EventManager.RegisterRoutedEvent("LongTailChanged", RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<double>), typeof(LongTailMemberTests));
		var b = new RoutedPropertyChangedEventArgs<double>(3, 4, evt);
		Assert.Same(evt, b.RoutedEvent);
		RoutedPropertyChangedEventHandler<double> h = (s, e) => Assert.Equal(4, e.NewValue);
		h(this, b);
	});

	[Fact]
	public void Window_ShowActivated_defaults_true_and_is_settable() => Run(() =>
	{
		var w = new Window();
		Assert.True(w.ShowActivated);
		Assert.Equal(true, w.GetValue(Window.ShowActivatedProperty));
		w.ShowActivated = false;
		Assert.False(w.ShowActivated);
		Assert.Equal(false, w.GetValue(Window.ShowActivatedProperty));
		w.SetValue(Window.ShowActivatedProperty, true);
		Assert.True(w.ShowActivated);
	});

	[Fact]
	public void MessageBoxOptions_matches_the_Win32_flag_values()
	{
		Assert.Equal(0, (int)MessageBoxOptions.None);
		Assert.Equal(0x200000, (int)MessageBoxOptions.ServiceNotification);
		Assert.Equal(0x20000, (int)MessageBoxOptions.DefaultDesktopOnly);
		Assert.Equal(0x80000, (int)MessageBoxOptions.RightAlign);
		Assert.Equal(0x100000, (int)MessageBoxOptions.RtlReading);
		Assert.NotNull(typeof(MessageBoxOptions).GetCustomAttributes(typeof(FlagsAttribute), false));
		Assert.Single(typeof(MessageBoxOptions).GetCustomAttributes(typeof(FlagsAttribute), false));
	}

	[Fact]
	public void MessageBox_overloads_with_owner_default_result_and_options_reach_the_presenter() => Run(() =>
	{
		var original = MessageBox.Presenter;
		var fake = new RecordingPresenter();
		MessageBox.Presenter = fake;
		try
		{
			var w = new Window();
			Assert.Equal(MessageBoxResult.Yes, MessageBox.Show(w, "t", "c", MessageBoxButton.YesNo,
				MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.RightAlign));
			Assert.Equal(("t", "c", MessageBoxButton.YesNo, MessageBoxImage.Question), fake.Last);
			MessageBox.Show("a", "b", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
				MessageBoxOptions.None);
			Assert.Equal(("a", "b", MessageBoxButton.OK, MessageBoxImage.Error), fake.Last);
			MessageBox.Show(w, "only");
			Assert.Equal(("only", string.Empty, MessageBoxButton.OK, MessageBoxImage.None), fake.Last);
			Assert.Throws<ArgumentNullException>(() => MessageBox.Show((Window)null!, "x"));
			Assert.Throws<ArgumentNullException>(() => MessageBox.Show(null!, "x", "y", MessageBoxButton.OK,
				MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None));
		}
		finally { MessageBox.Presenter = original; }
	});

	sealed class RecordingPresenter : IMessageBoxPresenter
	{
		public (string, string, MessageBoxButton, MessageBoxImage) Last;

		public MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
		{
			Last = (messageText, caption, button, icon);
			return MessageBoxResult.Yes;
		}
	}
}
