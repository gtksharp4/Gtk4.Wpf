using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Runtime behaviour of the MarkupCompiler's root <c>BeginInit</c>/<c>EndInit</c> bracket: a XAML
/// root is initialized before its constructor returns, after its children (WPF order).
/// </summary>
public class GeneratedInitializeComponentTests
{
	/// <summary>Mirrors the generated <c>InitializeComponent</c> shape for a Window with one child.</summary>
	sealed class XamlWindow : WpfWindow
	{
		public readonly List<string> Log = new List<string>();
		public bool InitializedInsideCtor;
		bool _contentLoaded;

		public XamlWindow()
		{
			InitializeComponent();
			InitializedInsideCtor = IsInitialized;
		}

		void InitializeComponent()
		{
			if (_contentLoaded) return;
			_contentLoaded = true;
			if ((object)this is System.ComponentModel.ISupportInitialize __initBeginInit) __initBeginInit.BeginInit();
			this.Title = "Generated";
			var __e0 = new Border();
			__e0.Initialized += (_, _) => Log.Add("child");
			this.Content = __e0;
			if ((object)this is System.ComponentModel.ISupportInitialize __initEndInit) __initEndInit.EndInit();
		}

		protected override void OnInitialized(EventArgs e)
		{
			Log.Add("root:" + Title);
			base.OnInitialized(e);
		}
	}

	[Fact]
	public void Xaml_root_is_initialized_before_its_constructor_returns()
	{
		var win = new XamlWindow();

		Assert.True(win.InitializedInsideCtor);
		Assert.True(win.IsInitialized);
	}

	[Fact]
	public void Root_OnInitialized_runs_after_children_with_properties_set()
	{
		var win = new XamlWindow();

		Assert.Equal(new[] { "child", "root:Generated" }, win.Log);
	}
}
