using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Runtime hooks the generated code relies on (Round 45).</summary>
public class GeneratorHooksTests
{
	[Fact]
	public void AttachElementTriggers_is_public_and_hidden_from_IntelliSense()
	{
		MethodInfo m = typeof(FrameworkElement).GetMethod("AttachElementTriggers", BindingFlags.Public | BindingFlags.Instance)!;
		Assert.NotNull(m);
		var attr = m.GetCustomAttribute<EditorBrowsableAttribute>();
		Assert.Equal(EditorBrowsableState.Never, attr!.State);
	}

	[Theory]
	[InlineData("0,0 10,0 5,8", 3)]
	[InlineData("0 0 10 0", 2)]
	[InlineData(" 1.5,2.5 ", 1)]
	[InlineData("", 0)]
	public void PointCollection_converter_reads_xy_pairs(string raw, int count)
	{
		var pc = (PointCollection)XamlValueConverter.ConvertByType(typeof(PointCollection), raw)!;
		Assert.Equal(count, pc.Count);
	}

	[Fact]
	public void PointCollection_converter_reads_values_in_order()
	{
		var pc = (PointCollection)XamlValueConverter.ConvertByType(typeof(PointCollection), "1,2 3,4")!;
		Assert.Equal(new Point(3, 4), pc[1]);
	}

	[Fact]
	public void PointCollection_converter_rejects_an_odd_count_and_garbage()
	{
		Assert.Throws<InvalidOperationException>(
			() => XamlValueConverter.ConvertByType(typeof(PointCollection), "1,2 3"));
		Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(PointCollection), "1,,2 3 4"));
		Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(PointCollection), ",1 2"));
		Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(PointCollection), "1,2,"));
		Assert.Throws<FormatException>(() => XamlValueConverter.ConvertByType(typeof(PointCollection), "a,b"));
	}
}
