using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Keeps the generator's hard-coded struct set in step with the runtime converter registry.</summary>
public class ConverterBackedStructsTests
{
	[Fact]
	public void Set_mirrors_the_struct_converters_registered_by_RegisterBuiltIns()
	{
		string dir = Path.GetDirectoryName(typeof(ConverterBackedStructsTests).Assembly.Location)!;
		var pf = Assembly.LoadFrom(Path.Combine(dir, "Gtk4.PresentationFramework.dll"));
		Type converter = pf.GetType("System.Windows.Markup.XamlValueConverter", throwOnError: true)!;
		RuntimeHelpers.RunClassConstructor(converter.TypeHandle);

		var field = converter.GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static)!;
		var keys = ((IDictionary)field.GetValue(null)!).Keys.Cast<Type>();
		string[] registered = keys
			.Where(t => t.IsValueType && !t.IsPrimitive && t.FullName != "System.Decimal")
			.Select(t => t.FullName!)
			.OrderBy(n => n, StringComparer.Ordinal)
			.ToArray();
		string[] generator = XamlCodeGenerator.ConverterBackedStructs.OrderBy(n => n, StringComparer.Ordinal).ToArray();

		Assert.True(registered.SequenceEqual(generator),
			"The generator's ConverterBackedStructs must mirror XamlValueConverter.RegisterBuiltIns.\n" +
			"Registered: " + string.Join(", ", registered) + "\nGenerator:  " + string.Join(", ", generator));
	}
}
