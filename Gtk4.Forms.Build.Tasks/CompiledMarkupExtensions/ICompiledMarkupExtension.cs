using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Build.Tasks
{
	interface ICompiledMarkupExtension
	{
		IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef);
	}
}