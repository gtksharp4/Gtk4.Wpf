using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gtk4.Forms.Build.Tasks;

namespace Gtk4.Forms.Xaml
{
	interface ICompiledValueProvider
	{
		IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context);
	}
}