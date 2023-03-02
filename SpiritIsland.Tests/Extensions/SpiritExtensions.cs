using SpiritIsland.Select;

namespace SpiritIsland.Tests;

public static class SpiritExtensions {

	/// <summary> Binds to the Next Decision </summary>
	internal static DecisionContext NextDecision( this Spirit spirit ) {
		spirit.WaitForNext();
		return new DecisionContext( spirit );
	}

	internal static void WaitForNext( this Spirit spirit ){
		const int ms = 10000;
		if( !spirit.Gateway.WaitForNext(ms) )
			throw new Exception($"Engine did not present Decision withing {ms}");

	}

	internal static SpiritConfigContext Configure( this Spirit spirit ) => new SpiritConfigContext( spirit );

	internal static void Given_Adjust( this SpiritPresence presence, SpaceState space, int count ) => space.Adjust( presence.Token, count );

	internal static void When_Growing(this Spirit spirit, Action userActions) {
		static async Task Wrapper(Spirit spirit) {
			GameState gs = GameState.Current;
			gs.Phase = Phase.Growth;
			await spirit.DoGrowth( gs );
		}
		Wrapper(spirit)
			.FinishUp( "Growth", userActions );
	}

	internal static void When_Growing( this Spirit spirit, int option, Action userActions ) {
		var gs = GameState.Current;
		gs.Phase = Phase.Growth;
		spirit.GrowAndResolve( spirit.GrowthTrack.Options[option], gs )
			.FinishUp($"Growth option {option}", userActions);
	}

	internal static async Task ResolvePower( this Spirit spirit, IFlexibleSpeedActionFactory card ) {
		await using ActionScope scope = await ActionScope.Start( ActionCategory.Spirit_Power );
		scope.Owner = spirit;
		SelfCtx selfCtx = spirit.BindMyPowers();
		await card.ActivateAsync( selfCtx );
	}

	internal static void When_ResolvingCard<T>( this Spirit spirit, Action userActions = null ) {
		spirit.ResolvePower( PowerCard.For<T>() ).FinishUp( typeof( T ).Name, userActions );
	}

	internal static void When_ResolvingInnate<T>( this Spirit spirit, Action userActions = null ) {
		spirit.ResolvePower( InnatePower.For<T>() ).FinishUp( typeof( T ).Name, userActions );
	}


	internal static void When_TargetingSpace( this Spirit spirit, Space space, Action<TargetSpaceCtx> method, Action<VirtualUser> userActions = null )
		=> spirit.When_TargetingSpace( space, ctx => { method(ctx); return Task.CompletedTask;}, userActions );

	internal static void When_TargetingSpace( this Spirit spirit, Space space, Func<TargetSpaceCtx,Task> methodAsync, Action<VirtualUser> userActions = null ) {
		static async Task ScopeWrapper( Func<TargetSpaceCtx, Task> methodAsync, Spirit spirit, Space space ) {
			await using ActionScope scope = await ActionScope.Start( ActionCategory.Spirit_Power );
			scope.Owner = spirit;
			await methodAsync( spirit.BindMyPowers().Target( space ) );
		}
		ScopeWrapper( methodAsync, spirit, space ).FinishUp( "Executing Unknown Method", userActions == null ? null : ()=>userActions(new VirtualUser(spirit)) );
	}


	internal static Spirit Given_HasTokensOn( this Spirit spirit, SpaceState spaceState, int count=1 ) { 
		spaceState.Init(spirit.Token,count);
		return spirit;
	}
	internal static Spirit Given_HasPresenceOn( this Spirit spirit, Space space, int count=1 ) => spirit.Given_HasTokensOn(space.Tokens, count );

}
