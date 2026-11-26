using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>Records every peer event (in order) for the duration of a test.</summary>
sealed class EventLog : IDisposable
{
	static readonly AutomationEvents[] _all = (AutomationEvents[])Enum.GetValues(typeof(AutomationEvents));

	public readonly List<string> Entries = new List<string>();

	public EventLog()
	{
		foreach (var e in _all) AutomationPeer.AddListener(e);
		AutomationPeer.EventRaised += OnEvent;
		AutomationPeer.PropertyChangedRaised += OnProperty;
	}

	void OnEvent(AutomationPeer peer, AutomationEvents e)
	{
		var detail = e == AutomationEvents.StructureChanged ? ":" + AutomationPeer.PendingStructure?.Type : "";
		Entries.Add($"{e}{detail} {peer.GetName()}");
	}

	void OnProperty(AutomationPeer peer, AutomationProperty p, object? o, object? n) =>
		Entries.Add($"{p.ProgrammaticName} {peer.GetName()} {o}->{n}");

	public void Dispose()
	{
		AutomationPeer.EventRaised -= OnEvent;
		AutomationPeer.PropertyChangedRaised -= OnProperty;
		foreach (var e in _all) AutomationPeer.RemoveListener(e);
	}
}
