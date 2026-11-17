using System.Collections.Generic;
using System.Globalization;
using Mono.Cecil.Cil;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Xaml;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Gtk4.Forms.Core.XamlC
{
	class ConstraintTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			double size;

			if (string.IsNullOrEmpty(value) || !double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
				throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Constraint));

			yield return Create(Ldc_R8, size);
			yield return Create(Call, module.ImportMethodReference(("Gtk4.Forms.Core", "Gtk4.Forms", "Constraint"),
																   methodName: "Constant",
																   parameterTypes: new[] { ("mscorlib", "System", "Double") },
																   isStatic: true));
		}
	}
}
