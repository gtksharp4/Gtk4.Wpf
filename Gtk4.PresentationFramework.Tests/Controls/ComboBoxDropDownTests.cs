using System;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Spy : ComboBox
{
	public int Opened, Closed;
	protected override void OnDropDownOpened(EventArgs e) { Opened++; base.OnDropDownOpened(e); }
	protected override void OnDropDownClosed(EventArgs e) { Closed++; base.OnDropDownClosed(e); }
}

public class ComboBoxDropDownTests
{
	[Fact]
	public void Open_raises_once()
	{
		var cb = new Spy { IsDropDownOpen = true };
		Assert.Equal(1, cb.Opened);
		Assert.Equal(0, cb.Closed);
	}

	[Fact]
	public void Close_raises_once()
	{
		var cb = new Spy { IsDropDownOpen = true };
		cb.IsDropDownOpen = false;
		Assert.Equal(1, cb.Opened);
		Assert.Equal(1, cb.Closed);
	}

	[Fact]
	public void Same_value_raises_nothing()
	{
		var cb = new Spy { IsDropDownOpen = false };
		Assert.Equal(0, cb.Opened + cb.Closed);
	}

	[Fact]
	public void Public_events_fire()
	{
		var cb = new ComboBox();
		int o = 0, c = 0;
		cb.DropDownOpened += (_, _) => o++;
		cb.DropDownClosed += (_, _) => c++;
		cb.IsDropDownOpen = true;
		cb.IsDropDownOpen = false;
		Assert.Equal((1, 1), (o, c));
	}

	[Fact]
	public void MaxDropDownHeight_default_and_store()
	{
		var cb = new ComboBox();
		Assert.Equal(ComboBox.DefaultMaxDropDownHeight, cb.MaxDropDownHeight);
		cb.MaxDropDownHeight = 100;
		Assert.Equal(100, cb.MaxDropDownHeight);
	}
}
