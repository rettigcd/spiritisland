namespace SpiritIsland.Tests.Core;

public class ActionScope_Tests {

	[Fact]
	public async Task Original_IsRestored() {
		Guid childId; // grab later

		// Given: we know the default/original action scope
		Guid originalId = ActionScope.Current.Id;

		{
			// And: we create a child actions scope
			await using var childScope = new ActionScope(ActionCategory.Spirit_Power);
			childId = childScope.Id;

			// When: we cross the await boundary (causing a TaskLocal copy)
			await Task.Delay(1);


		}	// And: the childscope gets cleaned up/disposed of

		// Then: current scope should revert to original
		ActionScope.Current.Id.ShouldBe( originalId );

		//  And: not match the child scope
		ActionScope.Current.Id.ShouldNotBe( childId );

	}

	//[Fact]
	//public async Task Await_CausesThreadSwitch() {
	//	IsThread("A");
	//	await Task.Delay(1);
	//	IsThread("B");
	//}

	//[Fact] 
	//public async Task SkippingAwait_OnSameThread() {
	//	IsThread( "A" );
	//	bool b=false;
	//	if(b)
	//		await Task.Delay( 1 );
	//	IsThread( "A" );
	//}


	//[Fact]
	//public async Task StartOfAsyncMethod_OnCallerThread() {
	//	IsThread( "A" );

	//	await Child();
	//	async Task Child() {
	//		IsThread("A");
	//		await Task.Delay( 1 );
	//		IsThread("B");
	//	}
	//	// IsThread( "C" ); // Sometimes this is "B" and sometimes "C"
	//}

	//[Fact]
	//public async Task CompletedTask_SwitchesThreads() {
	//	IsThread( "A" );
	//	await Task.CompletedTask;
	//	IsThread( "A" );
	//}

	//[Fact]
	//public async Task AwaitingACompleteTask_SameThread() {
	//	IsThread( "A" );
	//	static Task Bob() => Task.CompletedTask;
	//	await Bob();
	//	IsThread( "A" );
	//}

	//[Fact]
	//public async Task AwaitingACompleteTask_SameThread2() {
	//	IsThread( "A" );
	//	Task t = Task.Delay(1);

	//	IsThread( "A" );
	//	System.Threading.Thread.Sleep(20);

	//	IsThread( "A" );

	//	await t;

	//	IsThread( "A" );
	//}

	//void IsThread(string name ) {
	//	int id = Environment.CurrentManagedThreadId;

	//	// find item that matches name OR id
	//	var (matchName,matchId) = _items.FirstOrDefault(x=>x.name==name||x.id==id);
	//	// If nothing, add it
	//	if(matchName == null && matchName == null) {
	//		_items.Add( (name, id) );
	//		return;
	//	}
	//	// Otherwise it better match this
	//	matchName.ShouldBe(name);
	//	matchId.ShouldBe(id);
	//}
	//readonly List<(string name,int id)> _items = new();

}