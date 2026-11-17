using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

[assembly: Dependency(typeof(MockDispatcherProvider))]
namespace Gtk4.Forms.Core.UnitTests;

public class MockDispatcherProvider : IDispatcherProvider
{
	public IDispatcher GetDispatcher(object context)
	{
		return new MockDispatcher();
	}
}