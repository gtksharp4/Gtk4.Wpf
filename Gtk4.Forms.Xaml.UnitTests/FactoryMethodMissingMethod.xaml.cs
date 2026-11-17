using System;
using Xunit;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class FactoryMethodMissingMethod : MockView
{
	public FactoryMethodMissingMethod()
	{
		InitializeComponent();
	}

	public FactoryMethodMissingMethod(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		public Tests()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Throw(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				new BuildExceptionConstraint(8, 4).Verify(() => MockCompiler.Compile(typeof(FactoryMethodMissingMethod)));
			else
				Assert.Throws<MissingMemberException>(() => new FactoryMethodMissingMethod(useCompiledXaml));
		}
	}
}