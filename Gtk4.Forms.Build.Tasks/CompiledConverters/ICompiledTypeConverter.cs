using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Xaml
{
	interface ICompiledTypeConverter
	{
		IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node);
	}
}

namespace Gtk4.Forms.Core.XamlC
{
	//only used in unit tests to make sure the compiled InitializeComponent is invoked
	class IsCompiledTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			if (value != "IsCompiled?")
				throw new Exception();
			yield return Instruction.Create(OpCodes.Ldc_I4_1);
		}
	}
}