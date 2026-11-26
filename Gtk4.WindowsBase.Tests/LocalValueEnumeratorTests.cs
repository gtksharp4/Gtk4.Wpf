using System;
using System.Collections;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class LocalValueEnumeratorTests
{
	class Subject : DependencyObject
	{
		public static readonly DependencyProperty AProperty =
			DependencyProperty.Register("A", typeof(int), typeof(Subject), new PropertyMetadata(1));
		public static readonly DependencyProperty BProperty =
			DependencyProperty.Register("B", typeof(string), typeof(Subject), new PropertyMetadata("b"));
	}

	[Fact]
	public void EnumeratesOnlyLocalValues()
	{
		var s = new Subject();
		s.SetValue(Subject.AProperty, 7);
		var e = s.GetLocalValueEnumerator();
		Assert.Equal(1, e.Count);
		Assert.True(e.MoveNext());
		Assert.Same(Subject.AProperty, e.Current.Property);
		Assert.Equal(7, e.Current.Value);
		Assert.False(e.MoveNext());
	}

	[Fact]
	public void SnapshotIsNotAffectedByLaterChanges()
	{
		var s = new Subject();
		s.SetValue(Subject.AProperty, 7);
		var e = s.GetLocalValueEnumerator();
		s.SetValue(Subject.BProperty, "x");
		s.ClearValue(Subject.AProperty);
		Assert.Equal(1, e.Count);
		Assert.True(e.MoveNext());
		Assert.Equal(7, e.Current.Value);
	}

	[Fact]
	public void ResetRestartsAndCurrentThrowsOutOfRange()
	{
		var s = new Subject();
		s.SetValue(Subject.AProperty, 7);
		var e = s.GetLocalValueEnumerator();
		Assert.Throws<InvalidOperationException>(() => e.Current);
		e.MoveNext();
		e.MoveNext();
		Assert.Throws<InvalidOperationException>(() => e.Current);
		e.Reset();
		Assert.True(e.MoveNext());
		Assert.Equal(7, ((LocalValueEntry)((IEnumerator)e).Current).Value);
	}

	[Fact]
	public void ClearValueRoundTripRestoresDefaults()
	{
		var s = new Subject();
		s.SetValue(Subject.AProperty, 9);
		s.SetValue(Subject.BProperty, "z");
		var e = s.GetLocalValueEnumerator();
		while (e.MoveNext())
			s.ClearValue(e.Current.Property);
		Assert.Equal(0, s.GetLocalValueEnumerator().Count);
		Assert.Equal(1, s.GetValue(Subject.AProperty));
		Assert.Equal("b", s.GetValue(Subject.BProperty));
	}

	[Fact]
	public void EntryEquality()
	{
		var s = new Subject();
		s.SetValue(Subject.AProperty, 7);
		var e1 = s.GetLocalValueEnumerator();
		var e2 = s.GetLocalValueEnumerator();
		e1.MoveNext();
		e2.MoveNext();
		Assert.True(e1.Current == e2.Current);
		Assert.Equal(e1.Current.GetHashCode(), e2.Current.GetHashCode());
		s.SetValue(Subject.AProperty, 8);
		var e3 = s.GetLocalValueEnumerator();
		e3.MoveNext();
		Assert.True(e1.Current != e3.Current);
	}

	static readonly DependencyPropertyKey RoKey =
		DependencyProperty.RegisterReadOnly("Ro", typeof(int), typeof(Subject), new PropertyMetadata(0));

	[Fact]
	public void DependencyPropertyReadOnlyReflectsRegistration()
	{
		Assert.True(RoKey.DependencyProperty.ReadOnly);
		Assert.False(Subject.AProperty.ReadOnly);
	}
}
