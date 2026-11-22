using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// Shared fixtures for the DataTrigger nested binding-path tests (E5 now supported):
/// INPC view-model types, a pure test element, a recording trigger action, and a
/// two-segment <c>Child.Name</c> trigger factory.
/// </summary>
public abstract class DataTriggerNestedPathTestBase
{
	protected sealed class Inner : INotifyPropertyChanged
	{
		string? _name;
		public string? Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	protected sealed class Outer : INotifyPropertyChanged
	{
		Inner? _child;
		public Inner? Child
		{
			get => _child;
			set { _child = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	protected sealed class Flat : INotifyPropertyChanged
	{
		bool _active;
		public bool Active
		{
			get => _active;
			set { _active = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	protected sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("DtNPTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public new string? Tag { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	protected sealed class RecordingAction : TriggerAction
	{
		public int Count { get; internal set; }
		internal override void Invoke(FrameworkElement target) => Count++;
	}

	protected static (TEl el, DataTrigger dt) Make2Seg(string value = "hit")
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var dt = new DataTrigger
		{
			Binding = new Binding("Child.Name"),
			Value = value
		};
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "hit" });
		style.Triggers.Add(dt);
		el.Style = style;
		return (el, dt);
	}
}
