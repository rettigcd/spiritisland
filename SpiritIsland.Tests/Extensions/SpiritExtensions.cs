namespace SpiritIsland.Tests;

public static class SpiritExtensions {

	/// <summary> Binds to the Next Decision </summary>
	internal static DecisionContext NextDecision( this Spirit spirit ) {
		spirit.WaitForNext();
		return new DecisionContext( spirit );
	}

	internal static void WaitForNext( this Spirit spirit ){
		const int ms = 10000;
		if( !spirit.Portal.WaitForNext(ms) )
			throw new Exception($"Engine did not present Decision withing {ms}");

	}

	internal static SpiritConfigContext Configure( this Spirit spirit ) => new SpiritConfigContext( spirit );

	#region Given

	internal static void Given_Adjust( this SpiritPresence presence, SpaceState space, int count ) => space.Adjust( presence.Token, count );

	internal static Spirit Given_HasTokensOn( this Spirit spirit, SpaceState spaceState, int count = 1 ) {
		spaceState.Init( spirit.Presence.Token, count );
		return spirit;
	}
	internal static Spirit Given_HasPresenceOn( this Spirit spirit, Space space, int count = 1 ) => spirit.Given_HasTokensOn( space.Tokens, count );

	#endregion Given

	#region When

	internal static Task When_Growing( this Spirit spirit, Action userActions ) {
		GameState gs = GameState.Current;
		gs.Phase = Phase.Growth;
		return spirit.DoGrowth( gs ).AwaitUser( userActions ).ShouldComplete("Growth");
	}

	internal static Task When_Growing( this Spirit spirit, int option, Action userActions ) {
		GameState gs = GameState.Current;
		gs.Phase = Phase.Growth;
		return Testing_GrowAndResolve( spirit, spirit.GrowthTrack.Options[option], gs )
			.AwaitUser( userActions )
			.ShouldComplete( $"Growth option {option}" );
	}

	internal static Task When_ResolvingCard<T>( this Spirit spirit, Action<VirtualUser> userActions = null )
		=> spirit.ResolvePower( PowerCard.For<T>() ).AwaitUser( spirit.HandleDecisions( userActions ) ).ShouldComplete( typeof( T ).Name );

	internal static Task When_ResolvingInnate<T>( this Spirit spirit, Action<VirtualUser> userActions = null )
		=> spirit.ResolvePower( InnatePower.For<T>() ).AwaitUser( spirit.HandleDecisions( userActions ) ).ShouldComplete( typeof( T ).Name );

	internal static async Task ResolvePower( this Spirit spirit, IFlexibleSpeedActionFactory card ) {
		await using ActionScope scope = await ActionScope.Start( ActionCategory.Spirit_Power );
		scope.Owner = spirit;
		SelfCtx selfCtx = spirit.BindMyPowers();
		await card.ActivateAsync( selfCtx );
	}

	internal static Task When_TargetingSpace( this Spirit spirit, Space space, Action<TargetSpaceCtx> method )
		=> spirit.ResolvePowerOnSpaceAsync(space, method ).ShouldComplete( method.Method.Name );

	internal static Task When_TargetingSpace( this Spirit spirit, Space space, Func<TargetSpaceCtx,Task> methodAsync, Action<VirtualUser> userActions = null )
		=> spirit.ResolvePowerOnSpaceAsync( space, methodAsync )
			.AwaitUser( spirit.HandleDecisions( userActions ) )
			.ShouldComplete( methodAsync.Method.Name );

	internal static async Task ResolvePowerOnSpaceAsync( this Spirit spirit, Space space, AsyncHandler<TargetSpaceCtx> methodAsync ) {
		await using ActionScope scope = await ActionScope.Start( ActionCategory.Spirit_Power );
		scope.Owner = spirit;
		await methodAsync.Execute( spirit.BindMyPowers().Target( space ) );
	}

	internal static Task AwaitUserToComplete( this Task task, string taskDescription, Action userActions )
		=> task.AwaitUser(userActions).ShouldComplete(taskDescription);

	internal static Task AwaitUser( this Task task, Action userActions ) {
		userActions?.Invoke();
		return task;
	}

	internal static Task AwaitUser( this Task task, Spirit spirit, Action<VirtualUser> userActions ) {
		spirit.HandleDecisions(userActions)();
		return task;
	}


	const int defaultWaitMs = 3000;
	internal static async Task ShouldComplete( this Task task, string taskDescription = "[Task]", int ms = defaultWaitMs ) {
		TimeSpan waitTime = TimeSpan.FromMilliseconds( ms );
		try {
			await task.WaitAsync( waitTime );
		}catch(TimeoutException ex) {
			throw new Exception($"Operation {taskDescription} did not complete within {ms}mS.",ex);
		}
		if(task.IsCompletedSuccessfully) return;
		if(task.Exception != null)
			throw new Exception("Task through exception.", task.Exception);
		throw new Exception( $"{taskDescription} did not complete in {waitTime}" );
	}

	/// <summary> Constructs a VirtualUser and passes it to userActions. </summary>
	internal static Action HandleDecisions(this Spirit spirit, Action<VirtualUser> userActions ) 
		=> userActions == null ? ()=>{ } : () => userActions( new VirtualUser( spirit ) );

	// !! Anything that needs to do this should use AsyncHandler<T>
	// UNLESS it is a method group, then we need to supply both versions OR explicitly Cast it to its delegat
	// internal static Func<T,Task> MakeAsync<T>(this Action<T> method) => (T t) => { method(t); return Task.CompletedTask; };

	#endregion When

	static public async Task Testing_GrowAndResolve( Spirit spirit, GrowthOption option, GameState gameState ) { // public for Testing

		await using var action = await ActionScope.Start( ActionCategory.Spirit_Growth );
		var ctx = spirit.BindSelf();

		// Auto run the auto-runs.
		foreach(var autoAction in option.AutoRuns)
			await autoAction.ActivateAsync( ctx );

		// If Option has only 1 Action, auto trigger it.
		if(option.UserRuns.Count() == 1) {
			await option.UserRuns.First().ActivateAsync( ctx );
		} else {
			foreach(IHelpGrow action2 in option.UserRuns)
				spirit.AddActionFactory( action2 );

			await spirit.ResolveActions( gameState );
		}
	}

}

class AsyncHandler<T> {

	public AsyncHandler(Action<T> action) { _action=action; }
	public static implicit operator AsyncHandler<T>( Action<T> action ) => new AsyncHandler<T>( action );

	public AsyncHandler( Func<T,Task> task ) { _task=task; }
	public static implicit operator AsyncHandler<T>( Func<T,Task> task ) => new AsyncHandler<T>( task );

	public Task Execute( T t ) { if(_task != null) return _task( t ); _action( t ); return Task.CompletedTask; }
	readonly Action<T> _action;
	readonly Func<T, Task> _task;

}