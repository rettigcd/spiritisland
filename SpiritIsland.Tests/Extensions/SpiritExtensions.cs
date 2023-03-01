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

	internal static void Adjust( this SpiritPresence presence, SpaceState space, int count ) => space.Adjust( presence.Token, count );

	internal static void When_Growing(this Spirit spirit, Action userActions) {
		var gs = GameState.Current;
		gs.Phase = Phase.Growth;
		spirit.DoGrowth( gs )
			.FinishUp("Growth", userActions);
	}

	internal static void When_Growing( this Spirit spirit, int option, Action userActions ) {
		var gs = GameState.Current;
		gs.Phase = Phase.Growth;
		spirit.GrowAndResolve( spirit.GrowthTrack.Options[option], gs )
			.FinishUp($"Growth option {option}", userActions);
	}

	internal static void When_ResolvingCard<T>( this Spirit spirit, Action userActions = null ) {
		static async Task ScopeWrapper( PowerCard card, Spirit spirit ) {
			await using ActionScope scope = await ActionScope.Start( ActionCategory.Spirit_Power );
			SelfCtx selfCtx = spirit.BindMyPowers();
			await card.ActivateAsync( selfCtx );
		}
		ScopeWrapper( PowerCard.For<T>(), spirit ).FinishUp( typeof( T ).Name, userActions );
	}


}
