using System;
using Xunit;

namespace MyApp
{
	public enum RuleMode { Fast, Slow }

	[Flags]
	public enum RuleFlags { None = 0, A = 1, B = 2 }

	public class TypedRule
	{
		public string Message { get; set; }
		public double Max { get; set; }
		public float Ratio { get; set; }
		public decimal Price { get; set; }
		public bool Strict { get; set; }
		public RuleMode Mode { get; set; }
		public RuleFlags Flags { get; set; }
	}
}

namespace Gtk4.MarkupCompiler.Tests
{
	/// <summary>Typed rule-attribute emission with a real reference assembly (the resolver finds the rule type).</summary>
	public class XamlCodeGeneratorRuleAttrTypedTests
	{
		static string Gen(string attrs)
		{
			var gen = new XamlCodeGenerator();
			gen.SetReferencePaths(new[] { typeof(MyApp.TypedRule).Assembly.Location });
			return gen.Generate($@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:local=""clr-namespace:MyApp"">
	<TextBox><TextBox.Text><Binding Path=""Age""><Binding.ValidationRules>
	<local:TypedRule {attrs}/></Binding.ValidationRules></Binding></TextBox.Text></TextBox></Window>", out _);
		}

		[Theory]
		[InlineData("Message=\"hi\"", ".Message = \"hi\";")]
		[InlineData("Max=\"2.5\"", ".Max = 2.5d;")]
		[InlineData("Max=\"NaN\"", ".Max = double.NaN;")]
		[InlineData("Max=\"-Infinity\"", ".Max = double.NegativeInfinity;")]
		[InlineData("Ratio=\"1.5\"", ".Ratio = 1.5f;")]
		[InlineData("Ratio=\"Infinity\"", ".Ratio = float.PositiveInfinity;")]
		[InlineData("Price=\"9.99\"", ".Price = 9.99m;")]
		[InlineData("Strict=\"True\"", ".Strict = true;")]
		[InlineData("Mode=\"Slow\"", ".Mode = global::MyApp.RuleMode.Slow;")]
		[InlineData("Flags=\"A, B\"", ".Flags = global::MyApp.RuleFlags.A | global::MyApp.RuleFlags.B;")]
		public void Typed_attribute_emits_a_valid_literal(string attrs, string expected)
			=> Assert.Contains(expected, Gen(attrs));
	}
}
