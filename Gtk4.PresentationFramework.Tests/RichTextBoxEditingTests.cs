using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for the <see cref="RichTextBox"/> editing API (events, input options, undo state)
/// that need no GTK loop.
/// </summary>
public class RichTextBoxEditingTests
{
	// ── TextChanged ─────────────────────────────────────────────────────────────

	[Fact]
	public void AppendText_raises_TextChanged_once()
	{
		var rtb = new RichTextBox();
		int raised = 0;
		rtb.TextChanged += (_, _) => raised++;

		rtb.AppendText("hello");

		Assert.Equal(1, raised);
	}

	[Fact]
	public void Setting_Selection_Text_raises_TextChanged()
	{
		var rtb = new RichTextBox();
		int raised = 0;
		rtb.TextChanged += (_, _) => raised++;

		rtb.Selection.Text = "replaced";

		Assert.Equal(1, raised);
	}

	// ── TextRange.IsEmpty ───────────────────────────────────────────────────────

	[Fact]
	public void Selection_IsEmpty_true_for_empty_document()
	{
		var rtb = new RichTextBox();
		Assert.True(rtb.Selection.IsEmpty);
	}

	[Fact]
	public void Selection_IsEmpty_false_with_text()
	{
		var rtb = new RichTextBox();
		rtb.AppendText("x");
		Assert.False(rtb.Selection.IsEmpty);
	}

	// ── Input options ───────────────────────────────────────────────────────────

	[Fact]
	public void AcceptsTab_defaults_false_and_round_trips()
	{
		var rtb = new RichTextBox();
		Assert.False(rtb.AcceptsTab);
		rtb.AcceptsTab = true;
		Assert.True(rtb.AcceptsTab);
	}

	[Fact]
	public void AcceptsReturn_defaults_true_and_round_trips()
	{
		var rtb = new RichTextBox();
		Assert.True(rtb.AcceptsReturn);
		rtb.AcceptsReturn = false;
		Assert.False(rtb.AcceptsReturn);
	}

	[Fact]
	public void IsUndoEnabled_defaults_true_and_round_trips()
	{
		var rtb = new RichTextBox();
		Assert.True(rtb.IsUndoEnabled);
		rtb.IsUndoEnabled = false;
		Assert.False(rtb.IsUndoEnabled);
	}

	// ── Undo state without a materialised widget ────────────────────────────────

	[Fact]
	public void CanUndo_CanRedo_false_without_widget()
	{
		var rtb = new RichTextBox();
		Assert.False(rtb.CanUndo);
		Assert.False(rtb.CanRedo);
	}
}
