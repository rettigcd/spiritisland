using System;
using System.Collections.Generic;

namespace SpiritIsland.WinForms;

class StopWatch : IDisposable {
	readonly string label;
	readonly DateTime start;
	public StopWatch(string label ) { this.label = label; start = DateTime.Now; }
	public void Dispose() {
		var dur = DateTime.Now - start;
		var duration = new RecordedDuration(label,(int)dur.TotalMilliseconds);
		timeLog.Add( duration );
	}
	static public List<RecordedDuration> timeLog = new List<RecordedDuration>();
}

class RecordedDuration {
	public int ms;
	public string label;
	public RecordedDuration( string label, int ms ) { this.label = label; this.ms = ms; }
	public override string ToString() => $"{label}: {ms}ms";
}