using System.Linq.Expressions;

namespace SpiritIsland.Tests;

public static class SpiritExtensions {

	#region Next Decision / Wait For Next

	/// <summary> Binds to the Next Decision </summary>
	internal static DecisionContext NextDecision( this Spirit spirit ) {
		spirit.WaitForNext();
		return new DecisionContext( spirit );
	}

	internal static void WaitForNext( this Spirit spirit ){
		if( !spirit.Portal.WaitForNext(defaultWaitMs) )
			throw new Exception($"Engine did not present Decision withing {defaultWaitMs}");

	}

	#endregion Next Decision / Wait For Next

	internal static SpiritConfigContext Configure( this Spirit spirit ) => new SpiritConfigContext( spirit );

	#region Given (spirit presence setup)

	internal static Spirit Given_IsOnMany( this Spirit spirit, string presenceString ) {

		Dictionary<string,Space> lookupByLabel = ActionScope.Current.Spaces_Unfiltered
			.ToDictionary(ss=>ss.SpaceSpec.Label,s=>s);

		Space[] spaces = new Space[presenceString.Length/2];
		for(int i=0;i*2<presenceString.Length;i++)
			spaces[i] = lookupByLabel[presenceString.Substring(i*2,2)];

		foreach(var space in spaces)
			spirit.Given_IsOn(space);

		return spirit;
	}

	/// <summary> Sets the # of Presence via .Init() </summary>
	internal static Spirit Given_IsOn( this Spirit spirit, SpaceSpec space, int count=1 )
		=> spirit.Given_IsOn( space.ScopeSpace, count );

	/// <summary> Sets the # of Presence via .Init() </summary>
	internal static Spirit Given_IsOn( this Spirit spirit, Space ss, int count=1 ){
		ss.Init( spirit.Presence.Token, count );
		return spirit;
	}

	#endregion Given (spirit presence setup)

	#region Given

	internal static void Given_HalfOfHandDiscarded( this Spirit spirit ) {
		for(int i=0;i<2;++i){
			int idx = spirit.Hand.Count-1;
			spirit.DiscardPile.Add( spirit.Hand[idx] );
			spirit.Hand.RemoveAt( idx );
		}
	}

	internal static PowerCard Given_PurchasedCard( this Spirit spirit, string cardName) {
		var card = spirit.Hand.Single( c => c.Title == cardName );
		spirit.PlayCard( card );
		return card;
	}


	#endregion Given

	#region When

	internal static Task When_Growing( this Spirit spirit, Action<VirtualUser> userActions ) {
		GameState gs = GameState.Current;
		gs.Phase = Phase.Growth;
		return spirit.DoGrowth( gs ).AwaitUser( userActions ).ShouldComplete("Growth");
	}

	internal static Task When_Growing( this Spirit spirit, int option, Action<VirtualUser> userActions ) {
		GameState gs = GameState.Current;
		gs.Phase = Phase.Growth;
		return Testing_GrowAndResolve( spirit, spirit.GrowthTrack.Groups[option], gs )
			.AwaitUser( userActions )
			.ShouldComplete( $"Growth option {option}" );
	}

	internal static Task When_ResolvingCard<T>( this Spirit spirit, Action<VirtualUser> userActions = null )
		=> spirit.ResolvePower( PowerCard.For(typeof(T)) ).AwaitUser( userActions ).ShouldComplete( typeof( T ).Name );

	internal static Task When_ResolvingInnate<T>( this Spirit spirit, Action<VirtualUser> userActions = null )
		=> spirit.ResolvePower( InnatePower.For(typeof(T)) ).AwaitUser( userActions ).ShouldComplete( typeof( T ).Name );

	internal static async Task ResolvePower( this Spirit spirit, IFlexibleSpeedActionFactory card ) {
		await using ActionScope scope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, spirit );
		scope.Owner = spirit;
		await card.ActivateAsync( spirit );
	}

	internal static void When_PlayingCards( this Spirit spirit, params PowerCard[] cards ){
		foreach(var card in cards)
			spirit.PlayCard( card );
	}

	internal static Task When_TargetingSpace( this Spirit spirit, SpaceSpec space, Action<TargetSpaceCtx> method )
		=> spirit.ResolvePowerOnSpaceAsync(space, method.AsAsync() )
			.ShouldComplete( method.Method.Name );

	internal static Task When_TargetingSpace( this Spirit spirit, SpaceSpec space, Func<TargetSpaceCtx,Task> methodAsync, Action<VirtualUser> userActions = null )
		=> spirit.ResolvePowerOnSpaceAsync( space, methodAsync )
			.AwaitUser( userActions )
			.ShouldComplete( methodAsync.Method.Name );

	/// <summary>
	/// Like playing a card, but user doesn't have to pick the space because it is passed in.
	/// </summary>
	internal static async Task ResolvePowerOnSpaceAsync( this Spirit spirit, SpaceSpec space, Func<TargetSpaceCtx, Task> func ) {
		await using ActionScope scope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, spirit );
		await func( spirit.Target( space ) );
	}

	#endregion When

	#region Await ...

	/// <summary>
	/// Wait for a VirtualUser to complete a series of action before returning the Task
	/// Version 2 - Virtual User is generated from Spirit
	/// </summary>
	internal static Task AwaitUser( this Task task, Action<VirtualUser> userActions ) {
		GameState.Current.Spirits[0].HandleDecisions(userActions)();
		return task;
	}

	#endregion Await ...

	#region Assert

	static public void Assert_HasCardAvailable( this Spirit spirit, string name ){
		bool nameMatches( PowerCard card ) => string.Compare(name,card.Title,true) == 0;
		Assert.True(spirit.Hand.Any( nameMatches ), $"Hand does not contain {name}.  Hand has "+spirit.Hand.Select(x=>x.Title).Join(",") );
	}

	static public void Assert_AllCardsAvailableToPlay(this Spirit spirit, int expectedAvailableCount = 4) {
		// Then: all cards reclaimed (including unplayed)
		Assert.Empty( spirit.DiscardPile ); // , "Should not be any cards in 'played' pile" );
		spirit.Hand.Count.ShouldBe( expectedAvailableCount );
	}

	static public void Assert_HasEnergy( this Spirit spirit, int expectedChange ) {
		spirit.Energy.ShouldBe( expectedChange );
	}

	static public void Assert_BoardPresenceIs( this Spirit spirit, string expected ) {
		var actual = ActionScope.Current.Spaces_Existing
			.Where( spirit.Presence.IsOn )
			.Select(s=>s.SpaceSpec.Label+":"+s[spirit.Presence.Token])
			.Order()
			.Join(",");
		Assert.Equal(expected, actual); // , Is.EqualTo(expected),"Presence in wrong place");
	}

	static public void Assert_CardIsReady( this Spirit spirit, PowerCard card, Phase speed ) {
		Assert.Contains(card, spirit.GetAvailableActions(speed).OfType<PowerCard>().ToList());
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

	#endregion Assert

	internal static void Given_SlotsRevealed( this IPresenceTrack track, int revealedSpaces ) {
		for(int i = 1; i < revealedSpaces; i++){
			Track location = track.RevealOptions.First(); 
			track.RevealAsync(location).Wait(); // $$$$
		}
	}

	static public TargetSpaceCtx TargetSpace( this Spirit self, string spaceLabel )
		=> self.Target(ActionScope.Current.Spaces_Unfiltered.First( s => s.SpaceSpec.Label == spaceLabel ) );

	/// <summary> Constructs a VirtualUser and passes it to userActions. </summary>
	internal static Action HandleDecisions(this Spirit spirit, Action<VirtualUser> userActions ) 
		=> userActions == null ? ()=>{ } : () => userActions( new VirtualUser( spirit ) );

	// !! Anything that needs to do this should use AsyncHandler<T>
	// UNLESS it is a method group, then we need to supply both versions OR explicitly Cast it to its delegat
	// internal static Func<T,Task> MakeAsync<T>(this Action<T> method) => (T t) => { method(t); return Task.CompletedTask; };

	static public async Task Testing_GrowAndResolve( Spirit spirit, GrowthGroup option, GameState gameState ) { // public for Testing

		await using var action = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Growth, spirit );

		// Auto run the auto-runs.
		foreach(var autoAction in option.AutoRuns)
			await autoAction.ActivateAsync( spirit );

		// If Option has only 1 Action, auto trigger it.
		if(option.UserRuns.Count() == 1) {
			await option.UserRuns.First().ActivateAsync( spirit );
		} else {
			foreach(IHelpGrow action2 in option.UserRuns)
				spirit.AddActionFactory( action2 );

			await spirit.ResolveActions( gameState );
		}
	}

}