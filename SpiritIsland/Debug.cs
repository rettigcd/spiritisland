using System.Runtime.CompilerServices;

#nullable enable

namespace SpiritIsland;

public static class MyDebug {

	static public void Trace() { }

	//static public void Trace([CallerMemberName] string memberName = "") {
	//	Thread thread = Thread.CurrentThread;
	//	_history.Add( new Snapshot(memberName) );

	//	if(ActionScope.Current == null) {
	//	}
	//}

//	static public List<Snapshot> _history = [];

}


public class Snapshot(string memberName) {
	public string MemberName { get; } = memberName;

	public int ThreadId => _thread.ManagedThreadId;
	public bool IsPoolThread => _thread.IsThreadPoolThread;
	public bool IsBackgroundThread => _thread.IsBackground;

	public Guid ActionScopeId => ActionScope != null ? ActionScope.Id : Guid.Empty;

	readonly ActionScope? ActionScope = ActionScope.Current;
	readonly Thread _thread = Thread.CurrentThread;
}

#nullable disable