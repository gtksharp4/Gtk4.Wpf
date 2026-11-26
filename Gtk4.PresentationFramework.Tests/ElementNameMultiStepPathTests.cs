using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A multi-step path on an element source keeps following every step, in InitializeComponent order.</summary>
public class ElementNameMultiStepPathTests : WpfGtkTestBase
{
	sealed class Holder : INotifyPropertyChanged
	{
		FillRule _rule;
		public event PropertyChangedEventHandler? PropertyChanged;
		public FillRule Rule
		{
			get => _rule;
			set { _rule = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rule))); }
		}
	}

	[Fact]
	public void Tag_Rule_follows_a_replaced_Tag_and_changes_of_the_current_Tag() => Run(() =>
	{
		var root = new StackPanel();
		var b0 = new Border { Tag = new Holder { Rule = FillRule.Nonzero } };
		var target = new TextBlock();
		target.SetBinding(TextBlock.TextProperty, new Binding("Tag.Rule") { ElementName = "b0" });
		root.Children.Add(b0);
		root.Children.Add(target);
		root.RegisterName("b0", b0);
		Assert.Equal("Nonzero", target.Text);

		var replacement = new Holder { Rule = FillRule.EvenOdd };
		b0.Tag = replacement;
		Assert.Equal("EvenOdd", target.Text);

		replacement.Rule = FillRule.Nonzero;
		Assert.Equal("Nonzero", target.Text);
	});

	[Fact]
	public void The_old_Tag_no_longer_drives_the_target_after_it_is_replaced() => Run(() =>
	{
		var root = new StackPanel();
		var old = new Holder { Rule = FillRule.Nonzero };
		var b0 = new Border { Tag = old };
		var target = new TextBlock();
		root.Children.Add(b0);
		root.Children.Add(target);
		root.RegisterName("b0", b0);
		target.SetBinding(TextBlock.TextProperty, new Binding("Tag.Rule") { ElementName = "b0" });
		b0.Tag = new Holder { Rule = FillRule.EvenOdd };
		old.Rule = FillRule.EvenOdd;
		old.Rule = FillRule.Nonzero;
		Assert.Equal("EvenOdd", target.Text);
	});
}
