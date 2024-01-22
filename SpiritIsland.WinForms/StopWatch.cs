using System;
using System.Collections.Generic;

namespace SpiritIsland.WinForms;

class StopWatch( string label ) : IDisposable {
	readonly string label = label;
	readonly DateTime start = DateTime.Now;

	public void Dispose() {
		var dur = DateTime.Now - start;
		var duration = new RecordedDuration(label,(int)dur.TotalMilliseconds);
		timeLog.Add( duration );
	}
	static public List<RecordedDuration> timeLog = [];
}

class RecordedDuration( string label, int ms ) {
	public int ms = ms;
	public string label = label;

	public override string ToString() => $"{label}: {ms}ms";
}