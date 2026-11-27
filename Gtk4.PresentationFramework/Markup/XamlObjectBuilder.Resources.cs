using System;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	// Innermost object under construction first, then the application.
	bool TryLookup(object key, out object? value)
	{
		for (int i = _stack.Count - 1; i >= 0; i--)
		{
			if (_stack[i] is FrameworkElement fe && fe.Resources.TryGetValue(key, out value)) return true;
			if (_stack[i] is ResourceDictionary rd && rd.TryGetValue(key, out value)) return true;
		}
		Application? app = Application.Current;
		if (app != null && app.Resources.TryGetValue(key, out value)) return true;
		value = app?.TryFindResource(key);
		return value != null;
	}

	object? LookupStatic(object key) =>
		TryLookup(key, out object? value)
			? value
			: throw new InvalidOperationException("Cannot find resource named '" + key + "'.");

	IServiceProvider Provider(XElement scope, object? target, object? member) =>
		new BuilderServices(this, scope, target, member);

	sealed class BuilderServices : IServiceProvider, IProvideValueTarget, IXamlTypeResolver
	{
		readonly XamlObjectBuilder _builder;
		readonly XElement _scope;

		public BuilderServices(XamlObjectBuilder builder, XElement scope, object? target, object? member)
		{
			_builder = builder;
			_scope = scope;
			TargetObject = target;
			TargetProperty = member;
		}

		public object? TargetObject { get; }

		public object? TargetProperty { get; }

		public Type Resolve(string qualifiedTypeName) => _builder.ResolveTypeName(qualifiedTypeName, _scope);

		public object? GetService(Type serviceType) =>
			serviceType == typeof(IProvideValueTarget) || serviceType == typeof(IXamlTypeResolver) ? this
			: serviceType == typeof(IUriContext) ? new UriContext(_builder.BaseUri) : null;

		sealed class UriContext : IUriContext
		{
			public UriContext(Uri? baseUri) => BaseUri = baseUri;

			public Uri? BaseUri { get; set; }
		}
	}
}
