using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	// Returned by EvaluateExtension when the extension already wrote the value (DynamicResource).
	static readonly object Applied = new object();

	sealed class Member
	{
		public DependencyProperty? Dp;
		public PropertyInfo? Prop;
		public bool Attached;
		public Type ValueType => Prop?.PropertyType ?? Dp!.PropertyType;
		public bool CanSetClr => !Attached && Prop?.SetMethod != null && Prop.SetMethod.IsPublic;
		public bool CanSet => CanSetClr || Dp != null;
	}

	static Member? FindMember(Type type, string name)
	{
		PropertyInfo? prop = FindProperty(type, name);
		DependencyProperty? dp = FindDp(type, name);
		return prop == null && dp == null ? null : new Member { Prop = prop, Dp = dp };
	}

	static Member? FindAttached(Type owner, string name)
	{
		DependencyProperty? dp = FindDp(owner, name);
		return dp == null ? null : new Member { Dp = dp, Attached = true };
	}

	static PropertyInfo? FindProperty(Type type, string name)
	{
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
		try
		{
			PropertyInfo? p = type.GetProperty(name, flags);
			return p != null && p.GetIndexParameters().Length == 0 ? p : null;
		}
		catch (AmbiguousMatchException)
		{
			for (Type? t = type; t != null; t = t.BaseType)
				if (t.GetProperty(name, flags | BindingFlags.DeclaredOnly) is { } p) return p;
			return null;
		}
	}

	static DependencyProperty? FindDp(Type type, string name) =>
		type.GetField(name + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			?.GetValue(null) as DependencyProperty;

	static void Assign(object target, Member m, object? value)
	{
		if (value is Data.BindingBase binding && target is FrameworkElement fe && m.Dp != null)
		{
			fe.SetBinding(m.Dp, binding);
			return;
		}
		if (m.CanSetClr) m.Prop!.SetValue(target, Coerce(value, m.ValueType));
		else if (m.Dp != null && target is DependencyObject d) d.SetValue(m.Dp, Coerce(value, m.ValueType));
		else throw new InvalidOperationException("Property '" + (m.Prop?.Name ?? "?") + "' is read-only.");
	}

	static object? Coerce(object? value, Type type)
	{
		Type target = Nullable.GetUnderlyingType(type) ?? type;
		if (value == null)
		{
			if (type.IsValueType && target == type) throw new InvalidCastException("null is not valid for " + type.Name + ".");
			return null;
		}
		if (target.IsInstanceOfType(value)) return value;
		if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(target))
			return System.Convert.ChangeType(value, target, System.Globalization.CultureInfo.InvariantCulture);
		throw new InvalidCastException("Cannot assign '" + value.GetType().Name + "' to " + type.Name + ".");
	}

	object? ConvertString(Type type, string raw, DependencyProperty? dp)
	{
		if (type == typeof(string) || type == typeof(object)) return raw;
		if (type == typeof(Type)) return ResolveTypeName(raw.Trim(), _ctx);
		if (type == typeof(RoutedEvent)) return ResolveRoutedEvent(raw.Trim());
		if (type == typeof(System.Windows.Input.ICommand) && raw.IndexOf(':') > 0)
			return CommandConverters.ParseCommand(raw, n => ResolveTypeName(n, _ctx));
		object? value = dp != null
			? XamlValueConverter.Convert(dp.OwnerType, dp, raw)
			: XamlValueConverter.ConvertByType(type, raw);
		Type target = Nullable.GetUnderlyingType(type) ?? type;
		if (value == null || target.IsInstanceOfType(value)) return value;
		TypeConverter converter = TypeDescriptor.GetConverter(target);
		if (converter.CanConvertFrom(typeof(string)))
			return converter.ConvertFromInvariantString(raw);
		throw new FormatException("Cannot convert '" + raw + "' to " + type.Name + ".");
	}
}
