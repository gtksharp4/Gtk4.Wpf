using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>INotifyDataErrorInfo support: ErrorsChanged tracking and the ValidatesOn* flags.</summary>
public class BindingNotifyDataErrorsTests : BindingWriteBackTestBase
{
	sealed class NdVm : INotifyDataErrorInfo, INotifyPropertyChanged, IDataErrorInfo
	{
		readonly Dictionary<string, List<object>> _errors = new Dictionary<string, List<object>>();
		string _name = "n";

		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}

		public int Subscribers => ErrorsChanged?.GetInvocationList().Length ?? 0;
		public bool HasErrors => _errors.Count > 0;
		public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
		public event PropertyChangedEventHandler? PropertyChanged;
		public string Error => string.Empty;
		public string this[string column] => string.Empty;

		public IEnumerable GetErrors(string? propertyName)
			=> _errors.TryGetValue(propertyName ?? string.Empty, out var l) ? l : new List<object>();

		public void SetErrors(string prop, params object[] errors)
		{
			if (errors.Length == 0) _errors.Remove(prop);
			else _errors[prop] = errors.ToList();
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
		}
	}

	static WpfTextBox Bind(NdVm vm, Action<Binding>? configure = null)
	{
		var tb = new WpfTextBox { DataContext = vm };
		var b = new Binding("Name");
		configure?.Invoke(b);
		tb.SetBinding(WpfTextBox.TextProperty, b);
		return tb;
	}

	[Fact]
	public void ErrorsChanged_adds_and_removes_errors_without_a_write_back() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm);
		Assert.False(Validation.GetHasError(tb));
		vm.SetErrors("Name", "bad");
		Assert.True(Validation.GetHasError(tb));
		vm.SetErrors("Name");
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void Initial_errors_are_picked_up_on_activation() => Run(() =>
	{
		var vm = new NdVm();
		vm.SetErrors("Name", "bad");
		Assert.True(Validation.GetHasError(Bind(vm)));
	});

	[Fact]
	public void One_ValidationError_per_error_object() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm);
		var obj = new object();
		vm.SetErrors("Name", "a", obj);
		var errors = Validation.GetErrors(tb);
		Assert.Equal(2, errors.Count);
		Assert.All(errors, e => Assert.IsType<NotifyDataErrorValidationRule>(e.RuleInError));
		Assert.Equal("a", errors[0].ErrorContent);
		Assert.Same(obj, errors[1].ErrorContent);
	});

	[Fact]
	public void Errors_for_other_properties_are_ignored() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm);
		vm.SetErrors("Other", "x");
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void ValidatesOnNotifyDataErrors_false_ignores_the_source() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm, b => b.ValidatesOnNotifyDataErrors = false);
		vm.SetErrors("Name", "bad");
		Assert.False(Validation.GetHasError(tb));
		Assert.Equal(0, vm.Subscribers);
	});

	[Fact]
	public void ValidatesOnDataErrors_does_not_double_report_INotifyDataErrorInfo() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm, b =>
		{
			b.ValidatesOnDataErrors = true;
			b.ValidatesOnNotifyDataErrors = false;
			b.Mode = BindingMode.TwoWay;
		});
		vm.SetErrors("Name", "bad");
		tb.Text = "z";
		MoveFocusAway(tb);
		Assert.False(Validation.GetHasError(tb));

		var vm2 = new NdVm();
		var tb2 = Bind(vm2, b => { b.ValidatesOnDataErrors = true; b.Mode = BindingMode.TwoWay; });
		vm2.SetErrors("Name", "bad");
		tb2.Text = "z";
		MoveFocusAway(tb2);
		Assert.Single(Validation.GetErrors(tb2));
	});

	[Fact]
	public void Unsubscribes_on_ClearBinding() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm);
		Assert.Equal(1, vm.Subscribers);
		vm.SetErrors("Name", "bad");
		Assert.True(Validation.GetHasError(tb));
		BindingOperations.ClearBinding(tb, WpfTextBox.TextProperty);
		Assert.Equal(0, vm.Subscribers);
		Assert.False(Validation.GetHasError(tb));
		Assert.Equal(0, Validation.GetErrors(tb).Count);
	});

	[Fact]
	public void Unsubscribes_from_the_old_source_on_DataContext_swap() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm);
		var other = new NdVm();
		tb.DataContext = other;
		Assert.Equal(0, vm.Subscribers);
		Assert.Equal(1, other.Subscribers);
	});

	[Fact]
	public void Off_thread_ErrorsChanged_is_marshalled_to_the_dispatcher() => Run(() =>
	{
		var vm = new NdVm();
		var tb = Bind(vm);
		var queue = new List<Action>();
		var oldOn = Dispatcher.IsOnDispatcherThread;
		var oldBegin = Dispatcher.BeginInvokeAction;
		var oldPriority = Dispatcher.BeginInvokePriorityAction;
		Dispatcher.BeginInvokePriorityAction = null;
		Dispatcher.IsOnDispatcherThread = () => false;
		Dispatcher.BeginInvokeAction = queue.Add;
		try
		{
			vm.SetErrors("Name", "bad");
			Assert.False(Validation.GetHasError(tb));
			Assert.Single(queue);
		}
		finally
		{
			Dispatcher.IsOnDispatcherThread = oldOn;
			Dispatcher.BeginInvokeAction = oldBegin;
			Dispatcher.BeginInvokePriorityAction = oldPriority;
		}
		queue[0]();
		Assert.True(Validation.GetHasError(tb));
	});
}
