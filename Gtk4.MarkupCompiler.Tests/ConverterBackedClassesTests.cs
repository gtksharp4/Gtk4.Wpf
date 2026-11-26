using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class ConverterBackedClassesTests
{
	[Fact]
	public void Every_class_in_the_set_has_a_registered_runtime_converter()
	{
		string dir = Path.GetDirectoryName(typeof(ConverterBackedClassesTests).Assembly.Location)!;
		var pf = Assembly.LoadFrom(Path.Combine(dir, "Gtk4.PresentationFramework.dll"));
		var converter = pf.GetType("System.Windows.Markup.XamlValueConverter", throwOnError: true)!;
		RuntimeHelpers.RunClassConstructor(converter.TypeHandle);
		var keys = ((IDictionary)converter.GetField("_converters", BindingFlags.NonPublic | BindingFlags.Static)!
			.GetValue(null)!).Keys.Cast<System.Type>().Select(t => t.FullName).ToList();
		foreach (string name in XamlCodeGenerator.ConverterBackedClasses)
			Assert.Contains(name, keys);
	}
}
