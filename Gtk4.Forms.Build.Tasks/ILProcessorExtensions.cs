using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Gtk4.Forms.Build.Tasks
{
	static class ILProcessorExtensions
	{
		public static void Append(this ILProcessor processor, IEnumerable<Instruction> instructions)
		{
			foreach (var instruction in instructions)
				processor.Append(instruction);
		}
	}
}