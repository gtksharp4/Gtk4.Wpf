using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XamlWriter skips bindings whose members it cannot write instead of writing them lossy.</summary>
public class Round51aRuntimeXamlWriterBindingTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static string Save(Binding binding)
	{
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, binding);
		return XamlWriter.Save(text);
	}

	[Fact]
	public void Plain_binding_is_still_written() => Run(() =>
		Assert.Contains("Text=\"{Binding Path=X}\"", Save(new Binding("X"))));

	[Fact]
	public void Binding_with_StringFormat_and_FallbackValue_is_skipped() => Run(() =>
		Assert.DoesNotContain("{Binding", Save(new Binding("X") { StringFormat = "{0:N2}", FallbackValue = "fb" })));

	[Fact]
	public void Binding_with_other_unwritten_members_is_skipped() => Run(() =>
	{
		Assert.DoesNotContain("{Binding", Save(new Binding("X") { TargetNullValue = "none" }));
		Assert.DoesNotContain("{Binding", Save(new Binding("X") { ValidatesOnExceptions = true }));
		Assert.DoesNotContain("{Binding", Save(new Binding("X") { ValidatesOnNotifyDataErrors = false }));
		Assert.DoesNotContain("{Binding", Save(new Binding("X") { NotifyOnSourceUpdated = true }));
		Assert.DoesNotContain("{Binding", Save(new Binding("X") { BindingGroupName = "g" }));
		var withRule = new Binding("X");
		withRule.ValidationRules.Add(new ExceptionValidationRule());
		Assert.DoesNotContain("{Binding", Save(withRule));
	});
}
