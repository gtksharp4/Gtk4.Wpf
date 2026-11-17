using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Core.XamlC
{
	class PassthroughValueProvider : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			yield break;
		}
	}
}