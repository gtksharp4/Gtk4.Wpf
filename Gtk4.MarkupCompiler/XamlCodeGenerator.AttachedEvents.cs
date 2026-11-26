using System;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		enum Qualified { Attached, OwnMember, Emitted }

		/// <summary>
		/// Classifies an owner-qualified attribute (<c>Owner.Name</c>). A member of the element's own type or a base
		/// type becomes a plain member (<paramref name="name"/> is stripped). A routed event of another owner
		/// (<c>Button.Click</c> on a panel) is wired with <c>AddHandler</c>. Anything else is an attached property.
		/// </summary>
		Qualified ClassifyQualified(
			IndentedWriter w, string fqType, string target, string owner, ref string name, string handler)
		{
			if (!_resolver.HasReferences) return Qualified.Attached;
			string ownerFq = QualifyTypeName(owner);
			string member = name.Substring(owner.Length + 1);
			if ((ownerFq == fqType || _resolver.DerivesFrom(fqType, ownerFq))
				&& (_resolver.GetPropertyType(fqType, member) != null || IsKnownEvent(fqType, member)))
			{
				name = member;
				return Qualified.OwnMember;
			}
			if (!_resolver.TryGetRoutedEvent(ownerFq, member, out string handlerType)) return Qualified.Attached;
			w.Line($"{target}.AddHandler({Emitted(ownerFq)}.{member}Event, " +
				$"new {Emitted(handlerType)}(this.{handler}));");
			return Qualified.Emitted;
		}
	}
}
