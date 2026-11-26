using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// VisualStateManager XAML emit routines for <see cref="XamlCodeGenerator"/>.
	/// Handles the attached-property element form:
	/// <code>
	/// &lt;Grid&gt;
	///   &lt;VisualStateManager.VisualStateGroups&gt;
	///     &lt;VisualStateGroup x:Name="CommonStates"&gt;
	///       &lt;VisualState x:Name="Normal"/&gt;
	///       &lt;VisualState x:Name="MouseOver"&gt;
	///         &lt;Storyboard&gt;…&lt;/Storyboard&gt;
	///       &lt;/VisualState&gt;
	///     &lt;/VisualStateGroup&gt;
	///   &lt;/VisualStateManager.VisualStateGroups&gt;
	/// &lt;/Grid&gt;
	/// </code>
	/// </summary>
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Emits code for a <c>&lt;VisualStateManager.VisualStateGroups&gt;</c> property element:
		/// constructs each <see cref="System.Windows.VisualStateGroup"/> with its
		/// <see cref="System.Windows.VisualState"/>s (and optional Storyboards), then attaches
		/// the collection to <paramref name="parentVar"/> via
		/// <c>System.Windows.VisualStateManager.SetVisualStateGroups</c>.
		/// </summary>
		/// <param name="w">Output writer.</param>
		/// <param name="propEl">
		/// The <c>&lt;VisualStateManager.VisualStateGroups&gt;</c> XElement.
		/// </param>
		/// <param name="xNs">The x: namespace (for <c>x:Name</c> attributes).</param>
		/// <param name="parentVar">C# variable name of the element being decorated.</param>
		/// <param name="counter">Shared variable counter (passed by ref).</param>
		internal void EmitVisualStateGroups(
			IndentedWriter w,
			XElement propEl,
			XNamespace xNs,
			string parentVar,
			ref int counter)
		{
			string listVar = $"__vsgList{counter++}";
			w.Line($"var {listVar} = new System.Collections.Generic.List<System.Windows.VisualStateGroup>();");

			foreach (XElement groupEl in propEl.Elements())
			{
				if (groupEl.Name.LocalName != "VisualStateGroup") continue;

				string groupVar = $"__vsg{counter++}";
				w.Line($"var {groupVar} = new System.Windows.VisualStateGroup();");

				// x:Name sets both the Name property and the named field.
				string groupName = (string)(groupEl.Attribute(xNs + "Name") ?? groupEl.Attribute("Name"));
				if (groupName != null)
				{
					w.Line($"{groupVar}.Name = \"{EscapeString(groupName)}\";");
					// Also assign to the generated field if one exists for it.
					if (_fieldElements.Contains(groupEl)) w.Line($"this.{groupName} = {groupVar};");
				}

				foreach (XElement stateEl in groupEl.Elements())
				{
					if (stateEl.Name.LocalName != "VisualState") continue;

					string stateVar = $"__vs{counter++}";
					w.Line($"var {stateVar} = new System.Windows.VisualState();");

					string stateName = (string)(stateEl.Attribute(xNs + "Name") ?? stateEl.Attribute("Name"));
					if (stateName != null)
					{
						w.Line($"{stateVar}.Name = \"{EscapeString(stateName)}\";");
						// Assign to field if declared.
						if (_fieldElements.Contains(stateEl)) w.Line($"this.{stateName} = {stateVar};");
					}

					// Optional <Storyboard> child (direct child or inside <VisualState.Storyboard>).
					XElement sbEl = FindStoryboardChild(stateEl);
					if (sbEl != null)
					{
						string sbVar = EmitStoryboardInline(w, sbEl, xNs, ref counter);
						w.Line($"{stateVar}.Storyboard = {sbVar};");
					}

					w.Line($"{groupVar}.States.Add({stateVar});");
				}

				w.Line($"{listVar}.Add({groupVar});");
			}

			w.Line($"System.Windows.VisualStateManager.SetVisualStateGroups({parentVar}, {listVar});");
		}

		/// <summary>
		/// Finds the <c>&lt;Storyboard&gt;</c> element that is either a direct child of
		/// <paramref name="stateEl"/> or the single child of a
		/// <c>&lt;VisualState.Storyboard&gt;</c> property element.
		/// Returns <c>null</c> when no storyboard is present.
		/// </summary>
		static XElement FindStoryboardChild(XElement stateEl)
		{
			foreach (XElement ch in stateEl.Elements())
			{
				// Direct <Storyboard> child.
				if (ch.Name.LocalName == "Storyboard"
					&& ch.Name.NamespaceName == WpfXmlnsMap.PresentationNs)
					return ch;

				// <VisualState.Storyboard> property-element wrapper.
				if (ch.Name.LocalName == "VisualState.Storyboard")
				{
					foreach (XElement inner in ch.Elements())
					{
						if (inner.Name.LocalName == "Storyboard"
							&& inner.Name.NamespaceName == WpfXmlnsMap.PresentationNs)
							return inner;
					}
				}
			}
			return null;
		}
	}
}
