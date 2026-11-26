using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// Element-level trigger emit routines for <see cref="XamlCodeGenerator"/>.
	/// Handles the property-element form declared on a FrameworkElement (not inside a Style):
	/// <code>
	/// &lt;Button&gt;
	///   &lt;Button.Triggers&gt;
	///     &lt;EventTrigger RoutedEvent="Button.Click"&gt;…&lt;/EventTrigger&gt;
	///     &lt;Trigger Property="IsMouseOver" Value="True"&gt;…&lt;/Trigger&gt;
	///   &lt;/Button.Triggers&gt;
	/// &lt;/Button&gt;
	/// </code>
	/// Any <c>&lt;{TypeName}.Triggers&gt;</c> property element is recognised as an element-level
	/// trigger block; <c>&lt;FrameworkElement.Triggers&gt;</c> is the canonical form.
	/// </summary>
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Handles <c>&lt;{owner}.Triggers&gt;</c> property-element children: constructs
		/// each trigger and adds it to <paramref name="parentVar"/>.Triggers, then emits
		/// <c>AttachElementTriggers()</c> so the triggers are live immediately.
		/// </summary>
		/// <param name="w">Output writer.</param>
		/// <param name="triggersEl">The <c>&lt;*.Triggers&gt;</c> XElement.</param>
		/// <param name="xNs">The x: namespace.</param>
		/// <param name="parentVar">C# variable name of the element that owns the Triggers collection.</param>
		/// <param name="counter">Shared variable counter (passed by ref).</param>
		internal void EmitElementTriggers(
			IndentedWriter w,
			XElement triggersEl,
			XNamespace xNs,
			string parentVar,
			ref int counter)
		{
			bool any = false;
			foreach (XElement ch in triggersEl.Elements())
			{
				string trigVar = $"__t{counter++}";
				switch (ch.Name.LocalName)
				{
					case "Trigger":
						EmitTriggerToCollection(w, ch, trigVar, parentVar, parentVar);
						any = true;
						break;
					case "DataTrigger":
						EmitDataTriggerToCollection(w, ch, trigVar, parentVar, parentVar);
						any = true;
						break;
					case "MultiTrigger":
						EmitMultiTriggerToCollection(w, ch, xNs, trigVar, parentVar, parentVar, ref counter);
						any = true;
						break;
					case "EventTrigger":
						EmitEventTriggerFull(w, ch, xNs, trigVar, parentVar, false, ref counter);
						any = true;
						break;
				}
			}

			// Attach after all triggers have been added.
			if (any)
				w.Line($"{parentVar}.AttachElementTriggers();");
		}
	}
}
