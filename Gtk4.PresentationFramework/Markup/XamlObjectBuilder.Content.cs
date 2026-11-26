using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	// A child element, or a run of text between elements.
	readonly struct Node
	{
		public readonly XElement? Element;
		public readonly string? Text;
		public Node(XElement? element, string? text) { Element = element; Text = text; }
	}

	void ApplyChildren(XElement el, object instance)
	{
		var content = new List<Node>();
		var text = new System.Text.StringBuilder();
		foreach (XNode node in el.Nodes())
		{
			if (node is XText t) { text.Append(t.Value); continue; }
			if (node is not XElement child) continue;
			FlushText(text, content);
			if (child.Name.LocalName.IndexOf('.') >= 0) ApplyPropertyElement(child, instance);
			else content.Add(new Node(child, null));
		}
		FlushText(text, content);
		if (content.Count > 0) ApplyContent(el, instance, content);
	}

	static void FlushText(System.Text.StringBuilder text, List<Node> content)
	{
		string value = Normalize(text.ToString());
		text.Clear();
		if (value.Length > 0) content.Add(new Node(null, value));
	}

	void ApplyPropertyElement(XElement pe, object instance)
	{
		string local = pe.Name.LocalName;
		int dot = local.IndexOf('.');
		Type owner = ResolveType(pe.Name.NamespaceName, local.Substring(0, dot), pe);
		string name = local.Substring(dot + 1);
		Type type = instance.GetType();
		Member member = (owner.IsAssignableFrom(type) ? FindMember(type, name) : FindAttached(owner, name))
			?? throw Fail(pe, "Cannot find property '" + name + "' in type '" + owner.Name + "'.");
		var nodes = new List<Node>();
		var text = new System.Text.StringBuilder();
		foreach (XNode node in pe.Nodes())
		{
			if (node is XText t) text.Append(t.Value);
			else if (node is XElement child) { FlushText(text, nodes); nodes.Add(new Node(child, null)); }
		}
		FlushText(text, nodes);
		if (nodes.Count == 0) return;
		XElement saved = _ctx;
		_ctx = pe;
		try { Fill(instance, member, nodes, pe); }
		catch (XamlParseException) { throw; }
		catch (Exception ex) { throw Fail(pe, ex.Message, Unwrap(ex)); }
		finally { _ctx = saved; }
	}

	void ApplyContent(XElement el, object instance, List<Node> nodes)
	{
		string? name = ContentPropertyName(instance);
		if (name == null && TryApplyTemplateContent(el, instance, nodes)) return;
		if (name == null && (instance is ResourceDictionary || instance is System.Collections.IList))
		{
			foreach (Node n in nodes) AddItem(instance, n, instance, el);
			return;
		}
		if (name == null && instance is IAddChild addChild)
		{
			foreach (Node n in nodes)
			{
				if (n.Text != null) addChild.AddText(n.Text);
				else if (BuildItem(n, instance, null) is { } child) addChild.AddChild(child);
			}
			return;
		}
		if (name == null) throw Fail(el, "'" + instance.GetType().Name + "' does not accept content.");
		Member member = FindMember(instance.GetType(), name)
			?? throw Fail(el, "Content property '" + name + "' not found.");
		Fill(instance, member, nodes, el);
	}

	static string? ContentPropertyName(object instance) =>
		instance.GetType().GetCustomAttribute<ContentPropertyAttribute>(true)?.Name
		?? FallbackContentProperty(instance.GetType());

	void Fill(object instance, Member member, List<Node> nodes, XElement source)
	{
		object? existing = GetValue(instance, member);
		if (existing == null || !IsCollection(existing))
		{
			if (nodes.Count != 1) throw Fail(source, "'" + member.Prop?.Name + "' accepts a single value.");
			Assign(instance, member, BuildItem(nodes[0], instance, member, member.ValueType));
			return;
		}
		if (nodes.Count == 1)
		{
			object? single = BuildItem(nodes[0], instance, member);
			if (single != null && member.ValueType.IsInstanceOfType(single) && member.CanSet && !member.Attached)
			{
				Assign(instance, member, single);
				return;
			}
			AddValue(existing, single, nodes[0], source);
			return;
		}
		foreach (Node n in nodes) AddItem(existing, n, instance, source, member);
	}

	static object? GetValue(object instance, Member m)
	{
		if (m.Prop?.GetMethod != null && !m.Attached) return m.Prop.GetValue(instance);
		return m.Dp != null && instance is DependencyObject d ? d.GetValue(m.Dp) : null;
	}

	void AddItem(object collection, Node node, object owner, XElement source, Member? member = null) =>
		AddValue(collection, BuildItem(node, owner, member), node, source);

	object? BuildItem(Node node, object owner, Member? member, Type? wanted = null)
	{
		if (node.Element != null) return BuildElement(node.Element, owner, (object?)member?.Dp ?? member?.Prop);
		return wanted == null ? node.Text : ConvertString(wanted, node.Text!, member?.Dp);
	}
}
