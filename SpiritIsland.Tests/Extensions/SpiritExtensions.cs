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
		Task growthTask = spirit.DoGrowth( gs );
		userActions();
		growthTask.Wait( 3000 );
		growthTask.IsCompletedSuccessfully.ShouldBeTrue( "Growth task did not complete in aloted time." );
	}

	internal static void When_Growing( this Spirit spirit, int option, Action userActions ) {
		var gs = GameState.Current;
		gs.Phase = Phase.Growth;
		Task t = spirit.GrowAndResolve( spirit.GrowthTrack.Options[option], gs );
		userActions();
		t.Wait( 3000 );
		if(!t.IsCompletedSuccessfully)
			throw new Exception( $"Growth option {option} did not complete in a timely manner." );
	}

}
