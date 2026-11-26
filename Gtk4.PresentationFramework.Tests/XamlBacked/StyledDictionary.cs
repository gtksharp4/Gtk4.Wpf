using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

public class TagConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => "tag:" + value;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}

/// <summary>A ResourceDictionary root whose Style consumes an earlier key of the same file.</summary>
public partial class StyledDictionary : ResourceDictionary
{
	public StyledDictionary() => InitializeComponent();
}
